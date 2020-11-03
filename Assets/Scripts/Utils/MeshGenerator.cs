using UnityEngine;
using UnityEngine.Assertions;
using Utils.Models;

namespace Utils
{
    public static class MeshGenerator
    {
        public static MeshData GenerateFromHeightMap(float[] map, int width, int height, int depth, AnimationCurve heightCurve)
        {
            Assert.AreEqual(map.Length, width * height);
            var data = new MeshData(width, height);
            float offsetX = (width - 1) / -2f;
            float offsetZ = (height - 1) / -2f;

            int vertexIndex = 0;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    data.Vertices[vertexIndex] = new Vector3((x + offsetX), heightCurve.Evaluate(map[vertexIndex]) * depth, (y + offsetZ));
                    data.UVs[vertexIndex] = new Vector2(x / (float)width, y / (float)height);

                    if (x < width - 1 && y < height - 1)
                    {
                        data.AddTriangleIndices(vertexIndex, vertexIndex + width, vertexIndex + 1);
                        data.AddTriangleIndices(vertexIndex + 1, vertexIndex + width, vertexIndex + width + 1);
                    }

                    vertexIndex++;
                }
            }

            return data;
        }

        public static MeshData GenerateFromHeightMapWithColors(float[] map, Gradient colors, int width, int height, int depth, AnimationCurve heightCurve)
        {
            var data = GenerateFromHeightMap(map, width, height, depth, heightCurve);

            for (int i = 0; i < map.Length; i++)
            {
                data.Colors[i] = colors.Evaluate(map[i]);
            }

            return data;
        }
    }
}
