using System;
using SneakySquirrelLabs.TerracedTerrainGenerator.Data;
using Unity.Collections;
using UnityEngine;

namespace SneakySquirrelLabs.TerracedTerrainGenerator.TerraceGeneration
{
    /// <summary>
    /// Modified an existing terrain mesh by creating terraces on it. The strategy used is similar to the one described
    /// in https://icospheric.com/blog/2016/07/17/making-terraced-terrain/.
    /// </summary>
    internal sealed class Terracer : IDisposable
    {
        #region Delegates

        /// <summary>
        /// Delegate used to slice a given <see cref="Triangle"/>.
        /// </summary>
        private delegate void Slicer(Triangle t, float height, float previousHeight, int terraceIndex);

        #endregion
        
        #region Fields

        /// <summary>
        /// The mesh data of the original terrain. Used only to read data from and is left unmodified.
        /// </summary>
        private readonly SimpleMeshData _meshData;
        /// <summary>
        /// The mesh builder used to build the new, terraced terrain.
        /// </summary>
        private readonly TerracedMeshBuilder _meshBuilder;
        /// <summary>
        /// The height of all the planes used to slice the triangles.
        /// </summary>
        private readonly NativeArray<float> _planeHeights;
        /// <summary>
        /// The number of terraces to be created.
        /// </summary>
        private readonly int _terraceCount;

        #endregion

        #region Setup

        /// <summary>
        /// Creates a new <see cref="Terracer"/>.
        /// </summary>
        /// <param name="meshData">The terrain's original mesh data. It will be used to read data from and remains
        /// unmodified.</param>
        /// <param name="terraceHeights">The heights of all terraces (in units), in ascending order.</param>
        /// <param name="allocator">The allocation strategy used when creating vertex and index buffers.</param>
        internal Terracer(SimpleMeshData meshData, float[] terraceHeights, Allocator allocator)
        {
	        _meshData = meshData;
	        // In the base case, there will be at least the same amount of vertices
	        var vertexCount = _meshData.Vertices.Length;
	        var indexCount = _meshData.Indices.Length;
	        _terraceCount = terraceHeights.Length;
	        _meshBuilder = new TerracedMeshBuilder(vertexCount, indexCount, _terraceCount, allocator);
	        _planeHeights = GetHeights(_terraceCount, terraceHeights, allocator);
            
	        static NativeArray<float> GetHeights(int count, float[] terraceHeights, Allocator allocator)
	        {
		        var heights = new NativeArray<float>(count, allocator);
		        NativeArray<float>.Copy(terraceHeights, 0, heights, 0, terraceHeights.Length);
		        return heights;
	        }
        }

        #endregion

        #region Public

        public void Dispose()
        {
            _meshData?.Dispose();
            _meshBuilder?.Dispose();
            var heights = _planeHeights;
            if (heights.IsCreated)
                heights.Dispose();
        }

        #endregion
        
        #region Internal

