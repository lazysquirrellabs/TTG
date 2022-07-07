using System;
using System.Collections.Generic;
using SneakySquirrelLabs.TerracedTerrainGenerator.Data;
using UnityEngine;

namespace SneakySquirrelLabs.TerracedTerrainGenerator.TerraceGeneration
{
    internal sealed class Terracer
    {
        #region Delegates

        private delegate void Slicer(Triangle t, float height, float previousHeight, int terraceIndex);

        #endregion
        
        #region Fields

        private readonly SimpleMeshData _meshData;
        private readonly TerracedMeshBuilder _meshBuilder;
        private readonly float[] _planeHeights;
        private readonly int _terraces;

        #endregion

        #region Setup

        internal Terracer(SimpleMeshData meshData, int terraces)
        {
            _meshData = meshData;
            // In the base case, there will be at least the same amount of vertices
            _meshBuilder = new TerracedMeshBuilder(_meshData.Indices.Count, terraces);
            // Two extra planes are placed: one below and one above all points. This helps the algorithm.
            var planeCount = terraces + 2;
            _planeHeights = GetHeights(planeCount, meshData.Vertices);
            _terraces = terraces;
            
            static float[] GetHeights(int count, List<Vector3> vertices)
            {
                var lowestPoint = float.PositiveInfinity;
                var highestPoint = float.NegativeInfinity;
                var heights = new float[count];
            
                foreach (var vertex in vertices)
                {
                    lowestPoint = Mathf.Min(vertex.y, lowestPoint);
                    highestPoint = Mathf.Max(vertex.y, highestPoint);
                }
            
                // // Ensure that all points are above the lowest plane
                lowestPoint -= float.Epsilon;
                var delta = (highestPoint - lowestPoint) / (count - 1);
            
                for (var i = 1; i < count; i++)
                    heights[i] = lowestPoint + i * delta;

                // Ensure that all points are below the highest plane
                heights[count - 1] += 0.001f;
            
                return heights;
            }
        }

        #endregion

        #region Internal

        internal void CreateTerraces()
        {
            if (_terraces == 0)
                return;
            
            var triangleCount = _meshData.Indices.Count / 3;
            var triangleIndex = 0;

            for (var t = 0; t < triangleCount; t++)
            {
                var triangle = new Triangle(_meshData.Indices, _meshData.Vertices, ref triangleIndex);
                AddTriangle(triangle);
            }

            void AddTriangle(Triangle t)
            {
                var planeCount = _planeHeights.Length;
                var previousHeight = _planeHeights[0];
                var added = false;

                // Loop through terraces, except the last one
                for (var terraceIx = 0; terraceIx < _terraces - 1; terraceIx++)
                {
                    // There is one plane below the first terrace, so offset its index by 1
                    var terraceHeight = _planeHeights[terraceIx + 1];
                    SliceTriangleAtHeight(terraceHeight, terraceIx);
                }
                
                // Handle the last terrace
                var lastHeight = _planeHeights[planeCount - 1];
                var lastTerraceIx = _terraces - 1;
                SliceTriangleAtHeight(lastHeight, lastTerraceIx);

                void SliceTriangleAtHeight(float height, int terraceIx)
                {
                    int pointsAbove;
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
                        case 1:
                            SliceTriangle(_meshBuilder.AddSlicedTriangle1Above);
                            break;
                        case 2:
                            SliceTriangle(_meshBuilder.AddSlicedTriangle2Above);
                            break;
                        // Ignore. Plane hasn't reached triangle yet.
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
                        slice(t, height, previousHeight, terraceIx + 1);
                        added = true;
                    }
                
                    void PlacePlane()
                    {
                        _meshBuilder.AddWholeTriangle(t, previousHeight, terraceIx);
                        added = true;
                    }
                
                    static (Triangle, int) RearrangeAccordingToPlane(Triangle triangle, float planeHeight)
                    {
                        var v1Below = triangle.V1.y < planeHeight;
                        var v2Below = triangle.V2.y < planeHeight;
                        var v3Below = triangle.V3.y < planeHeight;

                        if (v1Below)
                        {
                            if (v2Below)
                                return triangle.V3.y < planeHeight ? (triangle, 0) : (triangle, 1);

                            if (v3Below)
                            {
                                triangle = new Triangle(triangle.V3, triangle.V1, triangle.V2);
                                return (triangle, 1);
                            }

                            triangle = new Triangle(triangle.V2, triangle.V3, triangle.V1);
                            return (triangle, 2);
                        }

                        if (!v2Below) 
                            return v3Below ? (triangle, 2) : (triangle, 3);
                
                        if (v3Below)
                        {
                            triangle = new Triangle(triangle.V2, triangle.V3, triangle.V1);
                            return (triangle, 1);
                        }

                        triangle = new Triangle(triangle.V3, triangle.V1, triangle.V2);
                        return (triangle, 2);

                    }
                }
            }
        }

        internal Mesh CreateMesh()
        {
            return _meshBuilder.Build();
        }
        
        #endregion

    }
}