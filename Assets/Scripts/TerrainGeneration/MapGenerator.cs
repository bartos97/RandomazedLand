using System;
using UnityEngine;
using DataStructures;
using System.Threading;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace TerrainGeneration
{
    public enum LevelOfDetail
    {
        _1 = 1,
        _2 = 2,
        _4 = 4,
        _6 = 6,
        _8 = 8
    }

    public class MapGenerator : MonoBehaviour
    {
        //Number of vertices in one dimension of map
        public const int MapChunkVerticesCount = 241;

        [Header("Map settings")]
        public float offsetX = 0f;
        public float offsetY = 0f;
        [Tooltip("Mesh simplification increment")]
        public LevelOfDetail LevelOfDetail = LevelOfDetail._1;

        [Header("Noise settings")]
        public int seed = 0;
        public NoiseParams noiseParams = NoiseParams.CreateWithDefaults();

        [Header("Terrain settings")]
        [Min(1)]
        public int TerrainDepthMultiplier = 20;
        public Gradient TerrainRegions;
        public AnimationCurve MeshHeightCurve;

        [Header("Mesh preview")]
        public GameObject previewMesh;
        public MeshFilter previewMeshFilter;
        public MeshRenderer previewMeshRenderer;
        public bool AutoUpdatePreview = false;

        private readonly ConcurrentQueue<MapThreadData<float[]>> noiseMapQueue = new ConcurrentQueue<MapThreadData<float[]>>();
        private readonly ConcurrentQueue<MapThreadData<MeshData>> meshQueue = new ConcurrentQueue<MapThreadData<MeshData>>();

        public void Start()
        {
            previewMesh.SetActive(false);
        }

        public void Update()
        {
            while (noiseMapQueue.TryDequeue(out MapThreadData<float[]> threadData))
                threadData.callback(threadData.parameter);

            while (meshQueue.TryDequeue(out MapThreadData<MeshData> threadData))
                threadData.callback(threadData.parameter);
        }

        public void GeneratePreview()
        {
            float[] noiseMap = Utils.NoiseMapGenerator.GenerateMap(MapChunkVerticesCount, MapChunkVerticesCount, noiseParams, offsetX, offsetY, seed);
            var meshData = Utils.MeshGenerator.GenerateFromHeightMap(noiseMap, MapChunkVerticesCount, TerrainDepthMultiplier, MeshHeightCurve, (int)LevelOfDetail, TerrainRegions);
            previewMeshFilter.sharedMesh.Clear();
            previewMeshFilter.sharedMesh = meshData.Create();
        }

        public void RequestNoiseMap(Action<float[]> callback, float offsetX, float offsetY)
        {
            var th = new Thread(() =>
            {
                float[] noiseMap = Utils.NoiseMapGenerator.GenerateMap(MapChunkVerticesCount, MapChunkVerticesCount, noiseParams, offsetX, offsetY, seed);
                noiseMapQueue.Enqueue(new MapThreadData<float[]>(callback, noiseMap));
            });

            th.Start();
        }

        public void RequestMeshData(Action<MeshData> callback, float[] noiseMap, LevelOfDetail lod)
        {
            var th = new Thread(() =>
            {
                var meshData = Utils.MeshGenerator.GenerateFromHeightMap(noiseMap, MapChunkVerticesCount, TerrainDepthMultiplier, MeshHeightCurve, (int)lod, TerrainRegions);
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
