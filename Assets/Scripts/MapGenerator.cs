using System;
using UnityEngine;
using Utils.Models;

public enum MapDisplayType
{
    HeightMap,
    ColorMap
}

[System.Serializable]
public struct TerrainHeightRegion
{
    public string Name;
    [Range(0, 1)]
    public float Height;
    public Color32 Color;
}

public class MapGenerator : MonoBehaviour
{
    public Renderer TextureRenderer;

    [Header("Map settings")]
    public MapDisplayType DisplayType = MapDisplayType.HeightMap;
    public bool MapAsSquare = true;

    [Min(1)]
    public int MapWidth = 255;
    private int PrevMapWidth = 255;

    [Min(1)]
    public int MapHeight = 255;
    private int PrevMapHeight = 255;

    [Header("Noise settings")]
    public int seed = 0;
    public NoiseParams noiseParams = new NoiseParams();

    [Header("Terrain settings")]
    public TerrainHeightRegion[] Regions;
    
    [Space]
    public bool AutoUpdate = false;

    public void Generate()
    {
        float[] noiseMap = Utils.NoiseHeightMap.GenerateMap(MapWidth, MapHeight, noiseParams, seed);

        switch (DisplayType)
        {
            case MapDisplayType.HeightMap:
                DrawTexture(Utils.TextureGenerator.CreateFromHeightMap(noiseMap, MapWidth, MapHeight));
                break;

            case MapDisplayType.ColorMap:
                Color32[] colorMap = CreateColorMapFromRegions(noiseMap);
                DrawTexture(Utils.TextureGenerator.CreateFromColorMap(colorMap, MapWidth, MapHeight));
                break;

            default:
                break;
        }
    }

    private Color32[] CreateColorMapFromRegions(float[] noiseMap)
    {
        Color32[] map = new Color32[noiseMap.Length];

        for (int mapIter = 0; mapIter < noiseMap.Length; mapIter++)
        {
            bool found = false;

            for (int regionIter = 0; regionIter < Regions.Length; regionIter++)
            {
                if (noiseMap[mapIter] <= Regions[regionIter].Height)
                {
                    found = true;
                    map[mapIter] = Regions[regionIter].Color;
                    break;
                }
            }

            if (!found)
            {
                byte heightValue = (byte)(noiseMap[mapIter] * 255);
                map[mapIter] = new Color32(heightValue, heightValue, heightValue, 255);
            }
        }

        return map;
    }

    private void DrawTexture(Texture2D texture)
    {
        TextureRenderer.sharedMaterial.mainTexture = texture;
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
}
