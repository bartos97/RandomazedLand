using System;
using UnityEngine;
using System.Threading;
using System.Collections.Concurrent;
using TerrainGeneration.Structs;

namespace TerrainGeneration
{   
    public class MapGenerator : MonoBehaviour
    {
        [Header("Gameplay stuff")]
        public Transform playerObject;        

        [Header("Noise settings")]
        public int seed = 0;
        public NoiseParams noiseParameters = NoiseParams.CreateWithDefaults();

        [Header("Terrain settings")]
        [Min(1)]
        public Material meshMaterial;
        public TerrainParams terrainParameters = new TerrainParams();

        [Header("Mesh preview")]
        public GameObject previewMesh;
        public MeshFilter previewMeshFilter;
        public MeshRenderer previewMeshRenderer;
        public float offsetX = 0f;
        public float offsetY = 0f;
        public LevelOfDetail LevelOfDetail = LevelOfDetail._1;
        public Utils.NormalizationType normalization;
        public bool AutoUpdatePreview = false;

        //Number of vertices in one dimension of map
        private const int MapChunkVerticesCount = LevelOfDetailConfig.chunkSize + 1;

        private readonly ConcurrentQueue<MapThreadData<float[]>> noiseMapQueue = new ConcurrentQueue<MapThreadData<float[]>>();
        private readonly ConcurrentQueue<MapThreadData<MeshData>> meshQueue = new ConcurrentQueue<MapThreadData<MeshData>>();

        private readonly InfiniteTerrain infiniteTerrain;

        public MapGenerator()
        {
            infiniteTerrain = new InfiniteTerrain(this);
        }

        public void Start()
        {
            previewMesh.SetActive(false);
            seed = seed == 0 ? GetRandomSeed() : seed;
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
            float[] noiseMap = Utils.NoiseMapGenerator.GenerateMap(MapChunkVerticesCount, noiseParameters, offsetX, offsetY, seed == 0 ? GetRandomSeed() : seed, normalization);
            var meshData = Utils.MeshGenerator.GenerateFromHeightMap(noiseMap, MapChunkVerticesCount, terrainParameters, (int)LevelOfDetail);
            previewMeshFilter.sharedMesh.Clear();
            previewMeshFilter.sharedMesh = meshData.Create();
            previewMesh.transform.localScale = Vector3.one * terrainParameters.UniformScaleMultiplier;
        }

        public void RequestNoiseMap(Action<float[]> callback, float offsetX, float offsetY)
        {
            var th = new Thread(() =>
            {
                float[] noiseMap = Utils.NoiseMapGenerator.GenerateMap(MapChunkVerticesCount, noiseParameters, offsetX, offsetY, seed);
                noiseMapQueue.Enqueue(new MapThreadData<float[]>(callback, noiseMap));
            });

            th.Start();
        }

        public void RequestMeshData(Action<MeshData> callback, float[] noiseMap, LevelOfDetail lod)
        {
            var th = new Thread(() =>
            {
                var meshData = Utils.MeshGenerator.GenerateFromHeightMap(noiseMap, MapChunkVerticesCount, terrainParameters, (int)lod);
                meshQueue.Enqueue(new MapThreadData<MeshData>(callback, meshData));
            });

            th.Start();
        }

        private int GetRandomSeed()
        {
            return new System.Random().Next();
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
