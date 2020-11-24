using UnityEngine;
using UnityEngine.Assertions;
using TerrainGeneration;

namespace TerrainGeneration.Utils
{
    public static class MeshGenerator
    {
        public static MeshData GenerateFromHeightMap(float[] noiseMap, int mapSize, int depth, AnimationCurve heightCurve, int levelOfDetail, Gradient colors)
        {
            Assert.AreEqual(noiseMap.Length, mapSize * mapSize);
            Assert.AreEqual((mapSize - 1) % levelOfDetail, 0); //LOD has to be devider of mapSize - 1

            float offsetX = (mapSize - 1) / -2f;
            float offsetZ = (mapSize - 1) / -2f;
            int meshSize = (mapSize - 1) / levelOfDetail + 1;
            var data = new MeshData(meshSize, meshSize);
            var heightCurveCopy = new AnimationCurve(heightCurve.keys);

            int iter = 0;
            for (int zCoord = 0; zCoord < mapSize; zCoord += levelOfDetail)
            {
                for (int xCoord = 0; xCoord < mapSize; xCoord += levelOfDetail)
                {
                    int noiseCoord = xCoord + zCoord * mapSize;
                    float yCoord = heightCurveCopy.Evaluate(noiseMap[noiseCoord]) * depth;

                    data.Vertices[iter] = new Vector3(xCoord + offsetX, yCoord, zCoord + offsetZ);
                    data.UVs[iter] = new Vector2(xCoord / (float)mapSize, zCoord / (float)mapSize);
                    data.Colors[iter] = colors.Evaluate(noiseMap[noiseCoord]);

                    if (xCoord < mapSize - 1 && zCoord < mapSize - 1)
                    {
                        data.AddTriangleIndices(iter, iter + meshSize, iter + 1);
                        data.AddTriangleIndices(iter + 1, iter + meshSize, iter + meshSize + 1);
                    }

                    iter++;
                }
            }

            return data;
        }
    }
}
