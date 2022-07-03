using System.Collections.Generic;
using UnityEngine;

namespace SneakySquirrelLabs.TerracedTerrainGenerator.Data
{
    internal sealed class SimpleMeshData : MeshData
    {
        #region Properties

        internal List<int> Indices => IndicesPerTerrace[0];

        #endregion
        
        #region Setup

        internal SimpleMeshData(int vertexCount, int indicesCount) : base(vertexCount, indicesCount) { }

        internal SimpleMeshData(Vector3[] vertices, IEnumerable<int> indices) : base(vertices, indices){ }

        #endregion

        #region Internal

        internal void AddTriangle(Vector3 v1, Vector3 v2, Vector3 v3)
        {
            AddTriangleAt(v1, v2, v3, 0);
        }

        internal void AddQuadrilateral(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4)
        {
            AddQuadrilateralAt(v1, v2, v3, v4, 0);
        }

        #endregion
    }
}