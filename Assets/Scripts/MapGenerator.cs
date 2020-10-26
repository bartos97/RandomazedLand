using System;
using UnityEngine;
using Utils.Models;

public enum MapDisplayType
{
    HeightMap,
    ColorMap,
    Mesh
}


public class MapGenerator : MonoBehaviour
{
    public Renderer TextureRenderer;
    public MeshFilter MeshFilter;
    public MeshRenderer MeshRenderer;

    [Header("Map settings")]
    public MapDisplayType DisplayType = MapDisplayType.HeightMap;
    public bool MapAsSquare = true;

    [Min(1)]
    public int MapWidth = 255;
    private int PrevMapWidth = 255;

    [Min(1)]
    public int MapHeight = 255;
    private int PrevMapHeight = 255;

    [Min(1)]
    public int TerrainDepth = 20;

    [Header("Noise settings")]
    public int seed = 0;
    public NoiseParams noiseParams = NoiseParams.CreateWithDefaults();

    [Header("Terrain settings")]
    public Gradient TerrainRegions;
    
    [Space]
    public bool AutoUpdate = false;

    public void Generate()
    {
        float[] noiseMap = Utils.NoiseMapGenerator.GenerateMap(MapWidth, MapHeight, noiseParams, seed);
        Color32[] colorMap = CreateColorMapFromRegions(noiseMap);

        switch (DisplayType)
        {
            case MapDisplayType.HeightMap:
                DisplayTexture(Utils.TextureGenerator.GenerateFromHeightMap(noiseMap, MapWidth, MapHeight));
                break;

            case MapDisplayType.ColorMap:
                DisplayTexture(Utils.TextureGenerator.GenerateFromColorMap(colorMap, MapWidth, MapHeight));
                break;

            case MapDisplayType.Mesh:
                DisplayMesh(Utils.MeshGenerator.GenerateFromHeightMap(noiseMap, MapWidth, MapHeight, TerrainDepth), Utils.TextureGenerator.GenerateFromColorMap(colorMap, MapWidth, MapHeight));
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
            TextureRenderer.transform.localScale = new Vector3(MapWidth, 1, MapHeight);
            PrevMapWidth = MapWidth;
            PrevMapHeight = MapHeight;
        }

    }

    private Color32[] CreateColorMapFromRegions(float[] noiseMap)
    {
        Color32[] map = new Color32[noiseMap.Length];

        for (int i = 0; i < noiseMap.Length; i++)
        {
            map[i] = TerrainRegions.Evaluate(noiseMap[i]);
        }

        return map;
    }

    private void DisplayTexture(Texture2D texture)
    {
        TextureRenderer.sharedMaterial.mainTexture = texture;
    }

    private void DisplayMesh(MeshData meshData, Texture2D texture)
    {
        MeshFilter.sharedMesh = meshData.Create();
        MeshRenderer.sharedMaterial.mainTexture = texture;
    }
}
