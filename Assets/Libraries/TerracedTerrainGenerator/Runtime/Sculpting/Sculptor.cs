using System;
using LazySquirrelLabs.TerracedTerrainGenerator.Data;
using UnityEngine;
using Random = System.Random;

namespace LazySquirrelLabs.TerracedTerrainGenerator.Sculpting
{
	/// <summary>
	/// Sculpts a terrain mesh using a Perlin filter.
	/// </summary>
	internal abstract class Sculptor 
	{
		#region Private

		private readonly NoiseFetcher _getNoise;

		#endregion
		
		#region Properties

		/// <summary>
		/// The height of the highest possible vertex after sculpting.
		/// </summary>
		protected float MaximumHeight { get; }

		/// <summary>
		/// The settings used for sculpting.
		/// </summary>
		protected SculptSettings Settings { get; }
		
		/// <summary>
		/// The random generator used to calculate noise offsets.
		/// </summary>
		protected Random Random { get; }

		#endregion

		#region Setup

		/// <summary>
		/// Creates a <see cref="Sculptor"/> with the given settings.
		/// </summary>
		/// <param name="sculptSettings">The settings used for sculpting.</param>
		/// <param name="maximumHeight"> The height of the highest possible vertex after sculpting.</param>
		protected Sculptor(SculptSettings sculptSettings, float maximumHeight)
		{
			Settings = sculptSettings;
			MaximumHeight = maximumHeight;
			Random = new Random(Settings.Seed);
		}

		#endregion
		
		#region Internal

		/// <summary>
		/// Sculpts the given terrain mesh.
		/// </summary>
		/// <param name="meshData">The mesh data to be sculpted.</param>
		internal void Sculpt(SimpleMeshData meshData)
		{
			InitializeOffsets();

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
			var applyFinalHeight = GetApplyFinalHeight(dropFactor);
			meshData.Map(applyFinalHeight);

			Vector3 CalculateVertexNoise(Vector3 vertex)
			{
				var height = GetTotalNoise(vertex, Settings);
				if (height > highestPointRelative)
					highestPointRelative = height;
				return PlaceVertexAtHeight(vertex, height);

				float GetTotalNoise(Vector3 v, SculptSettings settings)
				{
					float relativeHeight = 0;
					var amplitude = 1f;
					var frequency = settings.BaseFrequency;
					var persistence = settings.Persistence;
					var lacunarity = settings.Lacunarity;
					for (var i = 0; i < settings.Octaves; i++)
					{
						var noise = GetNoise(v, frequency, i);
						relativeHeight += amplitude * noise;
						amplitude *= persistence;
						frequency *= lacunarity;
					}

					return relativeHeight;
				}
			}
		}

		#endregion

		#region Protected

		protected abstract void InitializeOffsets();

		protected abstract Vector3 PlaceVertexAtHeight(Vector3 vertex, float height);
		
		protected abstract float GetNoise(Vector3 vertex, float frequency, int index);

		protected abstract Func<Vector3,Vector3> GetApplyFinalHeight(float dropFactor);

		#endregion
	}
}