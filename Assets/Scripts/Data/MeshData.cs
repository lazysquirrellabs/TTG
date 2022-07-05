using System;
using System.Collections.Generic;
using UnityEngine;

namespace SneakySquirrelLabs.TerracedTerrainGenerator.Data
{
    /// <summary>
    /// Represents all the necessary data to build a mesh.
    /// </summary>
    internal abstract class MeshData
    {
        #region Fields

        /// <summary>
        /// The index of the next vertex to be added to the mesh.
        /// </summary>
        private int _nextVertexIndex;

        #endregion
        
        #region Properties

        /// <summary>
        /// The mesh's vertices.
        /// </summary>
        internal List<Vector3> Vertices { get; }
        /// <summary>
        /// The mesh's (triangle) indices per sub mesh.
        /// </summary>
        protected List<int>[] IndicesPerSubMesh { get;}

        #endregion

        #region Setup

        /// <summary>
        /// Creates mesh data with only 1 sub mesh and with the provided mesh data.
        /// </summary>
        /// <param name="vertices">The initial mesh vertices.</param>
        /// <param name="indices">The initial mesh (triangle) indices.</param>
        internal MeshData(IEnumerable<Vector3> vertices, IEnumerable<int> indices)
        {
            Vertices = new List<Vector3>(vertices);
            IndicesPerSubMesh = new List<int>[1];
            IndicesPerSubMesh[0] = new List<int>(indices);
        }

        /// <summary>
        /// Creates mesh data with many sub meshes, allocating space for the provided vertex and indices amounts.
        /// </summary>
        /// <param name="vertexCount">The initial amount of mesh vertices.</param>
        /// <param name="indicesCount">The initial amount of mesh (triangle) indices.</param>
        /// <param name="subMeshes">The number of sub meshes.</param>
        /// <exception cref="ArgumentException">Thrown whenever an invalid number of sub meshes is provided.</exception>
        internal MeshData(int vertexCount, int indicesCount, int subMeshes)
        {
            if (subMeshes < 1)
                throw new ArgumentException("Mesh data must contain at least 1 sub mesh");
            
            Vertices = new List<Vector3>(vertexCount);
            IndicesPerSubMesh = new List<int>[subMeshes];
            var indicesPerSubMesh = indicesCount / subMeshes;
            for (var i = 0; i < subMeshes; i++)
                IndicesPerSubMesh[i] = new List<int>(indicesPerSubMesh);
        }

        #endregion

        #region Internal

        /// <summary>
        /// Maps a function <paramref name="f"/> to all vertices of the mesh.
        /// </summary>
        /// <param name="f">The function to be executed on all vertices.</param>
        internal void Map(Func<Vector3, Vector3> f)
        {
            for (var i = 0; i < Vertices.Count; i++)
                Vertices[i] = f(Vertices[i]);
        }

        /// <summary>
        /// Adds a triangle to the mesh. Points are provided in clockwise order as soon from the rendered surface.
        /// </summary>
        /// <param name="v1">The first point of the triangle.</param>
        /// <param name="v2">The second point of the triangle.</param>
        /// <param name="v3">The third point of the triangle.</param>
        /// <param name="subMesh">The index of the sub mesh to add the triangle to.</param>
        internal void AddTriangleAt(Vector3 v1, Vector3 v2, Vector3 v3, int subMesh)
        {
            // Add vertices
            Vertices.Add(v1);
            Vertices.Add(v2);
            Vertices.Add(v3);
            // Add indices
            var indices = IndicesPerSubMesh[subMesh];
            indices.Add(_nextVertexIndex);
            _nextVertexIndex++;
            indices.Add(_nextVertexIndex);
            _nextVertexIndex++;
            indices.Add(_nextVertexIndex);
            _nextVertexIndex++;
        }

        /// <summary>
        /// Adds a quadrilateral to the mesh. Points are provided in clockwise order as seen from the rendered surface.
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
            var indices = IndicesPerSubMesh[subMesh];
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