using UnityEngine;

namespace LazySquirrelLabs.TerracedTerrainGenerator.Utils
{
	internal static class Vector3Extensions
	{
		#region Internal

		/// <summary>
		/// Rotates the given <paramref name="point"/> around the origin by a given angle.
		/// </summary>
		/// <param name="point">The point to be rotated.</param>
		/// <param name="degrees">The angle in degrees.</param>
		/// <returns>The rotated point.</returns>
		internal static Vector3 Rotate(this Vector3 point, float degrees)
		{
			var angle = degrees * Mathf.Deg2Rad;
			var cos = Mathf.Cos(angle);
			var sin = Mathf.Sin(angle);
			var newX = point.x * cos - point.z * sin;
			var newZ = point.x * sin + point.z * cos;
			return new Vector3(newX, point.y, newZ);
		}

		#endregion
	}
}