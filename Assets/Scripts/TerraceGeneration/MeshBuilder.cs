using System.Collections.Generic;
using UnityEngine;

namespace SneakySquirrelLabs.TerracedTerrainGenerator.TerraceGeneration
{
    internal sealed class MeshBuilder
    {
        #region Fields

        private readonly List<Vector3> _vertices;
        private readonly List<int> _indices;
        private int _nextVertexIndex;
        private int _nextTriangleIndex;

        #endregion

        #region Properties

        internal Vector3[] Vertices => _vertices.ToArray();
        internal int[] Triangles => _indices.ToArray();

        #endregion

        #region Setup

        internal MeshBuilder(int vertexCount, int triangleCount)
        {
            _vertices = new List<Vector3>(vertexCount);
            _indices = new List<int>(triangleCount);
        }

        #endregion

        #region Internal

        internal void AddWholeTriangle(Triangle triangle, float height)
        {
            var v1 = new Vector3(triangle.V1.x, height, triangle.V1.z);
            var v2 = new Vector3(triangle.V2.x, height, triangle.V2.z);
            var v3 = new Vector3(triangle.V3.x, height, triangle.V3.z);
            AddTriangle(v1, v2, v3);
        }
        
        internal void AddSlicedTriangle1Above(Triangle t, float plane, float previousPlane)
        {
            // Add floor
            var v13Plane = GetPlanePoint(t.V1, t.V3, plane);
            var v23Plane = GetPlanePoint(t.V2, t.V3, plane);
            var v3Plane = new Vector3(t.V3.x, plane, t.V3.z);
            AddTriangle(v13Plane, v23Plane, v3Plane);

            // Add wall
            var v13PreviousPlane = new Vector3(v13Plane.x, previousPlane, v13Plane.z);
            var v23PreviousPlane = new Vector3(v23Plane.x, previousPlane, v23Plane.z);
            AddQuadrilateral(v23Plane, v13Plane, v13PreviousPlane, v23PreviousPlane);
        }
        
        internal void AddSlicedTriangle2Above(Triangle t, float plane, float previousPlane)
        {
            // Add floor
            var v13Plane = GetPlanePoint(t.V1, t.V3, plane);
            var v23Plane = GetPlanePoint(t.V2, t.V3, plane);
            var v1Plane = new Vector3(t.V1.x, plane, t.V1.z);
            var v2Plane = new Vector3(t.V2.x, plane, t.V2.z);
            AddQuadrilateral(v13Plane, v1Plane, v2Plane, v23Plane);

            // Add wall
            var v13PreviousPlane = new Vector3(v13Plane.x, previousPlane, v13Plane.z);
            var v23PreviousPlane = new Vector3(v23Plane.x, previousPlane, v23Plane.z);
            AddQuadrilateral(v13Plane, v23Plane, v23PreviousPlane, v13PreviousPlane);
        }

        #endregion
        
        #region Private

        private void AddTriangle(Vector3 v1, Vector3 v2, Vector3 v3)
        {
            _vertices.Add(v1);
            _vertices.Add(v2);
            _vertices.Add(v3);
            _indices.Add(_nextVertexIndex);
            _nextVertexIndex++;
            _indices.Add(_nextVertexIndex);
            _nextVertexIndex++;
            _indices.Add(_nextVertexIndex);
            _nextVertexIndex++;
        }

        /// <summary>
        /// Add a quadrilateral to the mesh. Points are provided in clockwise order as seen from the rendered surface.
        /// </summary>
        /// <param name="v1">The bottom left corner of the quadrilateral.</param>
        /// <param name="v2">The top left corner of the quadrilateral.</param>
        /// <param name="v3">The top right corner of the quadrilateral.</param>
        /// <param name="v4">The bottom right corner of the quadrilateral.</param>
        private void AddQuadrilateral(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4)
        {
            // Add all vertices
            _vertices.Add(v1);
            _vertices.Add(v2);
            _vertices.Add(v3);
            _vertices.Add(v4);
            // Calculate each vertex's index
            var index1 = _nextVertexIndex;
            _nextVertexIndex++;
            var index2 = _nextVertexIndex;
            _nextVertexIndex++;
            var index3 = _nextVertexIndex;
            _nextVertexIndex++;
            var index4 = _nextVertexIndex;
            _nextVertexIndex++;
            // Add triangle 1
            _indices.Add(index1);
            _indices.Add(index2);
            _indices.Add(index4);
            // Add triangle 2
            _indices.Add(index2);
            _indices.Add(index3);
            _indices.Add(index4);
        }
        
        private static Vector3 GetPlanePoint(Vector3 lower, Vector3 higher, float planeHeight)
        {
            var t = (planeHeight - lower.y) / (higher.y - lower.y);
            return Vector3.Lerp(lower, higher, t);
        }

        #endregion
    }
}