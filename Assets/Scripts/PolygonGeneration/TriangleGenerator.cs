using UnityEngine;

namespace SneakySquirrelLabs.TerracedTerrainGenerator.PolygonGeneration
{
    internal class TriangleGenerator : PolygonGenerator
    {
        #region Setup

        internal TriangleGenerator(float radius) : base(radius) { }

        #endregion

        #region Internal

        internal override Mesh Generate()
        {
            var mesh = new Mesh
            {
                vertices = CreateVertices(Radius), 
                triangles = new[] {0, 1, 2},
                normals = new [] {Vector3.up, Vector3.up, Vector3.up },
                name = "Terraced Terrain Mesh"
            };
            return mesh;

            static Vector3[] CreateVertices(float radius)
            {
                var vertices = new Vector3[3];
                vertices[0] = new Vector3(radius, 0f, 0f);
                vertices[1] = Rotate(vertices[0], -120);
                vertices[2] = Rotate(vertices[1], -120);
                return vertices;

                static Vector3 Rotate(Vector3 point, float degrees)
                {
                    var angle = degrees * Mathf.Deg2Rad;
                    var cos = Mathf.Cos(angle);
                    var sin = Mathf.Sin(angle);
                    var newX = point.x * cos - point.z * sin;
                    var newZ = point.x * sin + point.z * cos;
                    return new Vector3(newX, point.y, newZ);
                }
            }
        }

        #endregion
    }
}