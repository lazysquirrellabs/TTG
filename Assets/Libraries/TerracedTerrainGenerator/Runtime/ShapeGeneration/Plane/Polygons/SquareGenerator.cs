using LazySquirrelLabs.TerracedTerrainGenerator.Data;
using Unity.Collections;
using UnityEngine;

namespace LazySquirrelLabs.TerracedTerrainGenerator.ShapeGeneration.Plane.Polygons
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
        internal SquareGenerator(float size) : base(size) { }
        
        #endregion

        #region Internal

        internal override SimpleMeshData Generate(Allocator allocator)
        {
            var meshData = new SimpleMeshData(4, 6, allocator);
            using var vertices = CreateVertices(Radius, allocator);
            meshData.AddQuadrilateral(vertices[0], vertices[1], vertices[2], vertices[3]);
            
            return meshData;

            static NativeArray<Vector3> CreateVertices(float radius, Allocator allocator)
            {
                var vertices = new NativeArray<Vector3>(4, allocator);
                vertices[0] = new Vector3(-radius, 0f, radius);
                vertices[1] = new Vector3(radius,  0f, radius);
                vertices[2] = new Vector3(radius,  0f, -radius);
                vertices[3] = new Vector3(-radius, 0f, -radius);
                return vertices;
            }
        }

        #endregion
    }
}