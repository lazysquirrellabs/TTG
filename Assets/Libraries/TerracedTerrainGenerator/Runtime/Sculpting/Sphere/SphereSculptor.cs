using System;
using UnityEngine;

namespace LazySquirrelLabs.TerracedTerrainGenerator.Sculpting.Sphere
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

		#region Protected

		protected override void InitializeOffsets()
		{
			for (var i = 0; i < Settings.Octaves; i++)
			{
				var xOffset = Random.Next(-10_000, 10_000);
				var yOffset = Random.Next(-10_000, 10_000);
				var zOffset = Random.Next(-10_000, 10_000);
				_offsets[i] = new Vector3(xOffset, yOffset, zOffset);
			}
		}

		protected override Vector3 PlaceVertexAtHeight(Vector3 vertex, float height)
		{
			return vertex.normalized * height;
		}

		protected override Func<Vector3,Vector3> GetApplyFinalHeight(float dropFactor)
		{
			return ApplyFinalHeight;

			Vector3 ApplyFinalHeight(Vector3 vertex)
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

		protected override float GetNoise(Vector3 vertex, float frequency, int octaveIndex)
		{
			var offset = _offsets[octaveIndex];
			var filterX = vertex.x * frequency + offset.x;
			var filterY = vertex.y * frequency + offset.y;
			var filterZ = vertex.z * frequency + offset.z;
			return PerlinNoise3D(filterX, filterY, filterZ);
			
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

		#endregion
	}
}