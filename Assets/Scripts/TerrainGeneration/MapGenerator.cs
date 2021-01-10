using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Threading;
using System.Collections.Concurrent;
using System.Linq;
using Data;
using Data.ScriptableObjects;
using TerrainGeneration.Utils;

namespace TerrainGeneration
{   
    public enum DisplayType
    {
        Mesh,
        NoiseMap,
        FalloffMap
    }

    public class MapGenerator : MonoBehaviour
    {
        public Transform playerObject;
        public Material meshMaterial;
        public Light sunlight;
        public TerrainParams[] terrains;
        public TerrainParams ActiveTerrainParams { get; private set; }

        [Header("Preview settings")]
        public GameObject previewMesh;
        public MeshFilter previewMeshFilter;
        public GameObject previewTexture;
        public DisplayType displayType = DisplayType.Mesh;
        public LevelOfDetail LevelOfDetail = LevelOfDetail._1;
        public NormalizationType normalization;
        public BorderChunkType borderFalloffType;
        public float offsetX = 0f;
        public float offsetY = 0f;
        public bool autoUpdatePreview;

        //Number of vertices in one dimension of map
        public const int mapChunkVerticesPerLine = InfiniteTerrainConfig.chunkSize + 1;
        public const int mapChunkVerticesPerLineWithBorder = mapChunkVerticesPerLine + 2;

        private readonly InfiniteTerrain infiniteTerrain;
        private readonly System.Random prng;
        private readonly ConcurrentQueue<MapThreadData<float[]>> noiseMapQueue;
        private readonly ConcurrentQueue<MapThreadData<MeshDataWrapper>> meshQueue;
        private readonly FalloffMap falloffMap;
        private int seed;

        private struct MapThreadData<T>
        {
            public readonly Action<T> callback;
            public readonly T parameter;

            public MapThreadData(Action<T> callback, T parameter)
            {
                this.callback = callback;
                this.parameter = parameter;
            }
        }

        private MapGenerator()
        {
            prng = new System.Random();
            infiniteTerrain = new InfiniteTerrain(this);
            noiseMapQueue = new ConcurrentQueue<MapThreadData<float[]>>();
            meshQueue = new ConcurrentQueue<MapThreadData<MeshDataWrapper>>();
            falloffMap = new FalloffMap(mapChunkVerticesPerLineWithBorder);
        }

    #region UnityMethods
        private void Start()
        {
            bool newGame = true;

            if (PlayerPrefs.HasKey(Config.PrefKeyContiuneGameFlag) && PlayerPrefs.HasKey(Config.PrefKeyGameSave))
            {
                if (PlayerPrefs.GetInt(Config.PrefKeyContiuneGameFlag) == 1)
                {
                    newGame = false;
                    string json = PlayerPrefs.GetString(Config.PrefKeyGameSave);
                    if (!string.IsNullOrEmpty(json))
                    {
                        var saveData = JsonUtility.FromJson<GameSaveData>(json);
                        seed = saveData.seed;
                        playerObject.position = new Vector3(saveData.playerPos[0], saveData.playerPos[1], saveData.playerPos[2]);

                        foreach (var terrain in terrains)
                        {
                            terrain.isActive = false;
                        }
                        ActiveTerrainParams = terrains.First(x => x.terrainName == saveData.terrainName);
                        ActiveTerrainParams.isActive = true;
                    }
                }
            }

            if (newGame)
            {
                ActiveTerrainParams = terrains.First(x => x.isActive);
                seed = ActiveTerrainParams.noiseParams.seed == 0 ? prng.Next() : ActiveTerrainParams.noiseParams.seed;
            }

            previewMesh.SetActive(false);
            previewTexture.SetActive(false);
            InitLightingFromParams();
            infiniteTerrain.OnStart();
        }

        private void Update()
        {
            while (noiseMapQueue.TryDequeue(out MapThreadData<float[]> threadData))
                threadData.callback(threadData.parameter);

            while (meshQueue.TryDequeue(out MapThreadData<MeshDataWrapper> threadData))
                threadData.callback(threadData.parameter);

            infiniteTerrain.OnUpdate();
        }

        private void OnValidate()
        {
            if (Application.isEditor)
            {
                try
                {
                    ActiveTerrainParams = terrains.First(x => x.isActive);
                }
                catch (Exception e)
                {
                    Debug.LogWarning(e.ToString());
                    ActiveTerrainParams = terrains.First();
                }

                foreach (var param in terrains)
                {
                    param.ValuesUpdated -= GeneratePreview;
                    param.ValuesUpdated += GeneratePreview;
                }
            }

        }
    #endregion

