using System;
using System.Collections.Generic;
using SneakySquirrelLabs.TerracedTerrainGenerator.Data;
using SneakySquirrelLabs.TerracedTerrainGenerator.Utils;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using Vector3 = UnityEngine.Vector3;

namespace SneakySquirrelLabs.TerracedTerrainGenerator.TerraceGeneration
{
    /// <summary>
    /// Helper entity responsible for easing the construction of a terraced terrain mesh.
    /// </summary>
    internal sealed class TerracedMeshBuilder : IDisposable
    {
        #region Fields

        /// <summary>
        /// The maximum number of vertices a mesh with a UInt16 index format can hold.
        /// </summary>
        private const int MaxVertexCountUInt16 = 65_535;
        /// <summary>
        /// Mesh data that holds all the terrain's "walls" (vertical triangles) 
        /// </summary>
        private readonly ComplexMeshData _horizontalMeshData;
        /// <summary>
        /// Mesh data that holds all the terrain's "floors" (horizontal triangles) 
        /// </summary>
        private readonly ComplexMeshData _verticalMeshData;
        /// <summary>
        /// The number of terraces to be created.
        /// </summary>
        private readonly int _terraceCount;
        /// <summary>
        /// The index of the next vertex to be added to the terrain mesh. It's shared between both vertical and
        /// horizontal mesh data.
        /// </summary>
        private int _nextVertexIndex;

        #endregion

        #region Setup

        /// <summary>
        /// Creates a <see cref="TerracedMeshBuilder"/>.
        /// </summary>
        /// <param name="vertexCount">The initial number of vertices.</param>
        /// <param name="indicesCount">The initial number of indices.</param>
        /// <param name="terraceCount">The number of terraces to be created.</param>
        /// <param name="allocator">The allocation strategy used when creating vertex and index buffers.</param>
        internal TerracedMeshBuilder(int vertexCount, int indicesCount, int terraceCount, Allocator allocator)
        {
            _terraceCount = terraceCount;
            // Both horizontal and vertical mesh data are initialized with the given vertex and indices count because
            // the number of generated data (both vertices and indices) is usually much larger than the provided, 
            // initial values. These initial values are used just to avoid late buffer resizing.
            _horizontalMeshData = new ComplexMeshData(vertexCount, indicesCount, terraceCount, allocator);
            _verticalMeshData = new ComplexMeshData(vertexCount, indicesCount, terraceCount, allocator);
        }

        #endregion

        #region Public

        public void Dispose()
        {
            _horizontalMeshData.Dispose();
            _verticalMeshData.Dispose();
        }

        #endregion

        #region Internal

        /// <summary>
        /// Adds a whole, flat <see cref="Triangle"/> to the to-be-generated terraced mesh.
        /// </summary>
        /// <param name="triangle">The original triangle.</param>
        /// <param name="height">The height which the triangle will be added at.</param>
        /// <param name="terraceIx">The index of the terrace the provided triangle will be added to.</param>
        internal void AddWholeTriangle(Triangle triangle, float height, int terraceIx)
        {
            var v1 = new Vector3(triangle.V1.x, height, triangle.V1.z);
            var v2 = new Vector3(triangle.V2.x, height, triangle.V2.z);
            var v3 = new Vector3(triangle.V3.x, height, triangle.V3.z);
            _horizontalMeshData.AddTriangle(v1, v2, v3, terraceIx, ref _nextVertexIndex);
        }
        
        /// <summary>
        /// Adds a triangle which has 1 vertex above the given <paramref name="plane"/>.
        /// </summary>
        /// <param name="t">The original <see cref="Triangle"/>.</param>
        /// <param name="plane">The plane which sliced the triangle, leaving 1 vertex above it.</param>
        /// <param name="previousPlane">The plane placed right above the one who sliced the triangle.</param>
        /// <param name="terraceIx">The index of the terrace to add the triangle to.</param>
        internal void AddSlicedTriangle1Above(Triangle t, float plane, float previousPlane, int terraceIx)
        {
            // Add floor
            var floor1 = GetPlanePoint(t.V1, t.V3, plane);
            var floor2 = GetPlanePoint(t.V2, t.V3, plane);
            var floor3 = new Vector3(t.V3.x, plane, t.V3.z);
            _horizontalMeshData.AddTriangle(floor1, floor2, floor3, terraceIx, ref _nextVertexIndex);

            // Add wall
            var wall1 = floor2;
            var wall2 = floor1;
            var wall3 = new Vector3(floor1.x, previousPlane, floor1.z);
            var wall4 = new Vector3(floor2.x, previousPlane, floor2.z);
            _verticalMeshData.AddQuadrilateral(wall1, wall2, wall3, wall4, terraceIx, ref _nextVertexIndex);
        }
        
