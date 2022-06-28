using System.Collections.Generic;
using UnityEngine;

namespace SneakySquirrelLabs.TerracedTerrainGenerator.TerraceGeneration
{
    internal sealed class MeshBuilder
    {
        #region Fields

        private readonly List<Vector3> _vertices;
        private readonly List<int> _indices;
        private int _nextVertexIndex;
        private int _nextTriangleIndex;

        #endregion

        #region Properties

        internal Vector3[] Vertices => _vertices.ToArray();
        internal int[] Triangles => _indices.ToArray();

        #endregion

        #region Setup

        internal MeshBuilder(int vertexCount, int triangleCount)
        {
            _vertices = new List<Vector3>(vertexCount);
            _indices = new List<int>(triangleCount);
        }

        #endregion

        #region Internal

        internal void AddTriangle(Vector3 v1, Vector3 v2, Vector3 v3)
        {
            _vertices.Add(v1);
            _vertices.Add(v2);
            _vertices.Add(v3);
            _indices.Add(_nextVertexIndex);
            _nextVertexIndex++;
            _indices.Add(_nextVertexIndex);
            _nextVertexIndex++;
            _indices.Add(_nextVertexIndex);
            _nextVertexIndex++;
        }

        /// <summary>
        /// Add a quadrilateral to the mesh. Points are provided in clockwise order as seen from the rendered surface.
        /// </summary>
        /// <param name="v1">The bottom left corner of the quadrilateral.</param>
        /// <param name="v2">The top left corner of the quadrilateral.</param>
        /// <param name="v3">The top right corner of the quadrilateral.</param>
        /// <param name="v4">The bottom right corner of the quadrilateral.</param>
        internal void AddQuadrilateral(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4)
        {
            // Add all vertices
            _vertices.Add(v1);
            _vertices.Add(v2);
            _vertices.Add(v3);
            _vertices.Add(v4);
            // Calculate each vertex index
            var index1 = _nextVertexIndex;
            _nextVertexIndex++;
            var index2 = _nextVertexIndex;
            _nextVertexIndex++;
            var index3 = _nextVertexIndex;
            _nextVertexIndex++;
            var index4 = _nextVertexIndex;
            _nextVertexIndex++;
            // Add triangle 1
            _indices.Add(index1);
            _indices.Add(index2);
            _indices.Add(index4);
            // Add triangle 2
            _indices.Add(index2);
            _indices.Add(index3);
            _indices.Add(index4);
        }

        #endregion
    }
}