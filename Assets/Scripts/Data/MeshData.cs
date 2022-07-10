using System.Collections.Generic;
using UnityEngine;

namespace SneakySquirrelLabs.TerracedTerrainGenerator.Data
{
    /// <summary>
    /// Abstract class used to create data that can be used to create a <see cref="Mesh"/>.
    /// </summary>
    internal abstract class MeshData
    {
        #region Properties

        /// <summary>
        /// The mesh's (triangle) indices per sub mesh.
        /// </summary>
        protected List<int>[] IndicesPerSubMesh { get; set; }

        #endregion

        #region Protected

        /// <summary>
        /// Adds a triangle to the mesh. Points are provided in clockwise order as soon from the rendered surface.
        /// </summary>
        /// <param name="v1">The first point of the triangle.</param>
        /// <param name="v2">The second point of the triangle.</param>
        /// <param name="v3">The third point of the triangle.</param>
        /// <param name="subMesh">The index of the sub mesh to add the triangle to.</param>
        /// <param name="vertexIndex">The index to start adding vertices from.</param>
        protected void AddTriangleAt(Vector3 v1, Vector3 v2, Vector3 v3, int subMesh, ref int vertexIndex)
        {
            // Add vertices
            var ix1 = AddVertex(v1, ref vertexIndex);
            var ix2 = AddVertex(v2, ref vertexIndex);
            var ix3 = AddVertex(v3, ref vertexIndex);
            // Add indices
            var indices = IndicesPerSubMesh[subMesh];
            indices.Add(ix1);
            indices.Add(ix2);
            indices.Add(ix3);
        }

        /// <summary>
        /// Adds a quadrilateral to the mesh. Points are provided in clockwise order as seen from the rendered surface.
        /// </summary>
        /// <param name="v1">The bottom left corner of the quadrilateral.</param>
        /// <param name="v2">The top left corner of the quadrilateral.</param>
        /// <param name="v3">The top right corner of the quadrilateral.</param>
        /// <param name="v4">The bottom right corner of the quadrilateral.</param>
        /// <param name="subMesh">The index of the sub mesh to add the quadrilateral to.</param>
        /// <param name="index">The index to start adding vertices from.</param>
        protected void AddQuadrilateralAt(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, int subMesh, ref int index)
        {
            // Add vertices
            var ix1 = AddVertex(v1, ref index);
            var ix2 = AddVertex(v2, ref index);
            var ix3 = AddVertex(v3, ref index);
            var ix4 = AddVertex(v4, ref index);
            var indices = IndicesPerSubMesh[subMesh];
            // Add triangle 1
            indices.Add(ix1);
            indices.Add(ix2);
            indices.Add(ix4);
            // Add triangle 2
            indices.Add(ix2);
            indices.Add(ix3);
            indices.Add(ix4);
        }

        /// <summary>
        /// Adds a vertex to the mesh data.
        /// </summary>
        /// <param name="vertex">The vertex to add.</param>
        /// <param name="index">The index to add the vertex at. Will be modified.</param>
        /// <returns></returns>
        protected abstract int AddVertex(Vector3 vertex, ref int index);

        #endregion
    }
}