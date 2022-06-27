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
            _mesh.triangles = triangles;
            _mesh.vertices = vertices;

            void SliceTriangle(Triangle triangle)
            {
                var planeCount = _planeHeights.Length;
                for (var p = 0; p < planeCount; p++)
                {
                    var isLastPlane = p == planeCount - 1;
                    var planeHeight = _planeHeights[p];
                    var pointsAbove = triangle.GetPointsAbove(planeHeight);

                    switch (pointsAbove)
                    {
                        case 0:
                            // Skip, it has been sliced
                            break;
                        case 1:
                            SlideTriangle1Above(triangle, planeHeight);
                            break;
                        case 2:

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

                void SlideTriangle1Above(Triangle t,float planeHeight)
                {
                    var v1Plane = GetPlanePoint(t.V1, t.V3, planeHeight);
                    var v2Plane = GetPlanePoint(t.V2, t.V3, planeHeight);
                    var v3Plane = new Vector3(t.V3.x, planeHeight, t.V3.z);
                    _meshBuilder.AddTriangle(v1Plane, v2Plane, v3Plane);
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