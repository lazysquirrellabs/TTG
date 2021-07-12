using System;
using SneakySquirrelLabs.TerracedTerrainGenerator.Utils;
using UnityEngine;

namespace SneakySquirrelLabs.TerracedTerrainGenerator.PolygonGeneration
{
    internal class RegularPolygonGenerator : PolygonGenerator
    {
        #region Fields

        private readonly uint _sides;

        #endregion
        
        #region Setup
        
        public RegularPolygonGenerator(uint sides, float radius) : base(radius)
        {
            if (sides < 5)
                throw new ArgumentException("Regular polygons should have at least 5 sides");
            _sides = sides;
        }
        
        #endregion

        #region Internal

        internal override Mesh Generate()
        {
            var angleDelta = 360f / _sides;

            var mesh = new Mesh
            {
                name = "Terraced Terrain Mesh",
                vertices = CreateVertices(Radius, angleDelta, _sides),
                triangles =  GetTriangles(_sides),
                normals =  GetNormals(_sides)
            };
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

            static int[] GetTriangles(uint sides)
            {
                var triangles = new int[sides * 3];
                for (var i = 0; i < sides; i++)
                {
                    triangles[i * 3] = (int) sides;
                    triangles[i * 3 + 1] = i;
                    triangles[i * 3 + 2] = (i + 1) % (int) sides;
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