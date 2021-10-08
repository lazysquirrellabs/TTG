using UnityEngine;
using Random = System.Random;

namespace SneakySquirrelLabs.TerracedTerrainGenerator.Deformation
{
    /// <summary>
    /// Deforms a terrain mesh using a planar Perlin filter. The deformation is applied on the Y axis, upwards.
    /// </summary>
    internal class PerlinDeformer
    {
        #region Fields

        /// <summary>
        /// Randomizer used to offset the filter noise application so it delivers different results.
        /// </summary>
        private readonly Random _random;

        #endregion
        
        #region Setup

        /// <summary>
        /// Creates a <see cref="PerlinDeformer"/> with a random seed.
        /// </summary>
        internal PerlinDeformer()
        {
            _random = new Random();
        }

        /// <summary>
        /// Creates a <see cref="PerlinDeformer"/> with the given <paramref name="seed"/>. If you want deterministic
        /// output, use this function.
        /// </summary>
        /// <param name="seed">Seed used by the randomizer.</param>
        internal PerlinDeformer(int seed)
        {
            _random = new Random(seed);
        }

        #endregion
        
        #region Internal    

        /// <summary>
        /// Deforms the given terrain mesh.
        /// </summary>
        /// <param name="mesh">The terrain mesh to be deformed.</param>
        /// <param name="height">The maximum height used for deformation.</param>
        internal void Deform(Mesh mesh, float height)
        {
            var vertices = mesh.vertices;
            var xOffset = (float) _random.NextDouble();
            var yOffset = (float) _random.NextDouble();

            for (var i = 0; i < vertices.Length; i++)
            {
                var vertex = vertices[i];
                vertex.y += height * Mathf.PerlinNoise(vertex.x + xOffset, vertex.z + yOffset);
                vertices[i] = vertex;
            }
            mesh.SetVertices(vertices);
            mesh.RecalculateNormals();
        }

        #endregion
    }
}