using System;
using LazySquirrelLabs.TerracedTerrainGenerator.Data;
using LazySquirrelLabs.TerracedTerrainGenerator.Utils;
using Unity.Collections;
using UnityEngine;

namespace LazySquirrelLabs.TerracedTerrainGenerator.ShapeGeneration
{
    /// <summary>
    /// Generates a regular polygon mesh with 5 or more sides.
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
        internal RegularPolygonGenerator(ushort sides, float radius) : base(radius)
        {
            if (sides < 5)
                throw new ArgumentException("Regular polygons should have at least 5 sides");
            _sides = sides;
        }
        
        #endregion

        #region Internal
        
        internal override SimpleMeshData Generate(Allocator allocator)
        {
            var angleDelta = 360f / _sides;

            var vertexCount = _sides + 1;
            var indicesCount = _sides * 3;
            var meshData = new SimpleMeshData(vertexCount, indicesCount, allocator);

            using var vertices = CreateEdges(Radius, angleDelta, _sides, allocator);
            var center = Vector3.zero;

            // Add all triangles, except the "knot" one
            for (var i = 0; i < _sides - 1; i++)
            {
                var p1 = vertices[i];
                var p2 = vertices[i + 1];
                meshData.AddTriangle(p1, p2, center);
            }
            
            // Add the "knot" triangle
            meshData.AddTriangle(vertices[_sides-1], vertices[0], center);
            
            return meshData;

            static NativeArray<Vector3> CreateEdges(float radius, float delta, uint sides, Allocator allocator)
            {
                var vertices = new NativeArray<Vector3>((int) sides, allocator);
                // Place the first vertex Radius units away
                vertices[0] = new Vector3(radius, 0f, 0f);
                
                // Place other outer vertices
                for (var i = 1; i < sides; i++)
                    vertices[i] = vertices[i - 1].Rotate(-delta);

                return vertices;
            }
        }

        #endregion
    }
}