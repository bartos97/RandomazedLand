using UnityEngine;

namespace TerrainGeneration
{
    public class MeshData
    {
        public Vector3[] vertices;
        public Vector3[] normals;
        public Vector2[] uvs;
        public Color32[] colors;
        private readonly int[] trianglesIndices;
        private readonly Triangle[] triangles;
        private int triangleWorkingIndex = 0;

        public MeshData(int numOfVerticesPerLine)
        {
            vertices = new Vector3[numOfVerticesPerLine * numOfVerticesPerLine];
            colors = new Color32[numOfVerticesPerLine * numOfVerticesPerLine];
            normals = new Vector3[numOfVerticesPerLine * numOfVerticesPerLine];
            uvs = new Vector2[numOfVerticesPerLine * numOfVerticesPerLine];
            trianglesIndices = new int[(numOfVerticesPerLine - 1) * (numOfVerticesPerLine - 1) * 6];
            triangles = new Triangle[(numOfVerticesPerLine - 1) * (numOfVerticesPerLine - 1) * 2];
        }

        public Mesh GetUnityMesh()
        {
            return new Mesh
            {
                vertices = vertices,
                uv = uvs,
                normals = normals,
                colors32 = colors,
                triangles = trianglesIndices
            };
        }

        public void AddTriangleIndices(int index1, int index2, int index3, bool isVisible)
        {
            triangles[triangleWorkingIndex++] = new Triangle(index1, index2, index3, isVisible);
        }

        public void CalculateNormals()
        {
            foreach (var triangle in triangles)
            {
                int vertexIndexA = triangle.Indices[0];
                int vertexIndexB = triangle.Indices[1];
                int vertexIndexC = triangle.Indices[2];
                Vector3 triangleNormal = CalculateTriangleNormal(vertices[vertexIndexA], vertices[vertexIndexB], vertices[vertexIndexC]);

                normals[vertexIndexA] += triangleNormal;
                normals[vertexIndexB] += triangleNormal;
                normals[vertexIndexC] += triangleNormal;
            }

            for (int i = 0; i < normals.Length; i++)
            {
                normals[i].Normalize();
            }
        }

        public void CalculateVisibleTriangleIndices()
        {
            int counter = 0;

            foreach (var triangle in triangles)
            {
                if (!triangle.IsVisible)
                    continue;

                foreach (int index in triangle.Indices)
                {
                    trianglesIndices[counter++] = index;
                }
            }
        }

        private Vector3 CalculateTriangleNormal(Vector3 vertexA, Vector3 vertexB, Vector3 vertexC)
        {
            Vector3 sideAB = vertexB - vertexA;
            Vector3 sideAC = vertexC - vertexA;
            return Vector3.Cross(sideAB, sideAC).normalized;
        }

        private class Triangle
        {
            public int[] Indices { get; private set; }
            public bool IsVisible { get; private set; }

            public Triangle(int index1, int index2, int index3, bool isVisible)
            {
                this.IsVisible = isVisible;
                this.Indices = new int[] { index1, index2, index3 };
            }
        }
    }
}
