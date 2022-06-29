using UnityEngine;
using UnityEngine.Rendering;

namespace SneakySquirrelLabs.TerracedTerrainGenerator.TerraceGeneration
{
    internal sealed class TerracedMeshBuilder
    {
        #region Fields

        private const int MaxVertexCountUInt16 = 65_535;
        private readonly MeshData _meshData;

        #endregion

        #region Setup

        internal TerracedMeshBuilder(int vertexCount, int indicesCount)
        {
            _meshData = new MeshData(vertexCount, indicesCount);
        }

        #endregion

        #region Internal

        internal Mesh Build()
        {
            var mesh = new Mesh();
            mesh.name = "Terraced Terrain";
            if (_meshData.Vertices.Count > MaxVertexCountUInt16)
                mesh.indexFormat = IndexFormat.UInt32;
            mesh.SetVertices(_meshData.Vertices);
            mesh.SetTriangles(_meshData.Indices, 0);
            mesh.RecalculateNormals();
            return mesh;
        }
        
        internal void AddWholeTriangle(Triangle triangle, float height)
        {
            var v1 = new Vector3(triangle.V1.x, height, triangle.V1.z);
            var v2 = new Vector3(triangle.V2.x, height, triangle.V2.z);
            var v3 = new Vector3(triangle.V3.x, height, triangle.V3.z);
            _meshData.AddTriangle(v1, v2, v3);
        }
        
        internal void AddSlicedTriangle1Above(Triangle t, float plane, float previousPlane)
        {
            // Add floor
            var v13Plane = GetPlanePoint(t.V1, t.V3, plane);
            var v23Plane = GetPlanePoint(t.V2, t.V3, plane);
            var v3Plane = new Vector3(t.V3.x, plane, t.V3.z);
            _meshData.AddTriangle(v13Plane, v23Plane, v3Plane);

            // Add wall
            var v13PreviousPlane = new Vector3(v13Plane.x, previousPlane, v13Plane.z);
            var v23PreviousPlane = new Vector3(v23Plane.x, previousPlane, v23Plane.z);
            _meshData.AddQuadrilateral(v23Plane, v13Plane, v13PreviousPlane, v23PreviousPlane);
        }
        
        internal void AddSlicedTriangle2Above(Triangle t, float plane, float previousPlane)
        {
            // Add floor
            var v13Plane = GetPlanePoint(t.V1, t.V3, plane);
            var v23Plane = GetPlanePoint(t.V2, t.V3, plane);
            var v1Plane = new Vector3(t.V1.x, plane, t.V1.z);
            var v2Plane = new Vector3(t.V2.x, plane, t.V2.z);
            _meshData.AddQuadrilateral(v13Plane, v1Plane, v2Plane, v23Plane);

            // Add wall
            var v13PreviousPlane = new Vector3(v13Plane.x, previousPlane, v13Plane.z);
            var v23PreviousPlane = new Vector3(v23Plane.x, previousPlane, v23Plane.z);
            _meshData.AddQuadrilateral(v13Plane, v23Plane, v23PreviousPlane, v13PreviousPlane);
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