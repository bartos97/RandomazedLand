using UnityEngine;

namespace Utils
{
    public static class TextureGenerator
    {
        public static Texture2D CreateFromColorMap(Color32[] map, int width, int height)
        {
            var texture = new Texture2D(width, height);
            texture.SetPixels32(map);
            texture.filterMode = FilterMode.Point;
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.Apply();
            return texture;
        }

        public static Texture2D CreateFromHeightMap(float[] map, int width, int height)
        {
            var colorMap = new Color32[width * height];

            for (int i = 0; i < width * height; i++)
            {
                byte heightValue = (byte)(map[i] * 255);
                colorMap[i] = new Color32(heightValue, heightValue, heightValue, 255);
            }

            return CreateFromColorMap(colorMap, width, height);
        }

    }
}
