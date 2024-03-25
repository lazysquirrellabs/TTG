using LazySquirrelLabs.TerracedTerrainGenerator.Data;
using UnityEngine;

namespace LazySquirrelLabs.TerracedTerrainGenerator.Sculpting
{
	/// <summary>
	/// Sculpts a terrain mesh using a 3D Perlin filter. The sculpting is applied on each vertex's direction, outwards.
	/// </summary>
	internal class SphereSculptor : Sculptor
	{
		#region Fields

		/// <summary>
		/// The height of the lowest possible vertex after sculpting.
		/// </summary>
		private readonly float _minimumHeight;
		
		/// <summary>
		/// The difference between the height of the highest and lowest vertex after sculpting.
		/// </summary>
		private readonly float _heightDelta;
		
		/// <summary>
		/// The octave offsets.
		/// </summary>
		private readonly Vector3[] _offsets;

		#endregion
		
		#region Setup

		/// <summary>
		/// Creates a <see cref="SphereSculptor"/> with the given settings.
		/// </summary>
		/// <param name="sculptSettings">The settings used for sculpting.</param>
		/// <param name="minimumHeight">The height (magnitude) of the lowest possible vertex after sculpting.</param>
		/// <param name="maximumHeight">The height (magnitude) of the highest possible vertex after sculpting.</param>
		internal SphereSculptor(SculptSettings sculptSettings, float minimumHeight, float maximumHeight) 
			: base(sculptSettings, maximumHeight)
		{
			_minimumHeight = minimumHeight;
			_heightDelta = MaximumHeight - _minimumHeight;
			_offsets = new Vector3[(int)sculptSettings.Octaves];
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
				var zOffset = Random.Next(-10_000, 10_000);
				offsets[i] = new Vector3(xOffset, yOffset, zOffset);
			}
			
			var highestPointRelative = float.MinValue;
			// Actually apply the Perlin noise modifier.
			meshData.Map(CalculateVertexNoise);
			// At this point, the vertices' magnitude store noise data, which will be used to calculate the final
			// heights. But first, we need to normalize the noise data, bringing all values to the [0,1] range. To do
			// so, we find by how much we need to multiply the largest magnitude value among the vertices in order to
			// bring it down to 1. This value will be used on the next step to normalize all vertices.
			var dropFactor = 1f / highestPointRelative;
			// Now that we've got noise data and a way to normalize it, we can actually apply the final height modifier,
			// bringing all vertices' magnitude to the [minimumHeight, maximumHeight] range.
			meshData.Map(ApplyHeight);
			
			Vector3 CalculateVertexNoise(Vector3 vertex)
			{
				var height = GetNoise(vertex, Settings, _offsets);
				if (height > highestPointRelative)
					highestPointRelative = height;
				vertex = vertex.normalized * height;
				return vertex;

				static float GetNoise(Vector3 vertex, SculptSettings settings, Vector3[] offsets)
				{
					float relativeHeight = 0;
					var amplitude = 1f;
					var frequency = settings.BaseFrequency;
					var persistence = settings.Persistence;
					var lacunarity = settings.Lacunarity;
					for (var i = 0; i < settings.Octaves; i++)
					{
						var offset = offsets[i];
						var filterX = vertex.x * frequency + offset.x;
						var filterY = vertex.y * frequency + offset.y;
						var filterZ = vertex.z * frequency + offset.z;
						var noise = PerlinNoise3D(filterX, filterY, filterZ);
						relativeHeight += amplitude * noise;

						amplitude *= persistence;
						frequency *= lacunarity;
					}

					return relativeHeight;
					
					static float PerlinNoise3D(float x, float y, float z)
					{
						var xy = Mathf.PerlinNoise(x, y);
						var xz = Mathf.PerlinNoise(x, z);
						var yz = Mathf.PerlinNoise(y, z);
			
						var yx = Mathf.PerlinNoise(y, x);
						var zx = Mathf.PerlinNoise(z, x);
						var zy = Mathf.PerlinNoise(z, y);

						var xyz = xy + xz + yz + yx + zx + zy;
						return xyz / 6f;
					}
				}
			}
			
			Vector3 ApplyHeight(Vector3 vertex)
			{
				// Normalize the height data, so it's in the [0, 1] range.
				var relativeHeight = vertex.magnitude * dropFactor;
				// Fetch the height distribution modifier
				var modifier = Settings.HeightDistribution.Evaluate(relativeHeight);
				// Find the final height so it's in [minimumHeight, maximumHeight]. 
				var height = _minimumHeight + _heightDelta * relativeHeight * modifier;
				return vertex.normalized * height;
			}
		}

		#endregion
	}
}