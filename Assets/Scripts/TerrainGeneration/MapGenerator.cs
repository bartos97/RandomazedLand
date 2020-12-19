using System;
using UnityEngine;
using System.Threading;
using System.Collections.Concurrent;
using TerrainGeneration.ScriptableObjects;
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
        [Header("Gameplay stuff")]
        public Transform playerObject;

        [Header("Terrain generation")]
        public Material meshMaterial;
        public NoiseParams noiseParams;
        public TerrainParams terrainParams;
        public bool useFalloffMap;

        [Header("Preview settings")]
        public DisplayType displayType = DisplayType.Mesh;
        public GameObject previewMesh;
        public GameObject previewTexture;
        public float offsetX = 0f;
        public float offsetY = 0f;
        public LevelOfDetail LevelOfDetail = LevelOfDetail._1;
        public NormalizationType normalization;
        public bool autoUpdatePreview;

        //Number of vertices in one dimension of map
        private const int mapChunkVerticesPerLine = InfiniteTerrainConfig.chunkSize + 1;
        private const int mapChunkVerticesPerLineWithBorder = mapChunkVerticesPerLine + 2;
        private readonly InfiniteTerrain infiniteTerrain;
        private readonly System.Random prng;
        private readonly ConcurrentQueue<MapThreadData<float[]>> noiseMapQueue;
        private readonly ConcurrentQueue<MapThreadData<MeshData>> meshQueue;
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
            meshQueue = new ConcurrentQueue<MapThreadData<MeshData>>();
            falloffMap = new FalloffMap(mapChunkVerticesPerLineWithBorder);
        }

        private void Start()
        {
            previewMesh.SetActive(false);
            previewTexture.SetActive(false);
            seed = noiseParams.seed == 0 ? prng.Next() : noiseParams.seed;

            infiniteTerrain.OnStart();
        }

        private void Update()
        {
            while (noiseMapQueue.TryDequeue(out MapThreadData<float[]> threadData))
                threadData.callback(threadData.parameter);

            while (meshQueue.TryDequeue(out MapThreadData<MeshData> threadData))
                threadData.callback(threadData.parameter);

            infiniteTerrain.OnUpdate();
        }

        private void OnValidate()
        {
            if (terrainParams != null)
            {
                terrainParams.ValuesUpdated -= GeneratePreview;
                terrainParams.ValuesUpdated += GeneratePreview;
            }
            if (noiseParams != null)
            {
                noiseParams.ValuesUpdated -= GeneratePreview;
                noiseParams.ValuesUpdated += GeneratePreview;
            }
        }

        public void RequestNoiseMap(Action<float[]> callback, float offsetX, float offsetY, Vector2 gridCoords)
        {
            var th = new Thread(() =>
            {
                float[] noiseMap = NoiseMapGenerator.GenerateFromPerlinNoise(mapChunkVerticesPerLineWithBorder, noiseParams, offsetX, offsetY, seed);

                if (useFalloffMap)
                {
                    int gridX = (int)gridCoords.x;
                    int gridY = (int)gridCoords.y;
                    bool isBorderChunk = Mathf.Abs(gridX) == InfiniteTerrainConfig.maxGridCoord || Mathf.Abs(gridY) == InfiniteTerrainConfig.maxGridCoord;
                    //Debug.Log($"border: {isBorderChunk}; {gridX}; {gridY}");

                    if (isBorderChunk)
                    {
                        float[] falloff = GetFalloffMapForBorderChunk(gridX, gridY);
                        for (int i = 0; i < (mapChunkVerticesPerLineWithBorder) * (mapChunkVerticesPerLineWithBorder); i++)
                        {
                            noiseMap[i] = Mathf.Clamp01(noiseMap[i] - falloff[i]);
                        }
                    }
                }

                noiseMapQueue.Enqueue(new MapThreadData<float[]>(callback, noiseMap));
            });

            th.Start();
        }

        private float[] GetFalloffMapForBorderChunk(int gridX, int gridY)
        {
            bool isTop = gridY == InfiniteTerrainConfig.maxGridCoord;
            bool isBottom = gridY == -InfiniteTerrainConfig.maxGridCoord;
            bool isRight = gridX == InfiniteTerrainConfig.maxGridCoord;
            bool isLeft = gridX == -InfiniteTerrainConfig.maxGridCoord;
            
            if (isTop)
            {
                if (isLeft)
                    return falloffMap.TopLeftChunk;
                if (isRight)
                    return falloffMap.TopRightChunk;
                return falloffMap.TopMiddleChunk;
            }
            else if (isBottom)
            {
                if (isLeft)
                    return falloffMap.BottomLeftChunk;
                if (isRight)
                    return falloffMap.BottomRightChunk;
                return falloffMap.BottomMiddleChunk;
            }
            else
            {
                if (isLeft)
                    return falloffMap.MiddleLeftChunk;
                if (isRight)
                    return falloffMap.MiddleRightChunk;
            }

            throw new Exception($"You fucked up; gridX = {gridX}; gridY = {gridY}");
        }

        public void RequestMeshData(Action<MeshData> callback, float[] noiseMap, LevelOfDetail lod)
        {
            var th = new Thread(() =>
            {
                var meshData = MeshGenerator.GenerateFromNoiseMap(noiseMap, mapChunkVerticesPerLine, terrainParams, (int)lod);
                meshQueue.Enqueue(new MapThreadData<MeshData>(callback, meshData));
            });

            th.Start();
        }

        public void GeneratePreview()
        {
            var noiseMap = NoiseMapGenerator.GenerateFromPerlinNoise(mapChunkVerticesPerLineWithBorder, noiseParams, offsetX, offsetY, noiseParams.seed == 0 ? prng.Next() : noiseParams.seed, normalization);

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
                    DisplayTexturePreview(falloffMap.CombinedMap); 
                    break;
            }
        }

        private void DisplayMeshPreview(float[] noiseMap)
        {
            if (useFalloffMap)
            {
                for (int i = 0; i < (mapChunkVerticesPerLineWithBorder) * (mapChunkVerticesPerLineWithBorder); i++)
                {
                    noiseMap[i] = Mathf.Clamp01(noiseMap[i] - falloffMap.CombinedMap[i]);
                }
            }

            var meshData = MeshGenerator.GenerateFromNoiseMap(noiseMap, mapChunkVerticesPerLine, terrainParams, (int)LevelOfDetail);
            var previewMeshFilter = previewMesh.GetComponent<MeshFilter>();
            previewMeshFilter.sharedMesh.Clear();
            previewMeshFilter.sharedMesh = meshData.GetUnityMesh();
            previewMesh.transform.localScale = Vector3.one * terrainParams.UniformScaleMultiplier;
        }

        private void DisplayTexturePreview(float[] noiseMap)
        {
            var tex = TextureGenerator.GenerateFromHeightMap(noiseMap, mapChunkVerticesPerLineWithBorder, mapChunkVerticesPerLineWithBorder);
            var texRenderer = previewTexture.GetComponent<MeshRenderer>();
            texRenderer.sharedMaterial.mainTexture = tex;
        }
    }
}
