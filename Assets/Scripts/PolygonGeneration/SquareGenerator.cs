using UnityEngine;

namespace SneakySquirrelLabs.TerracedTerrainGenerator.PolygonGeneration
{
    internal class SquareGenerator : PolygonGenerator
    {
        #region Setup
        
        public SquareGenerator(float radius) : base(radius) { }
        
        #endregion

        #region Internal

        internal override Mesh Generate()
        {
            var mesh = new Mesh
            {
                vertices = CreateVertices(Radius), 
                triangles = new[] {0, 1, 2, 0, 2, 3},
                normals = new [] {Vector3.up, Vector3.up, Vector3.up , Vector3.up },
                name = "Terraced Terrain Mesh"
            };
            return mesh;

            static Vector3[] CreateVertices(float radius)
            {
                var vertices = new Vector3[4];
                vertices[0] = new Vector3(-radius, 0f, radius);
                vertices[1] = new Vector3(radius, 0f, radius);
                vertices[2] = new Vector3(radius, 0f, -radius);
                vertices[3] = new Vector3(-radius, 0f, -radius);
                return vertices;
            }
        }

        #endregion
    }
}