using TerrainGeneration.Structs;
using UnityEngine;

namespace TerrainGeneration.Utils
{
    public enum NormalizationType
    {
        Local, Global
    }

    public static class NoiseMapGenerator
    {
        private const int MaxRandomOffset = 10000;

        public static float[] GenerateMap(int size, NoiseParams noiseParams, float offsetX, float offsetY, int seed, NormalizationType normalizationType = NormalizationType.Global)
        {
            return GenerateMap(size, size, noiseParams, offsetX, offsetY, seed, normalizationType);
        }

        /// <summary>
        /// Generates height map based on Perlin Noise
        /// </summary>
        /// <param name="width">map width (X axis)</param>
        /// <param name="height">map height (Z axis)</param>
        /// <param name="noiseParams">parameters for noise generation</param>
        /// <param name="offsetX">perlin noise map position X</param>
        /// <param name="offsetY">perlin noise map position Y</param>
        /// <param name="seed">seed for random number generator</param>
        /// <returns>Flatten 2D array (row major order) with values in range [0, 1]</returns>
        public static float[] GenerateMap(int width, int height, NoiseParams noiseParams, float offsetX, float offsetY, int seed, NormalizationType normalizationType = NormalizationType.Global)
        {
            float halfWidth = width / 2f;
            float halfHeight = height / 2f;
            float[] map = new float[width * height];
            Vector2[] octavesOffsets = GenerateRandomOffsets(noiseParams.OctavesAmount, offsetX, offsetY, seed);

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
                        float xPerlinCoord = ((float)x - halfWidth + octavesOffsets[i].x) / noiseParams.Scale * frequency;
                        float yPerlinCoord = ((float)y - halfHeight + octavesOffsets[i].y) / noiseParams.Scale * frequency;
                        float noiseValue = Mathf.PerlinNoise(xPerlinCoord, yPerlinCoord) - 0.5f;

                        noiseHeight += noiseValue * amplitude;
                        amplitude *= noiseParams.Persistance;
                        frequency *= noiseParams.Lacunarity;
                    }

                    map[mapIndex] = noiseHeight;
                    mapIndex++;
                }
            }

            NormalizeValues(map, normalizationType, noiseParams.OctavesAmount, noiseParams.Persistance);
            return map;
        }

        private static Vector2[] GenerateRandomOffsets(int amount, float constX = 0f, float constY = 0f, int seed = 0)
        {
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

        private static void NormalizeValues(float[] array, NormalizationType normalizationType, float octavesAmount, float persistance)
        {
            float minValue = 0f;
            float maxValue = 0f;

            switch (normalizationType)
            {
                case NormalizationType.Local:
                    minValue = array[0];
                    maxValue = array[0];
                    for (int i = 1; i < array.Length; i++)
                    {
                        if (array[i] > maxValue)
                            maxValue = array[i];
                        if (array[i] < minValue)
                            minValue = array[i];
                    }
                    break;

                default:
                case NormalizationType.Global:
                    float amplitude = 1f;
                    for (int i = 0; i < octavesAmount; i++)
                    {
                        maxValue += 0.5f * amplitude;
                        minValue += -0.5f * amplitude;
                        amplitude *= persistance;
                    }
                    minValue /= 1.35f;
                    maxValue /= 1.35f;
                    break;
            }

            //Debug.Log($"min: {minValue} max: {maxValue}");

            for (int i = 0; i < array.Length; i++)
            {
                array[i] = Mathf.Clamp(array[i], minValue, maxValue);
                array[i] = Mathf.InverseLerp(minValue, maxValue, array[i]);
            }
        }
    }
}
