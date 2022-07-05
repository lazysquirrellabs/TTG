using System.Collections.Generic;
using UnityEngine;

namespace SneakySquirrelLabs.TerracedTerrainGenerator.Data
{
    /// <summary>
    /// Represents all the data necessary to build a simple mesh - a mesh with just 1 sub mesh.
    /// </summary>
    internal sealed class SimpleMeshData : MeshData
    {
        #region Properties

        /// <summary>
        /// The mesh' (triangle) indices.
        /// </summary>
        internal List<int> Indices => IndicesPerSubMesh[0];

        #endregion
        
        #region Setup


        /// <summary>
        /// Creates mesh data for a simple mesh with the provided mesh data.
        /// </summary>
        /// <param name="vertices">The initial mesh vertices.</param>
        /// <param name="indices">The initial mesh (triangle) indices.</param>
        internal SimpleMeshData(Vector3[] vertices, IEnumerable<int> indices) : base(vertices, indices){ }
        
        /// <summary>
        /// Creates mesh data for a simple mesh, allocating space for the provided vertex and indices amounts.
        /// </summary>
        /// <param name="vertexCount">The initial amount of mesh vertices.</param>
        /// <param name="indicesCount">The initial amount of mesh (triangle) indices.</param>
        internal SimpleMeshData(int vertexCount, int indicesCount) : base(vertexCount, indicesCount, 1) { }

        #endregion

        #region Internal

        /// <summary>
        /// Adds a triangle to the mesh. Points are provided in clockwise order as soon from the rendered surface.
        /// </summary>
        /// <param name="v1">The first point of the triangle.</param>
        /// <param name="v2">The second point of the triangle.</param>
        /// <param name="v3">The third point of the triangle.</param>
        internal void AddTriangle(Vector3 v1, Vector3 v2, Vector3 v3)
        {
            AddTriangleAt(v1, v2, v3, 0);
        }

        /// <summary>
        /// Adds a quadrilateral to the mesh. Points are provided in clockwise order as seen from the rendered surface.
        /// </summary>
        /// <param name="v1">The bottom left corner of the quadrilateral.</param>
        /// <param name="v2">The top left corner of the quadrilateral.</param>
        /// <param name="v3">The top right corner of the quadrilateral.</param>
        /// <param name="v4">The bottom right corner of the quadrilateral.</param>
        internal void AddQuadrilateral(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4)
        {
            AddQuadrilateralAt(v1, v2, v3, v4, 0);
        }

        #endregion
    }
}