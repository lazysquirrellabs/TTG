using SneakySquirrelLabs.TerracedTerrainGenerator.Data;
using UnityEngine;
using UnityEngine.Rendering;

namespace SneakySquirrelLabs.TerracedTerrainGenerator.TerraceGeneration
{
    internal sealed class TerracedMeshBuilder
    {
        #region Fields

        private const int MaxVertexCountUInt16 = 65_535;
        private readonly ComplexMeshData _meshData;
        private readonly int _terraceCount;

        #endregion

        #region Setup

        internal TerracedMeshBuilder(int terraceCount, int vertexCount, int indicesCount)
        {
            _terraceCount = terraceCount;
            _meshData = new ComplexMeshData(terraceCount, vertexCount, indicesCount);
        }

        #endregion

        #region Internal

        internal Mesh Build()
        {
            var mesh = new Mesh();
            mesh.name = "Terraced Terrain";
            var vertexCount = _meshData.Vertices.Count;
            if (vertexCount > MaxVertexCountUInt16)
                mesh.indexFormat = IndexFormat.UInt32;
            mesh.subMeshCount = _terraceCount;
            mesh.SetVertices(_meshData.Vertices, 0, vertexCount);
            
            for (var i = 0; i < _terraceCount; i++)
            {
                var indices = _meshData.GetIndices(i);
                mesh.SetTriangles(indices, 0, indices.Count, i);
            }
            
            mesh.RecalculateNormals();
            return mesh;
        }
        
        internal void AddWholeTriangle(Triangle triangle, float height, int terraceIx)
        {
            var v1 = new Vector3(triangle.V1.x, height, triangle.V1.z);
            var v2 = new Vector3(triangle.V2.x, height, triangle.V2.z);
            var v3 = new Vector3(triangle.V3.x, height, triangle.V3.z);
            _meshData.AddTriangle(v1, v2, v3, terraceIx);
        }
        
        internal void AddSlicedTriangle1Above(Triangle t, float plane, float previousPlane, int terraceIx)
        {
            // Add floor
            var v13Plane = GetPlanePoint(t.V1, t.V3, plane);
            var v23Plane = GetPlanePoint(t.V2, t.V3, plane);
            var v3Plane = new Vector3(t.V3.x, plane, t.V3.z);
            _meshData.AddTriangle(v13Plane, v23Plane, v3Plane, terraceIx);

            // Add wall
            var v13PreviousPlane = new Vector3(v13Plane.x, previousPlane, v13Plane.z);
            var v23PreviousPlane = new Vector3(v23Plane.x, previousPlane, v23Plane.z);
            _meshData.AddQuadrilateral(v23Plane, v13Plane, v13PreviousPlane, v23PreviousPlane, terraceIx);
        }
        
        internal void AddSlicedTriangle2Above(Triangle t, float plane, float previousPlane, int terraceIx)
        {
            // Add floor
            var v13Plane = GetPlanePoint(t.V1, t.V3, plane);
            var v23Plane = GetPlanePoint(t.V2, t.V3, plane);
            var v1Plane = new Vector3(t.V1.x, plane, t.V1.z);
            var v2Plane = new Vector3(t.V2.x, plane, t.V2.z);
            _meshData.AddQuadrilateral(v13Plane, v1Plane, v2Plane, v23Plane, terraceIx);

            // Add wall
            var v13PreviousPlane = new Vector3(v13Plane.x, previousPlane, v13Plane.z);
            var v23PreviousPlane = new Vector3(v23Plane.x, previousPlane, v23Plane.z);
            _meshData.AddQuadrilateral(v13Plane, v23Plane, v23PreviousPlane, v13PreviousPlane, terraceIx);
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