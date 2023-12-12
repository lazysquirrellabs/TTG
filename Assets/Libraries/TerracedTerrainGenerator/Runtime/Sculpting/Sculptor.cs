using System.Collections.Generic;
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
			// Fetch random offsets to increment coordinates for each octave.§§§§§§§§§§§§§§§§§
			var offsets = GenerateOffsets(_settings.Seed, _settings.Octaves);
			var highestPointRelative = float.MinValue;
			// Actually apply the Perlin noise modifier.
			meshData.Map(CalculateVertexNoise);
			// At this point, the vertices' Y coordinate store noise data, which will be used to calculate the final 
			// heights. But first, we need to normalize the noise data, bringing all values to the [0,1] range. To do
			// so, we find by how much we need to multiply the largest noise value among the vertices in order to bring
			// it down to 1. This value will be used on the next step to normalize all vertices.
			var dropFactor = 1f / highestPointRelative;
			// Now that we've got noise data and a way to normalize it, we can actually apply the final height modifier,
			// bringing all vertices' Y coordinate to the [0, maximumHeight] range.
			meshData.Map(ApplyHeight);

			Vector3 CalculateVertexNoise(Vector3 vertex)
			{
				var height = GetNoise(vertex.x, vertex.z, _settings, offsets);
				if (height > highestPointRelative)
					highestPointRelative = height;
				vertex.y = height;
				return vertex;

				static float GetNoise(float x, float y, SculptSettings settings, IReadOnlyList<Vector2> offsets)
				{
					var amplitude = 1f;
					var frequency = settings.BaseFrequency;
					float relativeHeight = 0;
					for (var i = 0; i < settings.Octaves; i++)
					{
						var offset = offsets[i];
						var filterX = x * frequency + offset.x;
						var filterY = y * frequency + offset.y;
						var noise = Mathf.PerlinNoise(filterX, filterY);
						relativeHeight += amplitude * noise;
						amplitude *= settings.Persistence;
						frequency *= settings.Lacunarity;
					}

					return relativeHeight;
				}
			}

			Vector3 ApplyHeight(Vector3 vertex)
			{
				// Normalize the height data, so it's in the [0, 1] range.
				var relativeHeight = vertex.y * dropFactor;
				// Fetch the height distribution modifier
				var modifier = _settings.HeightDistribution.Evaluate(relativeHeight);
				vertex.y = _maximumHeight * relativeHeight * modifier;
				return vertex;
			}
			
			static Vector2[] GenerateOffsets(int seed, uint count)
			{
				var offsets = new Vector2[count];
				var random = new Random(seed);
				for (var i = 0; i < count; i++)
				{
					var xOffset = random.Next(-10_000, 10_000);
					var yOffset = random.Next(-10_000, 10_000);
					offsets[i] = new Vector2(xOffset, yOffset);
				}

				return offsets;
			}
		}

		#endregion
	}
}