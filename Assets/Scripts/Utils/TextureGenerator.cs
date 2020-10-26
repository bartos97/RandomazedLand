using UnityEngine;
using UnityEngine.Assertions;

namespace Utils
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

            for (int i = 0; i < width * height; i++)
            {
                byte heightValue = (byte)(map[i] * 255);
                colorMap[i] = new Color32(heightValue, heightValue, heightValue, 255);
            }

            return GenerateFromColorMap(colorMap, width, height);
        }

    }
}
