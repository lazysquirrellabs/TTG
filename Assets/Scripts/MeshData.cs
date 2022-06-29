using System;
using System.Collections.Generic;
using UnityEngine;

namespace SneakySquirrelLabs.TerracedTerrainGenerator
{
    internal sealed class MeshData
    {
        #region Fields

        private int _nextVertexIndex;

        #endregion
        
        #region Properties

        internal List<Vector3> Vertices { get; }
        internal List<int> Indices { get; }

        #endregion

        #region Setup

        internal MeshData(int vertexCount, int indicesCount)
        {
            Vertices = new List<Vector3>(vertexCount);
            Indices = new List<int>(indicesCount);
        }

        internal MeshData(Vector3[] vertices, IEnumerable<int> indices)
        {
            Vertices = new List<Vector3>(vertices);
            Indices = new List<int>(indices);
        }

        #endregion

        #region Internal

        internal void Map(Func<Vector3, Vector3> f)
        {
            for (var i = 0; i < Vertices.Count; i++)
                Vertices[i] = f(Vertices[i]);
        }
        
        internal void AddTriangle(Vector3 v1, Vector3 v2, Vector3 v3)
        {
            Vertices.Add(v1);
            Vertices.Add(v2);
            Vertices.Add(v3);
            Indices.Add(_nextVertexIndex);
            _nextVertexIndex++;
            Indices.Add(_nextVertexIndex);
            _nextVertexIndex++;
            Indices.Add(_nextVertexIndex);
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
            Vertices.Add(v1);
            Vertices.Add(v2);
            Vertices.Add(v3);
            Vertices.Add(v4);
            // Calculate each vertex's index
            var index1 = _nextVertexIndex;
            _nextVertexIndex++;
            var index2 = _nextVertexIndex;
            _nextVertexIndex++;
            var index3 = _nextVertexIndex;
            _nextVertexIndex++;
            var index4 = _nextVertexIndex;
            _nextVertexIndex++;
            // Add triangle 1
            Indices.Add(index1);
            Indices.Add(index2);
            Indices.Add(index4);
            // Add triangle 2
            Indices.Add(index2);
            Indices.Add(index3);
            Indices.Add(index4);
        }

        #endregion
    }
}