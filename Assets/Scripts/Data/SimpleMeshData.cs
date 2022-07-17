using System;
using System.Linq;
using Unity.Collections;
using UnityEngine;

namespace SneakySquirrelLabs.TerracedTerrainGenerator.Data
{
    /// <summary>
    /// Represents all the data necessary to build a simple mesh - a mesh with just 1 sub mesh.
    /// </summary>
    internal sealed class SimpleMeshData : MeshData
    {
        #region Fields

        /// <summary>
        /// The index of the next vertex.
        /// </summary>
        private int _nextVertexIndex;

        #endregion
        
        #region Properties
        
        /// <summary>
        /// All the vertices in the mesh data. 
        /// </summary>
        internal NativeList<Vector3> Vertices { get; private set; }
        /// <summary>
        /// All the (triangle) indices in the mesh data.
        /// </summary>
        internal NativeList<int> Indices => IndicesPerSubMesh[0];
        
        #endregion

        #region Public

        public override void Dispose()
        {
            if (Vertices.IsCreated)
                Vertices.Dispose();
            
            foreach (var indices in IndicesPerSubMesh.Where(i => i.IsCreated))
                indices.Dispose();
        }

        #endregion
        
        #region Setup

        /// <summary>
        /// Creates mesh data for a simple mesh with the provided mesh data.
        /// </summary>
        /// <param name="vertices">The initial mesh vertices.</param>
        /// <param name="indices">The initial mesh (triangle) indices.</param>
        internal SimpleMeshData(NativeList<Vector3> vertices, NativeList<int> indices)
        {
            Vertices = vertices;
            // A simple mesh on has 1 sub mesh
            IndicesPerSubMesh = new NativeList<int>[1];
            IndicesPerSubMesh[0] = indices;
        }

        /// <summary>
        /// Creates mesh data for a simple mesh, allocating space for the provided vertex and indices amounts.
        /// </summary>
        /// <param name="vertexCount">The initial amount of mesh vertices.</param>
        /// <param name="indicesCount">The initial amount of mesh (triangle) indices.</param>
        /// <param name="allocator">The allocation strategy used when creating vertex and index buffers.</param>
        internal SimpleMeshData(int vertexCount, int indicesCount, Allocator allocator)
        {
            Vertices = new NativeList<Vector3>(vertexCount, allocator);
            // A simple mesh on has 1 sub mesh
            IndicesPerSubMesh = new NativeList<int>[1];
            IndicesPerSubMesh[0] = new NativeList<int>(indicesCount, allocator);
        }

        #endregion

        #region Internal

        /// <summary>
        /// Maps a function <paramref name="f"/> to all vertices of the mesh.
        /// </summary>
        /// <param name="f">The function to be applied on all vertices.</param>
        internal void Map(Func<Vector3, Vector3> f)
        {
            var vertices = Vertices;
            for (var i = 0; i < Vertices.Length; i++)
            {
                var vertex = Vertices[i];
                vertex = f(vertex);
                vertices[i] = vertex;
            }

            Vertices = vertices;
        }
        
        /// <summary>
        /// Adds a triangle to the mesh. Points are provided in clockwise order as seen from the rendered surface.
        /// </summary>
        /// <param name="v1">The first point of the triangle.</param>
        /// <param name="v2">The second point of the triangle.</param>
        /// <param name="v3">The third point of the triangle.</param>
        internal void AddTriangle(Vector3 v1, Vector3 v2, Vector3 v3)
        {
            AddTriangleAt(v1, v2, v3, 0, ref _nextVertexIndex);
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
            AddQuadrilateralAt(v1, v2, v3, v4, 0, ref _nextVertexIndex);
        }

        #endregion

        #region Protected

        protected override int AddVertex(Vector3 vertex, ref int index)
        {
            Vertices.Add(vertex);
            var newIndex = index;
            index++;
            return newIndex;
        }

        #endregion
    }
}