        /// <summary>
        /// Creates the terraces, but doesn't create the <see cref="Mesh"/> yet.
        /// </summary>
        /// <exception cref="NotSupportedException">Thrown whenever a triangle has more than 3 vertices below a given
        /// place (which should be impossible, because it's a triangle). </exception>
        internal void CreateTerraces()
        {
            if (_terraceCount == 0)
                return;

            Slicer addSlicedTriangle1Above = _meshBuilder.AddSlicedTriangle1Above;
            Slicer addSlicedTriangle2Above = _meshBuilder.AddSlicedTriangle2Above;
            var triangleCount = _meshData.Indices.Length / 3;
            var triangleIndex = 0;

            // Loop through all triangles, slicing each one
            for (var t = 0; t < triangleCount; t++)
            {
                var triangle = new Triangle(_meshData.Indices, _meshData.Vertices, ref triangleIndex);
                SliceAndAddTriangle(triangle);
            }

            void SliceAndAddTriangle(Triangle t)
            {
                var planeCount = _planeHeights.Length;
                // Keep track of the previous plane because some slice cases need to add triangles on it
                var previousHeight = _planeHeights[0];
                var added = false;

                // Loop through all planes, except the last one, and try to slice the triangle at the current one
                for (var terraceIx = 0; terraceIx < _terraceCount - 1; terraceIx++)
                {
                    // There is one plane below the first terrace, so offset its index by 1
                    var terraceHeight = _planeHeights[terraceIx + 1];
                    SliceTriangleAtHeight(terraceHeight, terraceIx, true);
                }
                
                // Handle the last terrace separately because it's a special case
                var lastHeight = _planeHeights[planeCount - 1];
                var lastTerraceIx = _terraceCount - 1;
                SliceTriangleAtHeight(lastHeight, lastTerraceIx, false);

                void SliceTriangleAtHeight(float height, int terraceIx, bool placeOnTerraceAbove)
                {
                    int pointsAbove;
                    // Rearrange the triangle based on how many of its vertices are above the given plane, and store
                    // that number as well. The rearranging is just to simplify the slicing algorithm. The number of
                    // points above is necessary for further calculation.
                    (t, pointsAbove) = RearrangeAccordingToPlane(t, height);
                    switch (pointsAbove)
                    {
                        // Triangle is between previous and current planes, add it and stop.
                        case 0 when !added:
                            PlacePlane();
                            return;
                        // Triangle was added and the current plane is above it, so stop.
                        case 0: 
                            return;
                        // Triangle has 1 point above the current plane. Slice it and continue.
                        case 1:
                            SliceTriangle(addSlicedTriangle1Above);
                            break;
                        // Triangle has 2 points above the current plane. Slice it and continue.
                        case 2:
                            SliceTriangle(addSlicedTriangle2Above);
                            break;
                        // Plane hasn't reached triangle yet. Skip and continue. 
                        case 3: 
                            break;
                        default:
                            throw new NotSupportedException($"Points above not supported: {pointsAbove}");
                    }
                    
                    previousHeight = height;
                    
                    void SliceTriangle(Slicer slice)
                    {
                        // If this is the fist slice, place the bottom triangle
                        if (!added)
                            PlacePlane();
                        var terraceIncrement = placeOnTerraceAbove ? 1 : 0;
                        // Actually slice the triangle.
                        slice(t, height, previousHeight, terraceIx + terraceIncrement);
                        added = true;
                    }
                
                    void PlacePlane()
                    {
                        // Fully places the triangle as a plane parallel to the terraces.
                        _meshBuilder.AddWholeTriangle(t, previousHeight, terraceIx);
                        added = true;
                    }
                    
                    static (Triangle, int) RearrangeAccordingToPlane(Triangle triangle, float planeHeight)
                    {
	                    var v1 = triangle.V1;
	                    var v2 = triangle.V2;
	                    var v3 = triangle.V3;
                        var v1Below = v1.y < planeHeight;
                        var v2Below = v2.y < planeHeight;
                        var v3Below = v3.y < planeHeight;

                        if (v1Below)
                        {
                            if (v2Below)
                                return v3.y < planeHeight ? (triangle, 0) : (triangle, 1);

                            if (v3Below)
                            {
                                triangle = new Triangle(v3, v1, v2);
                                return (triangle, 1);
                            }

                            triangle = new Triangle(v2, v3, v1);
                            return (triangle, 2);
                        }

                        if (!v2Below) 
                            return v3Below ? (triangle, 2) : (triangle, 3);
                
                        if (v3Below)
                        {
                            triangle = new Triangle(v2, v3, v1);
                            return (triangle, 1);
                        }

                        triangle = new Triangle(v3, v1, v2);
                        return (triangle, 2);
                    }
                }
            }
        }

        /// <summary>
        /// Bakes the terraced mesh data. Usually followed by a call to <see cref="CreateMesh"/>. The baking and
        /// creation steps are separate to allow callers to invoke the baking in a separate thread to maximize
        /// performance.
        /// </summary>
        /// <param name="allocator">The allocation strategy used during the baking.</param>
        internal void BakeMeshData(Allocator allocator)
        {
            _meshBuilder.Bake(allocator);
        }
        
        /// <summary>
        /// Actually creates the previously baked terraced terrain mesh. This method must be called from the
        /// main thread, otherwise the Unity <see cref="Mesh"/> API will throw an exception.
        /// </summary>
        /// <returns>The terraced terrain's mesh.</returns>
        internal Mesh CreateMesh()
        {
            return _meshBuilder.Build();
        }
        
        #endregion
    }
}