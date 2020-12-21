using TerrainGeneration.ScriptableObjects;
using UnityEngine;
using UnityEngine.Assertions;

namespace TerrainGeneration.Utils
{
    public enum NormalizationType
    {
        Local, Global
    }

    public static class NoiseMapGenerator
    {
        private const int MaxRandomOffset = 10000;

        /// <summary>
        /// Generates height map based on Perlin Noise
        /// </summary>
        /// <param name="pointsPerLine">number of points in width and height</param>
        /// <param name="noiseParams">parameters for noise generation</param>
        /// <param name="offsetX">perlin noise map position X</param>
        /// <param name="offsetY">perlin noise map position Y</param>
        /// <param name="seed">seed for random number generator</param>
        /// <returns>Flatten 2D array (row major order) with values in range [0, 1]</returns>
        public static float[] GenerateFromPerlinNoise(int pointsPerLine, NoiseParams noiseParams, float offsetX, float offsetY, int seed, NormalizationType normalizationType = NormalizationType.Global)
        {
            float halfWidth = pointsPerLine / 2f;
            float halfHeight = pointsPerLine / 2f;
            float[] map = new float[pointsPerLine * pointsPerLine];
            Vector2[] octavesOffsets = GenerateRandomOffsets(noiseParams.octavesAmount, offsetX, offsetY, seed);

            int mapIndex = 0;
            for (int y = 0; y < pointsPerLine; y++)
            {
                for (int x = 0; x < pointsPerLine; x++)
                {
                    float amplitude = 1f;
                    float frequency = 1f;
                    float noiseHeight = 0f;

                    for (int i = 0; i < noiseParams.octavesAmount; i++)
                    {
                        float xPerlinCoord = ((float)x - halfWidth + octavesOffsets[i].x) / noiseParams.scale * frequency;
                        float yPerlinCoord = ((float)y - halfHeight + octavesOffsets[i].y) / noiseParams.scale * frequency;
                        float noiseValue = Mathf.PerlinNoise(xPerlinCoord, yPerlinCoord) - 0.5f;

                        noiseHeight += noiseValue * amplitude;
                        amplitude *= noiseParams.persistance;
                        frequency *= noiseParams.lacunarity;
                    }

                    map[mapIndex++] = noiseHeight;
                }
            }

            NormalizeValues(map, normalizationType, noiseParams.octavesAmount, noiseParams.persistance);
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

            for (int i = 0; i < array.Length; i++)
            {
                array[i] = Mathf.Clamp(array[i], minValue, maxValue);
                array[i] = Mathf.InverseLerp(minValue, maxValue, array[i]);
            }
        }
    }
}