        public GameSaveData GetSaveData()
        {
            return new GameSaveData
            {
                seed = seed,
                terrainName = ActiveTerrainParams.terrainName,
                playerPos = new float[]
                {
                    playerObject.position.x,
                    playerObject.position.y + 5f,
                    playerObject.position.z
                }
            };
        }

        public void RequestNoiseMap(Action<float[]> callback, float offsetX, float offsetY, Vector2 gridCoords, BorderChunkType borderType = BorderChunkType.Invalid)
        {
            var th = new Thread(() =>
            {
                float[] noiseMap = NoiseGenerator.GenerateFromPerlinNoise(mapChunkVerticesPerLineWithBorder, ActiveTerrainParams.noiseParams, offsetX, offsetY, seed);

                if (ActiveTerrainParams.useFalloffMap && borderType != BorderChunkType.Invalid)
                {
                    float[] falloff = falloffMap.GetChunk(borderType);
                    for (int i = 0; i < (mapChunkVerticesPerLineWithBorder) * (mapChunkVerticesPerLineWithBorder); i++)
                    {
                        noiseMap[i] = Mathf.Clamp01(noiseMap[i] - falloff[i]);
                    }
                }

                noiseMapQueue.Enqueue(new MapThreadData<float[]>(callback, noiseMap));
            });

            th.Start();
        }

        public void RequestMeshData(Action<MeshDataWrapper> callback, float[] noiseMap, LevelOfDetail lod)
        {
            var th = new Thread(() =>
            {
                var meshData = MeshGenerator.GenerateFromNoiseMap(noiseMap, mapChunkVerticesPerLine, ActiveTerrainParams, (int)lod);
                meshQueue.Enqueue(new MapThreadData<MeshDataWrapper>(callback, meshData));
            });

            th.Start();
        }

        public void GeneratePreview()
        {
            OnValidate();
            InitLightingFromParams();
            var noiseMap = NoiseGenerator.GenerateFromPerlinNoise(
                mapChunkVerticesPerLineWithBorder, 
                ActiveTerrainParams.noiseParams, 
                offsetX, offsetY, 
                ActiveTerrainParams.noiseParams.seed == 0 ? prng.Next() : ActiveTerrainParams.noiseParams.seed,
                normalization);

            switch (displayType)
            {
                default:
                case DisplayType.Mesh: 
                    DisplayMeshPreview(noiseMap);
                    break;
                case DisplayType.NoiseMap:
                    DisplayTexturePreview(noiseMap); 
                    break;
                case DisplayType.FalloffMap:
                    DisplayTexturePreview(borderFalloffType == BorderChunkType.Invalid ? falloffMap.CombinedMap : falloffMap.GetChunk(borderFalloffType)); 
                    break;
            }
        }

        private void DisplayMeshPreview(float[] noiseMap)
        {
            if (ActiveTerrainParams.useFalloffMap)
            {
                float[] falloff = borderFalloffType == BorderChunkType.Invalid ? falloffMap.CombinedMap : falloffMap.GetChunk(borderFalloffType);
                for (int i = 0; i < (mapChunkVerticesPerLineWithBorder) * (mapChunkVerticesPerLineWithBorder); i++)
                {
                    noiseMap[i] = Mathf.Clamp01(noiseMap[i] - falloff[i]);
                }
            }

            var meshData = MeshGenerator.GenerateFromNoiseMap(noiseMap, mapChunkVerticesPerLine, ActiveTerrainParams, (int)LevelOfDetail);
            previewMeshFilter.sharedMesh.Clear();
            previewMeshFilter.sharedMesh = meshData.GetUnityMesh();
            previewMesh.transform.localScale = Vector3.one * ActiveTerrainParams.UniformScaleMultiplier;
        }

        private void DisplayTexturePreview(float[] noiseMap)
        {
            var tex = TextureGenerator.GenerateFromHeightMap(noiseMap, mapChunkVerticesPerLineWithBorder, mapChunkVerticesPerLineWithBorder);
            var texRenderer = previewTexture.GetComponent<MeshRenderer>();
            texRenderer.sharedMaterial.mainTexture = tex;
        }

        private void InitLightingFromParams()
        {
            RenderSettings.fog = ActiveTerrainParams.fogEnabled;
            RenderSettings.fogDensity = ActiveTerrainParams.fogDensity;
            RenderSettings.fogColor = ActiveTerrainParams.fogColor;
            RenderSettings.skybox = ActiveTerrainParams.skybox;
            sunlight.color = ActiveTerrainParams.sunlightColor;
            sunlight.intensity = ActiveTerrainParams.sunlightIntensity;
        }
    }
}
