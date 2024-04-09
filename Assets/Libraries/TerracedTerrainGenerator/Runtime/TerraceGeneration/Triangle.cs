using Unity.Collections;
using UnityEngine;

namespace LazySquirrelLabs.TerracedTerrainGenerator.TerraceGeneration
{
    /// <summary>
    /// A triangle representation used for internal calculations.
    /// </summary>
    internal readonly struct Triangle
    {
        #region Properties

        internal Vector3 V1 { get; }
        internal Vector3 V2 { get; }
        internal Vector3 V3 { get; }

        #endregion

        #region Setup

        /// <summary>
        /// Creates a triangle by reading from index and vertex data.
        /// </summary>
        /// <param name="indices">The index data to fetch the triangle indices from.</param>
        /// <param name="vertices">The vertex data to fetch the triangle vertices from.</param>
        /// <param name="index">The index of the first triangle index. Will be modified.</param>
        internal Triangle(NativeList<int> indices, NativeArray<Vector3> vertices, ref int index)
        {
            var ix1 = indices[index];
            index++;
            var ix2 = indices[index];
            index++;
            var ix3 = indices[index];
            index++;

            V1 = vertices[ix1];
            V2 = vertices[ix2];
            V3 = vertices[ix3];
        }

        /// <summary>
        /// Creates a triangle by providing its vertices.
        /// </summary>
        /// <param name="v1">The triangle's first vertex.</param>
        /// <param name="v2">The triangle's second vertex.</param>
        /// <param name="v3">The triangle's third vertex.</param>
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