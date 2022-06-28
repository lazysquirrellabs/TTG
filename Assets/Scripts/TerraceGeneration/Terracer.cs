using System;
using System.Collections.Generic;
using UnityEngine;

namespace SneakySquirrelLabs.TerracedTerrainGenerator.TerraceGeneration
{
    internal sealed class Terracer
    {
        #region Fields

        private readonly Mesh _mesh;
        private readonly MeshBuilder _meshBuilder;
        private readonly float[] _planeHeights;
        private readonly uint _terraces;
        private readonly List<Vector3> _originalVertices;
        private readonly List<int> _originalTriangles;

        #endregion

        #region Setup

        internal Terracer(Mesh mesh, uint terraces)
        {
            _mesh = mesh;
            // In the base case, there will be at least the same amount of vertices
            _meshBuilder = new MeshBuilder(mesh.vertexCount, mesh.triangles.Length);
            var originalVertexCount = mesh.vertexCount;
            _originalVertices = new List<Vector3>(originalVertexCount);
            mesh.GetVertices(_originalVertices);
            _originalTriangles = new List<int>(3 * originalVertexCount);
            mesh.GetTriangles(_originalTriangles, 0);
            _planeHeights = GetHeights(terraces + 1, _originalVertices);

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

                var delta = (highestPoint - lowestPoint) / (count - 1);

                for (var i = 0; i < count; i++)
                    heights[i] = lowestPoint + i * delta;

                return heights;
            }
        }

        #endregion

        #region Internal

        internal void CreateTerraces()
        {
            var triangleCount = _originalTriangles.Count / 3;
            var originalTriangleIndex = 0;

            for (var t = 0; t < triangleCount; t++)
            {
                var triangle = new Triangle(_originalTriangles, _originalVertices, ref originalTriangleIndex);
                SliceTriangle(triangle);
            }

            var vertices = _meshBuilder.Vertices;
            var triangles = _meshBuilder.Triangles;
            _mesh.vertices = vertices;
            _mesh.triangles = triangles;

            void SliceTriangle(Triangle triangle)
            {
                var planeCount = _planeHeights.Length;
                for (var p = 0; p < planeCount; p++)
                {
                    var isLastPlane = p == planeCount - 1;
                    var planeHeight = _planeHeights[p];
                    int pointsAbove;
                    (triangle, pointsAbove) = RearrangeAccordingToPlane(triangle, planeHeight);

                    switch (pointsAbove)
                    {
                        case 0:
                            // Skip, it has been sliced
                            break;
                        case 1:
                            SliceTriangle1Above(triangle, planeHeight);
                            break;
                        case 2:
                            SliceTriangle2Above(triangle, planeHeight);
                            break;
                        case 3 when !isLastPlane:
                            // Skip, will be caught by another plane.
                            break;
                        case 3:
                            Debug.LogWarning($"Triangle {triangle} above last plane. This should not happen.");
                            break;
                        default:
                            throw new NotSupportedException($"It's impossible to have {pointsAbove} above.");
                    }
                }

                void SliceTriangle1Above(Triangle t,float planeHeight)
                {
                    var v13Plane = GetPlanePoint(t.V1, t.V3, planeHeight);
                    var v23Plane = GetPlanePoint(t.V2, t.V3, planeHeight);
                    var v3Plane = new Vector3(t.V3.x, planeHeight, t.V3.z);
                    _meshBuilder.AddTriangle(v13Plane, v23Plane, v3Plane);
                }
                
                void SliceTriangle2Above(Triangle t,float planeHeight)
                {
                    var v13Plane = GetPlanePoint(t.V1, t.V3, planeHeight);
                    var v23Plane = GetPlanePoint(t.V2, t.V3, planeHeight);
                    var v1Plane = new Vector3(t.V1.x, planeHeight, t.V1.z);
                    var v2Plane = new Vector3(t.V2.x, planeHeight, t.V2.z);
                    _meshBuilder.AddQuadrilateral(v13Plane, v1Plane, v2Plane, v23Plane);
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

                    if (v2Below)
                    {
                        if (v3Below)
                        {
                            triangle = new Triangle(triangle.V2, triangle.V3, triangle.V1);
                            return (triangle, 1);
                        }
                        
                        triangle = new Triangle(triangle.V3, triangle.V1, triangle.V2);
                        return (triangle, 2);
                    }

                    return v3Below ? (triangle, 2) : (triangle, 3);
                }
                
                static Vector3 GetPlanePoint(Vector3 v1, Vector3 v2, float planeHeight)
                {
                    var t = (planeHeight - v1.y) / (v2.y - v1.y);
                    return Vector3.Lerp(v1, v2, t);
                }
            }
        }
        
        #endregion
    }
}