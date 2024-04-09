using System;
using LazySquirrelLabs.TerracedTerrainGenerator.Data;
using LazySquirrelLabs.TerracedTerrainGenerator.Utils;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using Vector3 = UnityEngine.Vector3;

namespace LazySquirrelLabs.TerracedTerrainGenerator.TerraceGeneration
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
        /// Mesh data that holds all the terrain's "floors" (horizontal triangles).
        /// </summary>
        private readonly ComplexMeshData _horizontalMeshData;
        /// <summary>
        /// Mesh data that holds all the terrain's "walls" (vertical triangles).
        /// </summary>
        private readonly ComplexMeshData _verticalMeshData;
        /// <summary>
        /// The number of terraces to be created.
        /// </summary>
        private readonly int _terraceCount;
        /// <summary>
        /// A method that finds the height of a vertex.
        /// </summary>
        private readonly HeightGetter _getVertexHeight;
        /// <summary>
        /// A method that returns a copy of a vertex set at a given height.
        /// </summary>
        private readonly HeightSetter _setVertexHeight;
        
        /// <summary>
        /// The index of the next vertex to be added to the terrain mesh. It's shared between both vertical and
        /// horizontal mesh data.
        /// </summary>
        private int _nextVertexIndex;
        /// <summary>
        /// The vertices that were an outcome of the terrain baking and that will be used to construct the final mesh.
        /// </summary>
        private NativeArray<Vector3> _bakedVertices;
        /// <summary>
        /// The indices that were an outcome of the terrain baking and that will be used to construct the final mesh.
        /// </summary>
        private NativeArray<NativeList<int>> _bakedIndices;
        /// <summary>
        /// Whether this builder has baked its mesh data yet.
        /// </summary>
        private bool _baked;

        #endregion

        #region Setup

        /// <summary>
        /// Creates a <see cref="TerracedMeshBuilder"/>.
        /// </summary>
        /// <param name="vertexCount">The initial number of vertices.</param>
        /// <param name="indicesCount">The initial number of indices.</param>
        /// <param name="terraceCount">The number of terraces to be created.</param>
        /// <param name="allocator">The allocation strategy used when creating vertex and index buffers.</param>
        /// <param name="getVertexHeight">A method that finds the height of a vertex.</param>
        /// <param name="setVertexHeight">A method that returns a copy of a vertex set at a given height.</param>
        internal TerracedMeshBuilder(int vertexCount, int indicesCount, int terraceCount, Allocator allocator, 
	        HeightGetter getVertexHeight, HeightSetter setVertexHeight)
        {
            _terraceCount = terraceCount;
            _getVertexHeight = getVertexHeight;
            _setVertexHeight = setVertexHeight;
            // Both horizontal and vertical mesh data are initialized with the given vertex and indices count because
            // the number of generated data (both vertices and indices) is usually much larger than the provided, 
            // initial values. These initial values are used just to avoid late buffer resizing.
            _horizontalMeshData = new ComplexMeshData(vertexCount, indicesCount, _terraceCount, allocator);
            _verticalMeshData = new ComplexMeshData(vertexCount, indicesCount, _terraceCount, allocator);
        }

        #endregion

        #region Public

        public void Dispose()
        {
            _horizontalMeshData.Dispose();
            _verticalMeshData.Dispose();
            
            foreach (var bakedIndices in _bakedIndices)
            {
                if (bakedIndices.IsCreated)
                    bakedIndices.Dispose();
            }
            _bakedIndices.Dispose();
            
            if (_bakedVertices.IsCreated)
                _bakedVertices.Dispose();
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
            ThrowIfAlreadyBaked();

            var v1 = _setVertexHeight(triangle.V1, height);
            var v2 = _setVertexHeight(triangle.V2, height);
            var v3 = _setVertexHeight(triangle.V3, height);
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
            ThrowIfAlreadyBaked();
            
            // Add floor
            var floor1 = GetPlanePoint(t.V1, t.V3, plane);
            var floor2 = GetPlanePoint(t.V2, t.V3, plane);
            var floor3 = _setVertexHeight(t.V3, plane);
            _horizontalMeshData.AddTriangle(floor1, floor2, floor3, terraceIx, ref _nextVertexIndex);

            // Add wall
            var wall1 = floor2;
            var wall2 = floor1;
            var wall3 = _setVertexHeight(floor1, previousPlane);
            var wall4 = _setVertexHeight(floor2, previousPlane);
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
            ThrowIfAlreadyBaked();
            
            // Add floor
            var floor1 = GetPlanePoint(t.V1, t.V3, plane);
            var floor2 = GetPlanePoint(t.V2, t.V3, plane);
            var floor3 = _setVertexHeight(t.V1, plane);
            var floor4 = _setVertexHeight(t.V2, plane);
            _horizontalMeshData.AddQuadrilateral(floor1, floor3, floor4, floor2, terraceIx, ref _nextVertexIndex);

            // Add wall
            var wall1 = floor1;
            var wall2 = floor2;
            var wall3 = _setVertexHeight(floor2, previousPlane);
            var wall4 = _setVertexHeight(floor1, previousPlane);
            _verticalMeshData.AddQuadrilateral(wall1, wall2, wall3, wall4, terraceIx, ref _nextVertexIndex);
        }

        /// <summary>
        /// Bakes the mesh data. Usually followed by a call to <see cref="Build"/>. The baking and building steps are
        /// separate to allow callers to invoke the baking in a separate thread to maximize performance.
        /// </summary>
        /// <param name="allocator">The allocation strategy used when creating vertex and index buffers.</param>
        internal void Bake(Allocator allocator)
        {
            ThrowIfAlreadyBaked();
            
            // Bake vertices
            _bakedVertices = MergeVertices(_horizontalMeshData.Vertices, _verticalMeshData.Vertices, allocator);
            // Initialize indices list (per terrace).
            _bakedIndices = new NativeArray<NativeList<int>>(_terraceCount, allocator);
            
            // Bake mesh indices data, per terrace/sub mesh
            for (var i = 0; i < _terraceCount; i++)
            {
                var horizontalIndices = _horizontalMeshData.GetIndices(i);
                var verticalIndices = _verticalMeshData.GetIndices(i);
                _bakedIndices[i] = horizontalIndices.Combine(verticalIndices, allocator);
            }

            _baked = true;
            return;
            
            static NativeArray<Vector3> MergeVertices(NativeParallelHashMap<Vector3,int> v1, 
	            NativeParallelHashMap<Vector3,int> v2, Allocator allocator)
            {
                var vertices = new NativeArray<Vector3>(v1.Count() + v2.Count(), allocator);
                AddVertices(v1);
                AddVertices(v2);
                return vertices;

                void AddVertices(NativeParallelHashMap<Vector3, int> v)
                {
                    foreach (var kvp in v)
                        vertices[kvp.Value] = kvp.Key;
                }
            }
        }
        
        /// <summary>
        /// Builds a <see cref="Mesh"/> based on the previously baked mesh data. This method must be called from the
        /// main thread, otherwise the Unity <see cref="Mesh"/> API will throw an exception.
        /// </summary>
        /// <returns>The terraced terrain <see cref="Mesh"/>.</returns>
        /// <exception cref="InvalidOperationException">Thrown if called without previously baking the mesh data
        /// (by calling <see cref="Bake"/>).</exception>
        internal Mesh Build()
        {
            if (!_baked)
                throw new InvalidOperationException("Mesh must be baked before being built.");
            
            var mesh = new Mesh();
            mesh.name = "Terraced Terrain";
            // Merge both vertical and horizontal vertices into a single array.
            var vertexCount = _bakedVertices.Length;
            if (vertexCount > MaxVertexCountUInt16)
                mesh.indexFormat = IndexFormat.UInt32;
            // Each terrace must be a sub mesh to allow for material assignment per terrace.
            mesh.subMeshCount = _terraceCount;
            
            // Set mesh vertex data
            mesh.SetVertices(_bakedVertices, 0, vertexCount);
            
            // Set mesh indices data, per terrace/sub mesh
            for (var i = 0; i < _terraceCount; i++)
            {
                var indices = _bakedIndices[i];
                // SetIndices doesn't have an overload for NativeList, so we need to transform the indices list into
                // an array. Luckily, NativeList implements a method (AsArray) that returns a native array that aliases
                // the content of this list (without copying or allocating for the entire list).
                var indicesArray = indices.AsArray();
                mesh.SetIndices(indicesArray, MeshTopology.Triangles, i);
            }
            
            mesh.RecalculateNormals();
            return mesh;
        }

        #endregion
        
        #region Private

        /// <summary>
        /// Throws an exception if the builder has already baked the mesh data. It can be used to prevent writing more
        /// data after the baking is done. 
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if the baking has already been done.</exception>
        private void ThrowIfAlreadyBaked()
        {
            if (_baked)
                throw new InvalidOperationException("Can't modify builder because the mesh has already been baked");
        }

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
        private Vector3 GetPlanePoint(Vector3 lower, Vector3 higher, float height)
        {
	        var lowerHeight = _getVertexHeight(lower);
	        var higherHeight = _getVertexHeight(higher);
            var t = (height - lowerHeight) / (higherHeight - lowerHeight);
            return Vector3.Lerp(lower, higher, t);
        }

        #endregion
    }
}