using System;
using UnityEngine;
using Utils.Models;

public enum MapDisplayType
{
    Texture,
    ColorShader
}


public class MapGenerator : MonoBehaviour
{
    public MeshFilter MeshFilter;
    public MeshRenderer MeshRenderer;

    [Header("Map settings")]
    public MapDisplayType DisplayType = MapDisplayType.Texture;
    public bool MapAsSquare = true;

    [Min(1)]
    [Tooltip("Number of vertices in X axis")]
    public int MapWidth = 255;
    private int PrevMapWidth = 255;

    [Min(1)]
    [Tooltip("Number of vertices in Z axis")]
    public int MapHeight = 255;
    private int PrevMapHeight = 255;

    [Min(1)]
    public int TerrainDepth = 20;

    [Header("Noise settings")]
    public int seed = 0;
    public NoiseParams noiseParams = NoiseParams.CreateWithDefaults();

    [Header("Terrain settings")]
    public Gradient TerrainRegions;
    //[Range(0, 1)]
    //public float WaterLevel = 0f;
    public AnimationCurve MeshHeightCurve;
    
    [Space]
    public bool AutoUpdate = false;

    public void Generate()
    {
        float[] noiseMap = Utils.NoiseMapGenerator.GenerateMap(MapWidth, MapHeight, noiseParams, seed);

        switch (DisplayType)
        {
            case MapDisplayType.Texture:
                DisplayMesh(Utils.MeshGenerator.GenerateFromHeightMap(noiseMap, MapWidth, MapHeight, TerrainDepth, MeshHeightCurve), Utils.TextureGenerator.GenerateFromHeightMap(noiseMap, MapWidth, MapHeight));
                break;

            case MapDisplayType.ColorShader:
                DisplayMesh(Utils.MeshGenerator.GenerateFromHeightMapWithColors(noiseMap, TerrainRegions, MapWidth, MapHeight, TerrainDepth, MeshHeightCurve));
                break;
        }
    }

    public void OnValidate()
    {
        if (MapAsSquare)
        {
            if (MapWidth != PrevMapWidth)
                MapHeight = MapWidth;
            else if (MapHeight != PrevMapHeight)
                MapWidth = MapHeight;
        }

        if (PrevMapWidth != MapWidth || PrevMapHeight != MapHeight)
        {
            PrevMapWidth = MapWidth;
            PrevMapHeight = MapHeight;
        }

    }

    private void DisplayMesh(MeshData meshData)
    {
        MeshFilter.sharedMesh.Clear();
        MeshFilter.sharedMesh = meshData.Create();
    }

    private void DisplayMesh(MeshData meshData, Texture2D texture)
    {
        MeshFilter.sharedMesh = meshData.Create();
        MeshRenderer.sharedMaterial.mainTexture = texture;
    }
}
