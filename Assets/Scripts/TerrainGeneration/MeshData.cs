using System;
using UnityEngine;

namespace TerrainGeneration
{
    public class MeshData
    {
        public Vector3[] Vertices;
        public Color32[] Colors;
        public int[] TrianglesIndices;
        public Vector2[] UVs;
        private int TriangleIndex = 0;

        public MeshData(int meshWidth, int meshHeight)
        {
            Vertices = new Vector3[meshWidth * meshHeight];
            Colors = new Color32[meshWidth * meshHeight];
            TrianglesIndices = new int[(meshWidth - 1) * (meshHeight - 1) * 6];
            UVs = new Vector2[meshWidth * meshHeight];
        }

        public Mesh Create()
        {
            var mesh = new Mesh();
            mesh.vertices = Vertices;
            mesh.colors32 = Colors;
            mesh.triangles = TrianglesIndices;
            mesh.uv = UVs;
            mesh.RecalculateNormals();
            return mesh;
        }

        public void AddTriangleIndices(int v0, int v1, int v2)
        {
            TrianglesIndices[TriangleIndex++] = v0;
            TrianglesIndices[TriangleIndex++] = v1;
            TrianglesIndices[TriangleIndex++] = v2;
        }
    }
}
