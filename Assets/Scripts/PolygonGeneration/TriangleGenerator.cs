using SneakySquirrelLabs.TerracedTerrainGenerator.Utils;
using UnityEngine;

namespace SneakySquirrelLabs.TerracedTerrainGenerator.PolygonGeneration
{
    /// <summary>
    /// Generates an equilateral triangle mesh.
    /// </summary>
    internal class TriangleGenerator : PolygonGenerator
    {
        #region Setup

        /// <summary>
        /// Creates a triangle generator
        /// </summary>
        /// <param name="size">The size of the generated triangle (the distance between the center and
        /// its vertices).</param>
        internal TriangleGenerator(float size) : base(size) { }

        #endregion

        #region Internal

        internal override MeshData Generate()
        {
            var meshData = new MeshData(3, 3);
            var vertices = CreateVertices(Radius);
            meshData.AddTriangle(vertices[0], vertices[1], vertices[2]);
            
            return meshData;

            static Vector3[] CreateVertices(float radius)
            {
                var vertices = new Vector3[3];
                vertices[0] = new Vector3(radius, 0f, 0f);
                vertices[1] = vertices[0].Rotate(-120f);
                vertices[2] = vertices[1].Rotate(-120f);
                return vertices;
            }
        }

        #endregion
    }
}