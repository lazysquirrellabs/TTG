using SneakySquirrelLabs.TerracedTerrainGenerator.Data;
using UnityEngine;
using Random = System.Random;

namespace SneakySquirrelLabs.TerracedTerrainGenerator.Sculpting
{
    /// <summary>
    /// Sculpts a terrain mesh using a planar Perlin filter. The sculpting is applied on the Y axis, upwards.
    /// </summary>
    internal class Sculptor
    {
        #region Fields

        /// <summary>
        /// The Y coordinate of the highest possible vertex after sculpting.
        /// </summary>
        private readonly float _maximumHeight;
        /// <summary>
        /// The settings used for sculpting.
        /// </summary>
        private readonly SculptSettings _settings;

        #endregion
        
        #region Setup

        /// <summary>
        /// Creates a <see cref="Sculptor"/> with the given settings.
        /// </summary>
        /// <param name="sculptSettings">The settings used for sculpting.</param>
        /// <param name="maximumHeight"> The Y coordinate of the highest possible vertex after sculpting.</param>
        internal Sculptor(SculptSettings sculptSettings, float maximumHeight)
        {
	        _settings = sculptSettings;
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
            var random = new Random(_settings.Seed);
            var xOffset = random.Next(-1_000, 1_000);
            var yOffset = random.Next(-1_000, 1_000);
            // Actually apply the Perlin noise modifier.
            var highestPoint = float.MinValue;
            meshData.Map(SculptVertex);
            // At this point the vertices have been sculpted, but the Perlin distribution rarely places vertices
            // high up, which is undesired. Instead, we'd like to always have some points placed on the highest terrace.
            // To enforce that, "lift" all vertices so at least one of them matches the maximum height. The strength of
            // the expansion depends on the the factor between the maximum height and the highest vertex.
            var liftFactor = _maximumHeight / highestPoint;
            meshData.Map(Lift);

            Vector3 SculptVertex(Vector3 vertex)
            {
                // Get the X and Y coordinates to be fed into the filter
                var frequency = _settings.BaseFrequency;
                var filterX = (vertex.x + xOffset) * frequency;
                var filterY = (vertex.z + yOffset) * frequency;
                var height = GetHeight(filterX, filterY, _maximumHeight, _settings.HeightDistribution);
                if (height > highestPoint)
	                highestPoint = height;
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
            
            Vector3 Lift(Vector3 vertex)
            {
	            vertex.y *= liftFactor;
	            return vertex;
            }
        }

        #endregion
    }
}