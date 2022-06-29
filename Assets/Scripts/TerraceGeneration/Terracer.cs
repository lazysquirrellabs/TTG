using System;
using System.Collections.Generic;
using UnityEngine;

namespace SneakySquirrelLabs.TerracedTerrainGenerator.TerraceGeneration
{
    internal sealed class Terracer
    {
        #region Delegates

        private delegate void Slicer(Triangle t, float height, float previousHeight);

        #endregion
        
        #region Fields

        private readonly MeshData _meshData;
        private readonly TerracedMeshBuilder _meshBuilder;
        private readonly float[] _planeHeights;
        private readonly uint _terraces;

        #endregion

        #region Setup

        internal Terracer(MeshData meshData, uint terraces)
        {
            _meshData = meshData;
            // In the base case, there will be at least the same amount of vertices
            _meshBuilder = new TerracedMeshBuilder(_meshData.Vertices.Count, _meshData.Indices.Count);
            _planeHeights = GetHeights(terraces + 1, meshData.Vertices);
            _terraces = terraces;
            
            static float[] GetHeights(uint count, List<Vector3> vertices)
            {
                var lowestPoint = float.PositiveInfinity;
                var highestPoint = float.NegativeInfinity;
                var heights = new float[count];
            
                foreach (var vertex in vertices)
                {
                    lowestPoint = Mathf.Min(vertex.y, lowestPoint);
                    highestPoint = Mathf.Max(vertex.y, highestPoint);
                }
            
                // Ensure that all points will be between the lowest and maximum points
                lowestPoint -= float.Epsilon;
                highestPoint += float.Epsilon;
                var delta = (highestPoint - lowestPoint) / (count - 1);
            
                for (var i = 0; i < count; i++)
                    heights[i] = lowestPoint + i * delta;
            
                return heights;
            }
        }

        #endregion

        #region Internal

        internal Mesh CreateTerraces()
        {
            if (_terraces == 0)
                return _meshBuilder.Build();
            
            var triangleCount = _meshData.Indices.Count / 3;
            var triangleIndex = 0;

            for (var t = 0; t < triangleCount; t++)
            {
                var triangle = new Triangle(_meshData.Indices, _meshData.Vertices, ref triangleIndex);
                AddTriangle(triangle);
            }

            return _meshBuilder.Build();

            void AddTriangle(Triangle t)
            {
                var planeCount = _planeHeights.Length;
                var previousHeight = _planeHeights[0];
                var added = false;

                for (var p = 1; p < planeCount; p++)
                {
                    var height = _planeHeights[p];
                    SliceTriangleAtHeight(height);
                }

                void SliceTriangleAtHeight(float height)
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
                        if (!added)
                            PlacePlane();
                        slice(t, height, previousHeight);
                        added = true;
                    }
                
                    void PlacePlane()
                    {
                        _meshBuilder.AddWholeTriangle(t, previousHeight);
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
        
        #endregion
    }
}