using System;
using System.Collections.Generic;
using UnityEngine;

namespace SneakySquirrelLabs.TerracedTerrainGenerator.Data
{
    /// <summary>
    /// Represents all the data necessary to build a complex mesh - a mesh with many sub meshes.
    /// </summary>
    internal class ComplexMeshData : MeshData
    {
        #region Fields

        internal Dictionary<Vector3, int> Vertices { get; }

        #endregion
        
        #region Setup

        /// <summary>
        /// Creates mesh data for a complex mesh, allocating space for the provided vertex and indices amounts.
        /// </summary>
        /// <param name="indicesCount">The initial amount of mesh (triangle) indices.</param>
        /// <param name="subMeshes">The number of sub meshes.</param>
        /// <exception cref="ArgumentException">Thrown whenever an invalid number of sub meshes is provided.</exception>
        internal ComplexMeshData(int indicesCount, int subMeshes)
        {
            if (subMeshes < 1)
                throw new ArgumentException("Mesh data must contain at least 1 sub mesh");

            Vertices = new Dictionary<Vector3, int>();
            IndicesPerSubMesh = new List<int>[subMeshes];
            var indicesPerSubMesh = indicesCount / subMeshes;
            for (var i = 0; i < subMeshes; i++)
                IndicesPerSubMesh[i] = new List<int>(indicesPerSubMesh);
        }

        #endregion

        #region Internal

        /// <inheritdoc cref="MeshData.AddTriangleAt"/>
        internal void AddTriangle(Vector3 v1, Vector3 v2, Vector3 v3, int subMesh, ref int index)
        {
            AddTriangleAt(v1, v2, v3, subMesh, ref index);
        }
        
        /// <inheritdoc cref="MeshData.AddQuadrilateralAt"/>
        internal void AddQuadrilateral(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, int subMesh, ref int index)
        {
            AddQuadrilateralAt(v1, v2, v3, v4, subMesh, ref index);
        }

        #endregion

        #region Internal

        /// <summary>
        /// Fetches the mesh (triangle) indices for a given <paramref name="subMesh"/> index.
        /// </summary>
        /// <param name="subMesh">The index of the sub mesh to fetch the indices from.</param>
        /// <returns></returns>
        internal List<int> GetIndices(int subMesh) => IndicesPerSubMesh[subMesh];

        #endregion

        #region Protected

        protected override int AddVertex(Vector3 vertex, ref int index)
        {
            if (Vertices.TryGetValue(vertex, out var existingIndex))
                return existingIndex;
            Vertices[vertex] = index;
            var newIndex = index;
            index++;
            return newIndex;
        }

        #endregion
    }
}