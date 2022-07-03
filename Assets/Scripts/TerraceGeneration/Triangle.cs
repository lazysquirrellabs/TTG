using System.Collections.Generic;
using UnityEngine;

namespace SneakySquirrelLabs.TerracedTerrainGenerator.TerraceGeneration
{
    internal readonly struct Triangle
    {
        #region Properties

        internal Vector3 V1 { get; }
        internal Vector3 V2 { get; }
        internal Vector3 V3 { get; }

        #endregion

        #region Setup

        internal Triangle(IReadOnlyList<int> triangles, IReadOnlyList<Vector3> vertices, ref int index)
        {
            var ix1 = triangles[index];
            index++;
            var ix2 = triangles[index];
            index++;
            var ix3 = triangles[index];
            index++;

            V1 = vertices[ix1];
            V2 = vertices[ix2];
            V3 = vertices[ix3];
        }

        internal Triangle(Vector3 v1, Vector3 v2, Vector3 v3)
        {
            V1 = v1;
            V2 = v2;
            V3 = v3;
        }

        #endregion

        #region Public

        public override string ToString()
        {
            return $"({V1}, {V2}, {V3})";
        }

        #endregion
    }
}