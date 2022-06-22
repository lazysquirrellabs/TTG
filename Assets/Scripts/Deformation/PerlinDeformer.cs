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
        /// <param name="frequency">The frequency of deformation (how many elements in a given area).</param>
        /// <param name="recalculateNormals">Whether the vertices' normals should be recalculated after
        /// deformation.</param>
        internal void Deform(Mesh mesh, float height, float frequency, bool recalculateNormals)
        {
            var vertices = mesh.vertices;
            var xOffset = _random.Next(-1_000, 1_000);
            var yOffset = _random.Next(-1_000, 1_000);

            for (var i = 0; i < vertices.Length; i++)
            {
                var vertex = vertices[i];
                var filterX = (vertex.x + xOffset) * frequency;
                var filterY = (vertex.z + yOffset) * frequency;
                vertex.y += height * Mathf.PerlinNoise(filterX, filterY);
                vertices[i] = vertex;
            }
            mesh.SetVertices(vertices);
            if (recalculateNormals)
                mesh.RecalculateNormals();
        }

        #endregion
    }
}