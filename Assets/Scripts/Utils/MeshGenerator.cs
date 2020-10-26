using UnityEngine;
using UnityEngine.Assertions;
using Utils.Models;

namespace Utils
{
    public static class MeshGenerator
    {
        public static MeshData GenerateFromHeightMap(float[] map, int width, int height, int depth)
        {
            Assert.AreEqual(map.Length, width * height);
            var data = new MeshData(width, height);

            int vertexIndex = 0;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    data.Vertices[vertexIndex] = new Vector3(x, map[vertexIndex] * depth, y);
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
    }
}
