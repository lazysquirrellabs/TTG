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
        private readonly float _maximumHeight;
        private readonly float _frequency;
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
        /// <param name="maximumHeight">The maximum height used for deformation.</param>
        /// <param name="frequency">The frequency of deformation (how many elements in a given area).</param>
        /// <param name="heightDistribution">The curve used to change the height distribution. If it's null, the
        /// distribution won't be affected, thus it will be linear.</param>
        internal PerlinDeformer(int seed, float maximumHeight, float frequency, AnimationCurve heightDistribution)
        {
            _random = new Random(seed);
            _maximumHeight = maximumHeight;
            _frequency = frequency;
            _heightDistribution = heightDistribution;
        }

        #endregion
        
        #region Internal

        /// <summary>
        /// Deforms the given terrain mesh.
        /// </summary>
        /// <param name="meshData">The mesh data to be deformed.</param>
        internal void Deform(SimpleMeshData meshData)
        {
            var xOffset = _random.Next(-1_000, 1_000);
            var yOffset = _random.Next(-1_000, 1_000);
            meshData.Map(DeformVertex);

            Vector3 DeformVertex(Vector3 vertex)
            {
                var filterX = (vertex.x + xOffset) * _frequency;
                var filterY = (vertex.z + yOffset) * _frequency;
                var height = GetHeight(filterX, filterY, _maximumHeight, _heightDistribution);
                vertex.y += height;
                return vertex;

                static float GetHeight(float x, float y, float maximum, AnimationCurve heightDistribution)
                {
                    var noise = Mathf.PerlinNoise(x, y);
                    var modifier = heightDistribution?.Evaluate(noise) ?? 1;
                    return maximum * modifier;
                }
            }
        }

        #endregion
    }
}