using LazySquirrelLabs.TerracedTerrainGenerator.Data;
using LazySquirrelLabs.TerracedTerrainGenerator.Utils;
using Unity.Collections;
using UnityEngine;

namespace LazySquirrelLabs.TerracedTerrainGenerator.PolygonGeneration
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

        internal override SimpleMeshData Generate(Allocator allocator)
        {
            var meshData = new SimpleMeshData(3, 3, allocator);
            using var vertices = CreateVertices(Radius, allocator);
            meshData.AddTriangle(vertices[0], vertices[1], vertices[2]);
            
            return meshData;

            static NativeArray<Vector3> CreateVertices(float radius, Allocator allocator)
            {
                var vertices = new NativeArray<Vector3>(3, allocator);
                vertices[0] = new Vector3(radius, 0f, 0f);
                vertices[1] = vertices[0].Rotate(-120f);
                vertices[2] = vertices[1].Rotate(-120f);
                return vertices;
            }
        }

        #endregion
    }
}