        /// <summary>
        /// Adds a triangle which has 2 vertices above the given <paramref name="plane"/>.
        /// </summary>
        /// <param name="t">The original <see cref="Triangle"/>.</param>
        /// <param name="plane">The plane which sliced the triangle, leaving 2 vertices above it.</param>
        /// <param name="previousPlane">The plane placed right above the one who sliced the triangle.</param>
        /// <param name="terraceIx">The index of the terrace to add the triangle to.</param>
        internal void AddSlicedTriangle2Above(Triangle t, float plane, float previousPlane, int terraceIx)
        {
            // Add floor
            var floor1 = GetPlanePoint(t.V1, t.V3, plane);
            var floor2 = GetPlanePoint(t.V2, t.V3, plane);
            var floor3 = new Vector3(t.V1.x, plane, t.V1.z);
            var floor4 = new Vector3(t.V2.x, plane, t.V2.z);
            _horizontalMeshData.AddQuadrilateral(floor1, floor3, floor4, floor2, terraceIx, ref _nextVertexIndex);

            // Add wall
            var wall1 = floor1;
            var wall2 = floor2;
            var wall3 = new Vector3(floor2.x, previousPlane, floor2.z);
            var wall4 = new Vector3(floor1.x, previousPlane, floor1.z);
            _verticalMeshData.AddQuadrilateral(wall1, wall2, wall3, wall4, terraceIx, ref _nextVertexIndex);
        }
        
        /// <summary>
        /// Builds a <see cref="Mesh"/> based on the builder mesh data.
        /// </summary>
        /// <returns>The terraced terrain <see cref="Mesh"/>.</returns>
        internal Mesh Build()
        {
            var mesh = new Mesh();
            mesh.name = "Terraced Terrain";
            // Merge both vertical and horizontal vertices into a single array.
            var vertices = MergeVertices(_horizontalMeshData.Vertices, _verticalMeshData.Vertices);
            var vertexCount = vertices.Length;
            if (vertexCount > MaxVertexCountUInt16)
                mesh.indexFormat = IndexFormat.UInt32;
            // Each terrace must be a sub mesh to allow for material assignment per terrace.
            mesh.subMeshCount = _terraceCount;
            
            // Set mesh vertex data
            mesh.SetVertices(vertices, 0, vertexCount);
            
            // Set mesh indices data, per terrace/sub mesh
            for (var i = 0; i < _terraceCount; i++)
            {
                var horizontalIndices = _horizontalMeshData.GetIndices(i);
                var verticalIndices = _verticalMeshData.GetIndices(i);
                var indices = horizontalIndices.Combine(verticalIndices);
                mesh.SetTriangles(indices, 0, indices.Length, i);
            }
            
            mesh.RecalculateNormals();
            return mesh;

            static Vector3[] MergeVertices(Dictionary<Vector3,int> v1, Dictionary<Vector3,int> v2)
            {
                var vertices = new Vector3[v1.Count + v2.Count];
                AddVertices(v1);
                AddVertices(v2);
                return vertices;

                void AddVertices(Dictionary<Vector3, int> v)
                {
                    foreach (var (vertex, index) in v)
                        vertices[index] = vertex;
                }
            }
        }

        #endregion
        
        #region Private

        /// <summary>
        /// Gets a point between two provided positions, placed exactly at the given <paramref name="height"/>. It
        /// assumes that the provided data is valid: that the <paramref name="lower"/> point is actually lower and the
        /// <paramref name="height"/> lies between the 2 provided points.
        /// </summary>
        /// <param name="lower">The lower point.</param>
        /// <param name="higher">The higher point.</param>
        /// <param name="height">The height of the desired point between <paramref name="lower"/> and
        /// <paramref name="higher"/>.</param>
        /// <returns>A point between <paramref name="lower"/> and <paramref name="higher"/>, placed exactly at the
        /// provided height.</returns>
        private static Vector3 GetPlanePoint(Vector3 lower, Vector3 higher, float height)
        {
            var t = (height - lower.y) / (higher.y - lower.y);
            return Vector3.Lerp(lower, higher, t);
        }

        #endregion
    }
}