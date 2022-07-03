using SneakySquirrelLabs.TerracedTerrainGenerator.Data;
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

        internal override SimpleMeshData Generate()
        {
            var meshData = new SimpleMeshData(4, 6);
            var vertices = CreateVertices(Radius);
            meshData.AddQuadrilateral(vertices[0], vertices[1], vertices[2], vertices[3]);
            
            return meshData;

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