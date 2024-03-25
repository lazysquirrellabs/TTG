using LazySquirrelLabs.TerracedTerrainGenerator.Data;
using UnityEngine;

namespace LazySquirrelLabs.TerracedTerrainGenerator.Sculpting
{
	/// <summary>
	/// Sculpts a terrain mesh using a planar Perlin filter. The sculpting is applied on the Y axis, upwards.
	/// </summary>
	internal class PlaneSculptor : Sculptor
	{
		#region Fields

		/// <summary>
		/// The octave offsets.
		/// </summary>
		private readonly Vector2[] _offsets;

		#endregion

		#region Setup

		/// <summary>
		/// Creates a <see cref="PlaneSculptor"/> with the given settings.
		/// </summary>
		/// <param name="sculptSettings">The settings used for sculpting.</param>
		/// <param name="maximumHeight"> The Y coordinate of the highest possible vertex after sculpting.</param>
		internal PlaneSculptor(SculptSettings sculptSettings, float maximumHeight) : base(sculptSettings, maximumHeight)
		{
			_offsets = new Vector2[(int)sculptSettings.Octaves];
		}

		#endregion

		#region Internal

		internal override void Sculpt(SimpleMeshData meshData)
		{
			var offsets = _offsets;
			for (var i = 0; i < Settings.Octaves; i++)
			{
				var xOffset = Random.Next(-10_000, 10_000);
				var yOffset = Random.Next(-10_000, 10_000);
				offsets[i] = new Vector2(xOffset, yOffset);
			}

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
				var height = GetNoise(vertex.x, vertex.z, Settings, _offsets);
				if (height > highestPointRelative)
					highestPointRelative = height;
				vertex.y = height;
				return vertex;

				static float GetNoise(float x, float y, SculptSettings settings, Vector2[] offsets)
				{
					float relativeHeight = 0;
					var amplitude = 1f;
					var frequency = settings.BaseFrequency;
					var persistence = settings.Persistence;
					var lacunarity = settings.Lacunarity;
					for (var i = 0; i < settings.Octaves; i++)
					{
						var offset = offsets[i];
						var filterX = x * frequency + offset.x;
						var filterY = y * frequency + offset.y;
						var noise = Mathf.PerlinNoise(filterX, filterY);
						relativeHeight += amplitude * noise;

						amplitude *= persistence;
						frequency *= lacunarity;
					}

					return relativeHeight;
				}
			}

			Vector3 ApplyHeight(Vector3 vertex)
			{
				// Normalize the height data, so it's in the [0, 1] range.
				var relativeHeight = vertex.y * dropFactor;
				// Fetch the height distribution modifier
				var modifier = Settings.HeightDistribution.Evaluate(relativeHeight);
				vertex.y = MaximumHeight * relativeHeight * modifier;
				return vertex;
			}
		}

		#endregion

	}
}