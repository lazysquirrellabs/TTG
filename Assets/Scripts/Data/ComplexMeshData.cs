using System.Collections.Generic;
using UnityEngine;

namespace SneakySquirrelLabs.TerracedTerrainGenerator.Data
{
    /// <summary>
    /// Represents all the data necessary to build a complex mesh - a mesh with many sub meshes.
    /// </summary>
    internal class ComplexMeshData : MeshData
    {
        #region Setup

        /// <summary>
        /// Creates mesh data for a complex mesh, allocating space for the provided vertex and indices amounts.
        /// </summary>
        /// <param name="vertexCount">The initial amount of mesh vertices.</param>
        /// <param name="indicesCount">The initial amount of mesh (triangle) indices.</param>
        /// <param name="subMeshes">The number of sub meshes.</param>
        internal ComplexMeshData(int vertexCount, int indicesCount, int subMeshes)
            : base(vertexCount, indicesCount, subMeshes) {}

        #endregion

        #region Internal

        /// <inheritdoc cref="MeshData.AddTriangleAt"/>
        internal void AddTriangle(Vector3 v1, Vector3 v2, Vector3 v3, int subMesh)
        {
            AddTriangleAt(v1, v2, v3, subMesh);
        }
        
        /// <inheritdoc cref="MeshData.AddQuadrilateralAt"/>
        internal void AddQuadrilateral(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, int subMesh)
        {
            AddQuadrilateralAt(v1, v2, v3, v4, subMesh);
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
    }
}