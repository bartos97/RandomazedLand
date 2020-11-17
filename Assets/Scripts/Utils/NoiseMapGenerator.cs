using UnityEngine;
using DataStructures;

namespace Utils
{
    public static class NoiseMapGenerator
    {
        private const int MaxRandomOffset = 10000;

        /// <summary>
        /// Generates height map based on Perlin Noise
        /// </summary>
        /// <param name="width">map width (X axis)</param>
        /// <param name="height">map height (Z axis)</param>
        /// <param name="noiseParams">parameters for noise generation</param>
        /// <param name="seed">seed for random number generator</param>
        /// <returns>Flatten 2D array (row major order) with values in range [0, 1]</returns>
        public static float[] GenerateMap(int width, int height, NoiseParams noiseParams, int seed)
        {
            float halfWidth = width / 2f;
            float halfHeight = height / 2f;
            float[] map = new float[width * height];
            Vector2[] octavesOffsets = GenerateRandomOffsets(noiseParams.OctavesAmount, noiseParams.OffsetX, noiseParams.OffsetY, seed);

            int mapIndex = 0;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float amplitude = 1f;
                    float frequency = 1f;
                    float noiseHeight = 0f;

                    for (int i = 0; i < noiseParams.OctavesAmount; i++)
                    {
                        float xPerlinCoord = ((float)x - halfWidth) / noiseParams.Scale * frequency + octavesOffsets[i].x;
                        float yPerlinCoord = ((float)y - halfHeight) / noiseParams.Scale * frequency + octavesOffsets[i].y;
                        float noiseValue = Mathf.PerlinNoise(xPerlinCoord, yPerlinCoord) - 0.5f;

                        noiseHeight += noiseValue * amplitude;
                        amplitude *= noiseParams.Persistance;
                        frequency *= noiseParams.Lacunarity;
                    }

                    map[mapIndex] = noiseHeight;
                    mapIndex++;
                }
            }

            NormalizeValues(map);
            return map;
        }

        private static Vector2[] GenerateRandomOffsets(int amount, float constX = 0f, float constY = 0f, int seed = 0)
        {
            seed = seed == 0 ? (int)System.DateTime.Now.Ticks : seed;
            var rand = new System.Random(seed);
            var array = new Vector2[amount];

            for (int i = 0; i < amount; i++)
            {
                float x = rand.Next(-MaxRandomOffset, MaxRandomOffset) + constX;
                float y = rand.Next(-MaxRandomOffset, MaxRandomOffset) + constY;
                array[i] = new Vector2(x, y);
            }

            return array;
        }

        private static void NormalizeValues(float[] array)
        {
            float minValue = array[0];
            float maxValue = array[0];

            for (int i = 1; i < array.Length; i++)
            {
                if (array[i] > maxValue)
                    maxValue = array[i];
                if (array[i] < minValue)
                    minValue = array[i];
            }

            for (int i = 0; i < array.Length; i++)
            {
                array[i] = Mathf.InverseLerp(minValue, maxValue, array[i]);
            }
        }
    }
}
