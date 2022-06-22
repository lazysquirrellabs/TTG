using UnityEngine;

namespace SneakySquirrelLabs.TerracedTerrainGenerator.PolygonGeneration
{
    /// <summary>
    /// Generates a square mesh.
    /// </summary>
    internal class SquareGenerator : PolygonGenerator
    {
        #region Setup
        
        /// <summary>
        /// Creates a square generator.
        /// </summary>
        /// <param name="size">The size of the generated square (length of one side).</param>
        public SquareGenerator(float size) : base(size) { }
        
        #endregion

        #region Internal

        internal override Mesh Generate(bool calculateNormals)
        {
            var mesh = new Mesh
            {
                name = "Terraced Terrain Mesh"
            };
            
            var vertices = CreateVertices(Radius);
            mesh.SetVertices(vertices);
            var triangles = new[]
            {
                0, 1, 2, // First triangle
                0, 2, 3  // Second triangle
            };
            mesh.SetTriangles(triangles, 0, false, 0);
            if (calculateNormals)
                mesh.RecalculateNormals();
            
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