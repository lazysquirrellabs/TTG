using System;
using SneakySquirrelLabs.TerracedTerrainGenerator.Utils;
using UnityEngine;

namespace SneakySquirrelLabs.TerracedTerrainGenerator.PolygonGeneration
{
    /// <summary>
    /// Generates regular polygons with 5 or more sides.
    /// </summary>
    internal class RegularPolygonGenerator : PolygonGenerator
    {
        #region Fields

        /// <summary>
        /// How many sides the polygon has.
        /// </summary>
        private readonly ushort _sides;

        #endregion
        
        #region Setup
        
        /// <summary>
        /// Creates a regular polygon generator.
        /// </summary>
        /// <param name="sides">How many sides the generated polygon should have.</param>
        /// <param name="radius">The radius of the generated polygon (distance between the center and its
        /// vertices).</param>
        /// <exception cref="ArgumentException">Thrown if the number of sides is less than 5. For squares and triangles,
        /// check <see cref="SquareGenerator"/> and <see cref="TriangleGenerator"/>.</exception>
        public RegularPolygonGenerator(ushort sides, float radius) : base(radius)
        {
            if (sides < 5)
                throw new ArgumentException("Regular polygons should have at least 5 sides");
            _sides = sides;
        }
        
        #endregion

        #region Internal
        
        internal override Mesh Generate(bool calculateNormals)
        {
            var angleDelta = 360f / _sides;

            var mesh = new Mesh
            {
                name = "Terraced Terrain Mesh",
                vertices = CreateVertices(Radius, angleDelta, _sides),
                triangles =  GetTriangles(_sides),
                normals =  GetNormals(_sides)
            };

            var vertices = CreateVertices(Radius, angleDelta, _sides);
            mesh.SetVertices(vertices);
            var triangles = GetTriangles(_sides);
            mesh.SetTriangles(triangles, 0, false, 0);
            if (calculateNormals)
                mesh.RecalculateNormals();
            
            return mesh;

            static Vector3[] CreateVertices(float radius, float delta, uint sides)
            {
                var vertices = new Vector3[sides + 1];
                // Place the first vertex Radius units away
                vertices[0] = new Vector3(radius, 0f, 0f);
                
                // Place other outer vertices
                for (var i = 1; i < sides; i++)
                    vertices[i] = vertices[i - 1].Rotate(-delta);
                
                // Place the center vertex
                vertices[sides] = Vector3.zero;

                return vertices;
            }

            static int[] GetTriangles(ushort sides)
            {
                var triangles = new int[sides * 3];
                for (var i = 0; i < sides; i++)
                {
                    triangles[i * 3] = sides;
                    triangles[i * 3 + 1] = i;
                    triangles[i * 3 + 2] = (i + 1) % sides;
                }
                
                return triangles;
            }
            
            static Vector3[] GetNormals(uint sides)
            {
                var vertexCount = sides + 1;
                var normals = new Vector3[vertexCount];
                for (var i = 0; i < vertexCount; i++) 
                    normals[i] = Vector3.up;
                return normals;
            }
        }

        #endregion
    }
}