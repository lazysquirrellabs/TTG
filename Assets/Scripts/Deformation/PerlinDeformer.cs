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
        /// Randomizer used to offset the noise application so it delivers different results.
        /// </summary>
        private readonly Random _random;
        /// <summary>
        /// The Y coordinate of the highest possible vertex after deformation.
        /// </summary>
        private readonly float _maximumHeight;
        /// <summary>
        /// The frequency of deformation (how many elements in a given area).
        /// </summary>
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
        /// <param name="maximumHeight">The Y coordinate of the highest possible vertex after deformation.</param>
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
            // Randomization. Fetch random offsets to increment coordinates.
            var xOffset = _random.Next(-1_000, 1_000);
            var yOffset = _random.Next(-1_000, 1_000);
            // Actually apply the Perlin noise modifier.
            meshData.Map(DeformVertex);

            Vector3 DeformVertex(Vector3 vertex)
            {
                // Get the X and Y coordinates to be fed into the filter
                var filterX = (vertex.x + xOffset) * _frequency;
                var filterY = (vertex.z + yOffset) * _frequency;
                var height = GetHeight(filterX, filterY, _maximumHeight, _heightDistribution);
                vertex.y += height;
                return vertex;

                static float GetHeight(float x, float y, float maximum, AnimationCurve heightDistribution)
                {
                    // Step 1, fetch the noise value at the given point
                    var noise = Mathf.PerlinNoise(x, y);
                    // Step 2, apply the height deformation curve (if it's not null) to the noise value
                    var modifier = heightDistribution?.Evaluate(noise) ?? 1;
                    // Step 3, apply the modifier to the maximum height
                    return maximum * modifier;
                }
            }
        }

        #endregion
    }
}