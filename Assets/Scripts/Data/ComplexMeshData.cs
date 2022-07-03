using System.Collections.Generic;
using UnityEngine;

namespace SneakySquirrelLabs.TerracedTerrainGenerator.Data
{
    internal class ComplexMeshData : MeshData
    {
        #region Setup

        internal ComplexMeshData(int subMeshes, int vertexCount, int indicesCount)
            : base(vertexCount, indicesCount, subMeshes) {}

        #endregion

        #region Internal

        internal void AddTriangle(Vector3 v1, Vector3 v2, Vector3 v3, int subMesh)
        {
            AddTriangleAt(v1, v2, v3, subMesh);
        }
        
        internal void AddQuadrilateral(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, int subMesh)
        {
            AddQuadrilateralAt(v1, v2, v3, v4, subMesh);
        }

        #endregion

        #region Internal

        internal List<int> GetIndices(int subMesh) => IndicesPerTerrace[subMesh];

        #endregion
    }
}