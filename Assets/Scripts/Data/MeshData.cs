using System;
using System.Collections.Generic;
using UnityEngine;

namespace SneakySquirrelLabs.TerracedTerrainGenerator.Data
{
    internal abstract class MeshData
    {
        #region Fields

        private int _nextVertexIndex;

        #endregion
        
        #region Properties

        internal List<Vector3> Vertices { get; }
        protected List<int>[] IndicesPerTerrace { get;}

        #endregion

        #region Setup

        internal MeshData(Vector3[] vertices, IEnumerable<int> indices)
        {
            Vertices = new List<Vector3>(vertices);
            IndicesPerTerrace = new List<int>[1];
            IndicesPerTerrace[0] = new List<int>(indices);
        }

        internal MeshData(int vertexCount, int indicesCount, int subMeshes = 1)
        {
            Vertices = new List<Vector3>(vertexCount);
            IndicesPerTerrace = new List<int>[subMeshes];
            var indicesPerSubMesh = indicesCount / subMeshes;
            for (var i = 0; i < subMeshes; i++)
                IndicesPerTerrace[i] = new List<int>(indicesPerSubMesh);
        }

        #endregion

        #region Internal

        internal void Map(Func<Vector3, Vector3> f)
        {
            for (var i = 0; i < Vertices.Count; i++)
                Vertices[i] = f(Vertices[i]);
        }

        internal void AddTriangleAt(Vector3 v1, Vector3 v2, Vector3 v3, int subMesh)
        {
            Vertices.Add(v1);
            Vertices.Add(v2);
            Vertices.Add(v3);
            var indices = IndicesPerTerrace[subMesh];
            indices.Add(_nextVertexIndex);
            _nextVertexIndex++;
            indices.Add(_nextVertexIndex);
            _nextVertexIndex++;
            indices.Add(_nextVertexIndex);
            _nextVertexIndex++;
        }

        /// <summary>
        /// Add a quadrilateral to the mesh. Points are provided in clockwise order as seen from the rendered surface.
        /// </summary>
        /// <param name="v1">The bottom left corner of the quadrilateral.</param>
        /// <param name="v2">The top left corner of the quadrilateral.</param>
        /// <param name="v3">The top right corner of the quadrilateral.</param>
        /// <param name="v4">The bottom right corner of the quadrilateral.</param>
        /// <param name="subMesh">The index of the sub mesh to add the quadrilateral to.</param>
        internal void AddQuadrilateralAt(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, int subMesh)
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
            var indices = IndicesPerTerrace[subMesh];
            // Add triangle 1
            indices.Add(index1);
            indices.Add(index2);
            indices.Add(index4);
            // Add triangle 2
            indices.Add(index2);
            indices.Add(index3);
            indices.Add(index4);
        }

        #endregion
    }
}