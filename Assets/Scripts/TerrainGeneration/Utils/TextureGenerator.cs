using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace TerrainGeneration.Utils
{
    public static class TextureGenerator
    {
        public static Texture2D GenerateFromColorMap(Color32[] map, int width, int height)
        {
            Assert.AreEqual(map.Length, width * height);
            var texture = new Texture2D(width, height);
            texture.SetPixels32(map);
            texture.filterMode = FilterMode.Point;
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.Apply();
            return texture;
        }

        public static Texture2D GenerateFromHeightMap(float[] map, int width, int height)
        {
            Assert.AreEqual(map.Length, width * height);
            var colorMap = new Color32[width * height];

            int counter = 0;
            for (int i = colorMap.Length - 1; i >= 0; i--)
            {
                byte heightValue = (byte)(map[i] * 255);
                colorMap[counter++] = new Color32(heightValue, heightValue, heightValue, 255);
            }

            return GenerateFromColorMap(colorMap, width, height);
        }

    }
}
