using UnityEngine;
using UnityEngine.Assertions;
using TerrainGeneration.Structs;

namespace TerrainGeneration.Utils
{
    public static class MeshGenerator
    {
        public static MeshData GenerateFromNoiseMap(float[] noiseMap, int verticesPerLine, TerrainParams terrainParams, int levelOfDetail)
        {
            Assert.AreEqual(noiseMap.Length, (verticesPerLine + 2) * (verticesPerLine + 2),
                "Noise map has to have additional border (1 point wide) data for normals calculation");
            Assert.AreEqual((verticesPerLine - 1) % levelOfDetail, 0,
                "LOD has to be devider of verticesPerLine - 1");

            int borderedMeshVerticesPerLine = verticesPerLine + 2;
            int visibleMeshVerticesPerLine = (verticesPerLine - 1) / levelOfDetail + 1;
            float offsetX = (verticesPerLine - 1) / -2f;
            float offsetZ = (verticesPerLine - 1) / -2f;
            var meshData = new MeshData(visibleMeshVerticesPerLine + 2);
            var heightCurve = new AnimationCurve(terrainParams.MeshHeightCurve.keys);

            int iter = 0;
            for (int zCoord = 0; zCoord < borderedMeshVerticesPerLine; zCoord = GetNextCoord(zCoord, borderedMeshVerticesPerLine - 1, levelOfDetail))
            {
                for (int xCoord = 0; xCoord < borderedMeshVerticesPerLine; xCoord = GetNextCoord(xCoord, borderedMeshVerticesPerLine - 1, levelOfDetail))
                {
                    int noiseCoord = xCoord + zCoord * borderedMeshVerticesPerLine;
                    float yCoord = heightCurve.Evaluate(noiseMap[noiseCoord]) * terrainParams.DepthMultiplier;

                    meshData.vertices[iter] = new Vector3(xCoord + offsetX, yCoord, zCoord + offsetZ);
                    meshData.uvs[iter] = new Vector2(xCoord / (float)verticesPerLine, zCoord / (float)verticesPerLine);
                    meshData.colors[iter] = terrainParams.ColorRegions.Evaluate(noiseMap[noiseCoord]);

                    //without last row / column
                    if (xCoord < borderedMeshVerticesPerLine - 1 && zCoord < borderedMeshVerticesPerLine - 1)
                    {
                        bool isBorderTriangle = 
                            xCoord == 0 || xCoord == borderedMeshVerticesPerLine - 2 ||
                            zCoord == 0 || zCoord == borderedMeshVerticesPerLine - 2;
                        meshData.AddTriangleIndices(iter, iter + visibleMeshVerticesPerLine + 2, iter + 1, !isBorderTriangle);
                        meshData.AddTriangleIndices(iter + 1, iter + visibleMeshVerticesPerLine + 2, iter + visibleMeshVerticesPerLine + 3, !isBorderTriangle);
                    }

                    iter++;
                }
            }

            meshData.CalculateVisibleTriangleIndices();
            meshData.CalculateNormals();
            return meshData;
        }

        private static int GetNextCoord(int prevValue, int maxValue, int lodIncrement)
        {
            return prevValue == 0 || prevValue == maxValue - 1 ? prevValue + 1 : prevValue + lodIncrement;
        }
    }
}
