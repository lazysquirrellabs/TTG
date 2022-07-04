using SneakySquirrelLabs.TerracedTerrainGenerator.Data;
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
        /// <summary>
        /// The mesh data to be fragmented.
        /// </summary>
        private readonly SimpleMeshData _meshData;
        /// <summary>
        /// The curve used to change the height distribution.
        /// </summary>
        private readonly AnimationCurve _heightDistribution;

        #endregion
        
        #region Setup

        /// <summary>
        /// Creates a <see cref="PerlinDeformer"/> with the given <paramref name="seed"/>. If you want deterministic
        /// output, use this function.
        /// </summary>
        /// <param name="seed">Seed used by the randomizer.</param>
        /// <param name="meshData">The mesh data to be deformed.</param>
        /// <param name="heightDistribution">The curve used to change the height distribution. If it's null,
        /// the distribution won't be affected, thus it will be linear.</param>
        internal PerlinDeformer(int seed, SimpleMeshData meshData, AnimationCurve heightDistribution)
        {
            _random = new Random(seed);
            _meshData = meshData;
            _heightDistribution = heightDistribution;
        }

        #endregion
        
        #region Internal

        /// <summary>
        /// Deforms the given terrain mesh.
        /// </summary>
        /// <param name="maximumHeight">The maximum height used for deformation.</param>
        /// <param name="frequency">The frequency of deformation (how many elements in a given area).</param>
        internal void Deform(float maximumHeight, float frequency)
        {
            var xOffset = _random.Next(-1_000, 1_000);
            var yOffset = _random.Next(-1_000, 1_000);
            _meshData.Map(DeformVertex);

            Vector3 DeformVertex(Vector3 vertex)
            {
                var filterX = (vertex.x + xOffset) * frequency;
                var filterY = (vertex.z + yOffset) * frequency;
                var height = GetHeight(filterX, filterY, maximumHeight, _heightDistribution);
                vertex.y += height;
                return vertex;

                static float GetHeight(float x, float y, float maximum, AnimationCurve heightDistribution)
                {
                    var noise = Mathf.PerlinNoise(x, y);
                    var modifier = heightDistribution == null ? 1 : heightDistribution.Evaluate(noise);
                    return maximum * modifier;
                }
            }
        }

        #endregion
    }
}