using SneakySquirrelLabs.TerracedTerrainGenerator.Data;
using UnityEngine;
using Random = System.Random;

namespace SneakySquirrelLabs.TerracedTerrainGenerator.Sculpting
{
    /// <summary>
    /// Sculpts a terrain mesh using a planar Perlin filter. The sculpting is applied on the Y axis, upwards.
    /// </summary>
    internal class PerlinSculptor
    {
        #region Fields

        /// <summary>
        /// Randomizer used to offset the noise application so it delivers different results.
        /// </summary>
        private readonly Random _random;
        /// <summary>
        /// The frequency of sculpture detail (how many elements in a given area).
        /// </summary>
        private readonly float _frequency;
        /// <summary>
        /// The curve used to change the height distribution.
        /// </summary>
        private readonly AnimationCurve _heightDistribution;
        /// <summary>
        /// The Y coordinate of the highest possible vertex after sculpting.
        /// </summary>
        private readonly float _maximumHeight;

        #endregion
        
        #region Setup

        /// <summary>
        /// Creates a <see cref="PerlinSculptor"/> with the given settings.
        /// </summary>
        /// <param name="sculptingSettings">The settings used for sculpting.</param>
        /// <param name="maximumHeight"> The Y coordinate of the highest possible vertex after sculpting.</param>
        internal PerlinSculptor(SculptingSettings sculptingSettings, float maximumHeight)
        {
            _random = new Random(sculptingSettings.Seed);
            _frequency = sculptingSettings.Frequency;
            _heightDistribution = sculptingSettings.HeightDistribution;
            _maximumHeight = maximumHeight;
        }

        #endregion
        
        #region Internal

        /// <summary>
        /// Sculpts the given terrain mesh.
        /// </summary>
        /// <param name="meshData">The mesh data to be sculpted.</param>
        internal void Sculpt(SimpleMeshData meshData)
        {
            // Randomization. Fetch random offsets to increment coordinates.
            var xOffset = _random.Next(-1_000, 1_000);
            var yOffset = _random.Next(-1_000, 1_000);
            // Actually apply the Perlin noise modifier.
            meshData.Map(SculptVertex);

            Vector3 SculptVertex(Vector3 vertex)
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
                    var clampedNoise = Mathf.Clamp(noise, 0, maximum);
                    // Step 2, apply the height curve (if it's not null) to the noise value
                    var modifier = heightDistribution?.Evaluate(clampedNoise) ?? 1;
                    // Step 3, apply the modifier to the maximum height
                    return maximum * modifier;
                }
            }
        }

        #endregion
    }
}