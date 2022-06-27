using System.Collections.Generic;
using UnityEngine;

namespace SneakySquirrelLabs.TerracedTerrainGenerator.TerraceGeneration
{
    internal sealed class MeshBuilder
    {
        #region Fields

        private readonly List<Vector3> _vertices;
        private readonly List<int> _triangles;
        private int _nextVertexIndex;
        private int _nextTriangleIndex;

        #endregion

        #region Properties

        internal Vector3[] Vertices => _vertices.ToArray();
        internal int[] Triangles => _triangles.ToArray();

        #endregion

        #region Setup

        internal MeshBuilder(int vertexCount, int triangleCount)
        {
            _vertices = new List<Vector3>(vertexCount);
            _triangles = new List<int>(triangleCount);
        }

        #endregion

        #region Internal

        internal void AddTriangle(Vector3 v1, Vector3 v2, Vector3 v3)
        {
            _vertices.Add(v1);
            _vertices.Add(v2);
            _vertices.Add(v3);
            _triangles.Add(_nextVertexIndex);
            _nextVertexIndex++;
            _triangles.Add(_nextVertexIndex);
            _nextVertexIndex++;
            _triangles.Add(_nextVertexIndex);
            _nextVertexIndex++;
        }

        #endregion
    }
}