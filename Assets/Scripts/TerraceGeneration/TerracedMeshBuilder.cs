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
            var floor1 = GetPlanePoint(t.V1, t.V3, plane);
            var floor2 = GetPlanePoint(t.V2, t.V3, plane);
            var floor3 = new Vector3(t.V3.x, plane, t.V3.z);
            _horizontalMeshData.AddTriangle(floor1, floor2, floor3, terraceIx, ref _vertexIndex);

            // Add wall
            var wall1 = floor2;
            var wall2 = floor1;
            var wall3 = new Vector3(floor1.x, previousPlane, floor1.z);
            var wall4 = new Vector3(floor2.x, previousPlane, floor2.z);
            _verticalMeshData.AddQuadrilateral(wall1, wall2, wall3, wall4, terraceIx, ref _vertexIndex);
        }
        
        internal void AddSlicedTriangle2Above(Triangle t, float plane, float previousPlane, int terraceIx)
        {
            // Add floor
            var floor1 = GetPlanePoint(t.V1, t.V3, plane);
            var floor2 = GetPlanePoint(t.V2, t.V3, plane);
            var floor3 = new Vector3(t.V1.x, plane, t.V1.z);
            var floor4 = new Vector3(t.V2.x, plane, t.V2.z);
            _horizontalMeshData.AddQuadrilateral(floor1, floor3, floor4, floor2, terraceIx, ref _vertexIndex);

            // Add wall
            var wall1 = floor1;
            var wall2 = floor2;
            var wall3 = new Vector3(floor2.x, previousPlane, floor2.z);
            var wall4 = new Vector3(floor1.x, previousPlane, floor1.z);
            _verticalMeshData.AddQuadrilateral(wall1, wall2, wall3, wall4, terraceIx, ref _vertexIndex);
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