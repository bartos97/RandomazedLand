﻿using System;
using UnityEngine;
using Utils.Models;

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
    //Number of vertices in width and height
    public const int MapSize = 241;

    public MeshFilter MeshFilter;
    public MeshRenderer MeshRenderer;

    [Header("Map settings")]
    public MapDisplayType DisplayType = MapDisplayType.Texture;
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
    
    [Space]
    public bool AutoUpdate = false;

    public void Generate()
    {
        float[] noiseMap = Utils.NoiseMapGenerator.GenerateMap(MapSize, MapSize, noiseParams, seed);
        var meshData = Utils.MeshGenerator.GenerateFromHeightMap(noiseMap, MapSize, TerrainDepthMultiplier, MeshHeightCurve, (int)LevelOfDetail, TerrainRegions);

        switch (DisplayType)
        {
            case MapDisplayType.Texture:
                DisplayMesh(meshData, Utils.TextureGenerator.GenerateFromHeightMap(noiseMap, MapSize, MapSize));
                break;

            case MapDisplayType.ColorShader:
                DisplayMesh(meshData);
                break;
        }
    }

    private void DisplayMesh(MeshData meshData)
    {
        MeshFilter.sharedMesh.Clear();
        MeshFilter.sharedMesh = meshData.Create();
    }

    private void DisplayMesh(MeshData meshData, Texture2D texture)
    {
        MeshFilter.sharedMesh.Clear();
        MeshFilter.sharedMesh = meshData.Create();
        MeshRenderer.sharedMaterial.mainTexture = texture;
    }
}
