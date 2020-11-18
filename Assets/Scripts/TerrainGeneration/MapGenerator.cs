using System;
using UnityEngine;
using DataStructures;
using System.Threading;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace TerrainGeneration
{
    public enum MapDisplayType
    {
        Texture,
        ColorShader
    }

    public enum LevelOfDetail
    {
        _1 = 1,
        _2 = 2,
        _4 = 4,
        _6 = 6,
        _8 = 8,
        _10 = 10
    }

    public class MapGenerator : MonoBehaviour
    {
        struct MapThreadData<T>
        {
            public readonly Action<T> callback;
            public readonly T parameter;

            public MapThreadData(Action<T> callback, T parameter)
            {
                this.callback = callback;
                this.parameter = parameter;
            }
        }

        //Number of vertices in one dimention of map
        public const int MapChunkVerticesCount = 241;

        public MeshFilter MeshFilter;
        public MeshRenderer MeshRenderer;

        [Header("Map settings")]
        public MapDisplayType DisplayType = MapDisplayType.Texture;
        [Tooltip("Mesh simplification increment")]
        public LevelOfDetail LevelOfDetail = LevelOfDetail._1;

        [Header("Noise settings")]
        public int seed = 0;
        public float offsetX = 0f;
        public float offsetY = 0f;
        public NoiseParams noiseParams = NoiseParams.CreateWithDefaults();

        [Header("Terrain settings")]
        [Min(1)]
        public int TerrainDepthMultiplier = 20;
        public Gradient TerrainRegions;
        public AnimationCurve MeshHeightCurve;

        [Space]
        public bool AutoUpdate = false;

        private readonly ConcurrentQueue<MapThreadData<float[]>> noiseMapQueue = new ConcurrentQueue<MapThreadData<float[]>>();
        private readonly ConcurrentQueue<MapThreadData<MeshData>> meshQueue = new ConcurrentQueue<MapThreadData<MeshData>>();

        public void Generate()
        {
            float[] noiseMap = Utils.NoiseMapGenerator.GenerateMap(MapChunkVerticesCount, MapChunkVerticesCount, noiseParams, offsetX, offsetY, seed);
            var meshData = Utils.MeshGenerator.GenerateFromHeightMap(noiseMap, MapChunkVerticesCount, TerrainDepthMultiplier, MeshHeightCurve, (int)LevelOfDetail, TerrainRegions);
            MeshFilter.sharedMesh.Clear();
            MeshFilter.sharedMesh = meshData.Create();
        }

        public void Update()
        {
            while (noiseMapQueue.TryDequeue(out MapThreadData<float[]> threadData))
                threadData.callback(threadData.parameter);

            while (meshQueue.TryDequeue(out MapThreadData<MeshData> threadData))
                threadData.callback(threadData.parameter);
        }

        public void RequestNoiseMap(Action<float[]> callback, float offsetX, float offsetY)
        {
            ThreadStart ts = delegate
            {
                NoiseMapThread(callback, offsetX, offsetY);
            };
            new Thread(ts).Start();
        }

        public void RequestMeshData(Action<MeshData> callback, float[] noiseMap)
        {
            ThreadStart ts = delegate
            {
                MeshDataThread(callback, noiseMap);
            };
            new Thread(ts).Start();
        }

        private void NoiseMapThread(Action<float[]> callback, float offsetX, float offsetY)
        {
            float[] noiseMap = Utils.NoiseMapGenerator.GenerateMap(MapChunkVerticesCount, MapChunkVerticesCount, noiseParams, offsetX, offsetY, seed);
            noiseMapQueue.Enqueue(new MapThreadData<float[]>(callback, noiseMap));
        }

        private void MeshDataThread(Action<MeshData> callback, float[] noiseMap)
        {
            var meshData = Utils.MeshGenerator.GenerateFromHeightMap(noiseMap, MapChunkVerticesCount, TerrainDepthMultiplier, MeshHeightCurve, (int)LevelOfDetail, TerrainRegions);
            meshQueue.Enqueue(new MapThreadData<MeshData>(callback, meshData));
        }
    }
}
