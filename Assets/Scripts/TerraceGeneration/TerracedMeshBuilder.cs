using System.Collections.Generic;
using SneakySquirrelLabs.TerracedTerrainGenerator.Data;
using SneakySquirrelLabs.TerracedTerrainGenerator.Utils;
using UnityEngine;
using UnityEngine.Rendering;
using Vector3 = UnityEngine.Vector3;

namespace SneakySquirrelLabs.TerracedTerrainGenerator.TerraceGeneration
{
    internal sealed class TerracedMeshBuilder
    {
        #region Fields

        private const int MaxVertexCountUInt16 = 65_535;
        private readonly ComplexMeshData _horizontalMeshData;
        private readonly ComplexMeshData _verticalMeshData;
        private readonly int _terraceCount;
        private int _vertexIndex;

        #endregion

        #region Setup

        internal TerracedMeshBuilder(int vertexCount, int indicesCount, int terraceCount)
        {
            _terraceCount = terraceCount;
            _horizontalMeshData = new ComplexMeshData(vertexCount, indicesCount, terraceCount);
            _verticalMeshData = new ComplexMeshData(vertexCount, indicesCount, terraceCount);
        }

        #endregion

        #region Internal

        internal Mesh Build()
        {
            var mesh = new Mesh();
            mesh.name = "Terraced Terrain";
            var vertices = MergeVertices(_horizontalMeshData.Vertices, _verticalMeshData.Vertices);
            var vertexCount = vertices.Length;
            if (vertexCount > MaxVertexCountUInt16)
                mesh.indexFormat = IndexFormat.UInt32;
            mesh.subMeshCount = _terraceCount;
            
            mesh.SetVertices(vertices, 0, vertexCount);
            
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
        
        internal void AddWholeTriangle(Triangle triangle, float height, int terraceIx)
        {
            var v1 = new Vector3(triangle.V1.x, height, triangle.V1.z);
            var v2 = new Vector3(triangle.V2.x, height, triangle.V2.z);
            var v3 = new Vector3(triangle.V3.x, height, triangle.V3.z);
            _horizontalMeshData.AddTriangle(v1, v2, v3, terraceIx, ref _vertexIndex);
        }
        
        internal void AddSlicedTriangle1Above(Triangle t, float plane, float previousPlane, int terraceIx)
        {
            // Add floor
            var v13Plane = GetPlanePoint(t.V1, t.V3, plane);
            var v23Plane = GetPlanePoint(t.V2, t.V3, plane);
            var v3Plane = new Vector3(t.V3.x, plane, t.V3.z);
            _horizontalMeshData.AddTriangle(v13Plane, v23Plane, v3Plane, terraceIx, ref _vertexIndex);

            // Add wall
            var v13PreviousPlane = new Vector3(v13Plane.x, previousPlane, v13Plane.z);
            var v23PreviousPlane = new Vector3(v23Plane.x, previousPlane, v23Plane.z);
            _verticalMeshData.AddQuadrilateral(v23Plane, v13Plane, v13PreviousPlane, v23PreviousPlane, terraceIx, ref _vertexIndex);
        }
        
        internal void AddSlicedTriangle2Above(Triangle t, float plane, float previousPlane, int terraceIx)
        {
            // Add floor
            var v13Plane = GetPlanePoint(t.V1, t.V3, plane);
            var v23Plane = GetPlanePoint(t.V2, t.V3, plane);
            var v1Plane = new Vector3(t.V1.x, plane, t.V1.z);
            var v2Plane = new Vector3(t.V2.x, plane, t.V2.z);
            _horizontalMeshData.AddQuadrilateral(v13Plane, v1Plane, v2Plane, v23Plane, terraceIx, ref _vertexIndex);

            // Add wall
            var v13PreviousPlane = new Vector3(v13Plane.x, previousPlane, v13Plane.z);
            var v23PreviousPlane = new Vector3(v23Plane.x, previousPlane, v23Plane.z);
            _verticalMeshData.AddQuadrilateral(v13Plane, v23Plane, v23PreviousPlane, v13PreviousPlane, terraceIx, ref _vertexIndex);
        }

        #endregion
        
        #region Private

        private static Vector3 GetPlanePoint(Vector3 lower, Vector3 higher, float planeHeight)
        {
            var t = (planeHeight - lower.y) / (higher.y - lower.y);
            return Vector3.Lerp(lower, higher, t);
        }

        #endregion
    }
}