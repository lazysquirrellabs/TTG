using System;
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

		#region Protected

		protected override void InitializeOffsets()
		{
			for (var i = 0; i < Settings.Octaves; i++)
			{
				var xOffset = Random.Next(-10_000, 10_000);
				var yOffset = Random.Next(-10_000, 10_000);
				_offsets[i] = new Vector2(xOffset, yOffset);
			}
		}

		protected override Vector3 PlaceVertexAtHeight(Vector3 vertex, float height)
		{
			vertex.y = height;
			return vertex;
		}

		protected override float GetNoise(Vector3 vertex, float frequency, int index)
		{
			var offset = _offsets[index];
			var filterX = vertex.x * frequency + offset.x;
			var filterY = vertex.y * frequency + offset.y;
			return Mathf.PerlinNoise(filterX, filterY);
		}

		protected override Func<Vector3, Vector3> GetApplyFinalHeight(float dropFactor)
		{
			return ApplyHeight;
			
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