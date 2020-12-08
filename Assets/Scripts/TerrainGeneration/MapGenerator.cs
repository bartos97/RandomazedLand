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

        [Header("Noise settings")]
        public NoiseParams noiseParams;

        [Header("Terrain settings")]
        public Material meshMaterial;
        public TerrainParams terrainParams;

        [Header("Preview settings")]
        public DisplayType displayType = DisplayType.Mesh;
        public GameObject previewMesh;
        public GameObject previewTexture;
        public float offsetX = 0f;
        public float offsetY = 0f;
        public LevelOfDetail LevelOfDetail = LevelOfDetail._1;
        public Utils.NormalizationType normalization;
        public bool autoUpdatePreview = false;

        //Number of vertices in one dimension of map
        private const int mapChunkVerticesPerLine = LevelOfDetailConfig.chunkSize + 1;

        private readonly ConcurrentQueue<MapThreadData<float[]>> noiseMapQueue = new ConcurrentQueue<MapThreadData<float[]>>();
        private readonly ConcurrentQueue<MapThreadData<MeshData>> meshQueue = new ConcurrentQueue<MapThreadData<MeshData>>();

        private InfiniteTerrain infiniteTerrain;
        private readonly System.Random prng;
        private readonly float[] falloffMap;

        public MapGenerator()
        {
            prng = new System.Random();
            falloffMap = Utils.NoiseMapGenerator.GenerateFalloffMap(mapChunkVerticesPerLine + 2);
        }

        public void Start()
        {
            previewMesh.SetActive(false);
            previewTexture.SetActive(false);
            infiniteTerrain = new InfiniteTerrain(this);
            if (noiseParams.seed == 0)
                noiseParams.seed = prng.Next();
        }

        public void Update()
        {
            while (noiseMapQueue.TryDequeue(out MapThreadData<float[]> threadData))
                threadData.callback(threadData.parameter);

            while (meshQueue.TryDequeue(out MapThreadData<MeshData> threadData))
                threadData.callback(threadData.parameter);

            infiniteTerrain.OnUpdate();
        }

        public void GeneratePreview()
        {
            var noiseMap = Utils.NoiseMapGenerator.GenerateFromPerlinNoise(mapChunkVerticesPerLine + 2, noiseParams, offsetX, offsetY, noiseParams.seed == 0 ? prng.Next() : noiseParams.seed, normalization);

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
                    DisplayTexturePreview(falloffMap); 
                    break;
            }
        }

        private void DisplayMeshPreview(float[] noiseMap)
        {
            if (terrainParams.useFalloffMap)
            {
                for (int i = 0; i < (mapChunkVerticesPerLine + 2) * (mapChunkVerticesPerLine + 2); i++)
                {
                    noiseMap[i] = Mathf.Clamp01(noiseMap[i] - falloffMap[i]);
                }
            }

            var meshData = Utils.MeshGenerator.GenerateFromNoiseMap(noiseMap, mapChunkVerticesPerLine, terrainParams, (int)LevelOfDetail);
            var previewMeshFilter = previewMesh.GetComponent<MeshFilter>();
            previewMeshFilter.sharedMesh.Clear();
            previewMeshFilter.sharedMesh = meshData.GetUnityMesh();
            previewMesh.transform.localScale = Vector3.one * terrainParams.UniformScaleMultiplier;
        }

        private void DisplayTexturePreview(float[] noiseMap)
        {
            var tex = Utils.TextureGenerator.GenerateFromHeightMap(noiseMap, mapChunkVerticesPerLine + 2, mapChunkVerticesPerLine + 2);
            var texRenderer = previewTexture.GetComponent<MeshRenderer>();
            texRenderer.sharedMaterial.mainTexture = tex;
        }

        public void RequestNoiseMap(Action<float[]> callback, float offsetX, float offsetY)
        {
            var th = new Thread(() =>
            {
                float[] noiseMap = Utils.NoiseMapGenerator.GenerateFromPerlinNoise(mapChunkVerticesPerLine + 2, noiseParams, offsetX, offsetY, noiseParams.seed);
                if (terrainParams.useFalloffMap)
                {
                    for (int i = 0; i < (mapChunkVerticesPerLine + 2) * (mapChunkVerticesPerLine + 2); i++)
                    {
                        noiseMap[i] = Mathf.Clamp01(noiseMap[i] - falloffMap[i]);
                    }
                }
                noiseMapQueue.Enqueue(new MapThreadData<float[]>(callback, noiseMap));
            });

            th.Start();
        }

        public void RequestMeshData(Action<MeshData> callback, float[] noiseMap, LevelOfDetail lod)
        {
            var th = new Thread(() =>
            {
                var meshData = Utils.MeshGenerator.GenerateFromNoiseMap(noiseMap, mapChunkVerticesPerLine, terrainParams, (int)lod);
                meshQueue.Enqueue(new MapThreadData<MeshData>(callback, meshData));
            });

            th.Start();
        }

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
    }
}
