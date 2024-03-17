using UnityEngine;

namespace LazySquirrelLabs.TerracedTerrainGenerator.SphereGeneration
{
	internal readonly struct Plane
	{
		#region Properties

		internal Vector3 TopLeft { get; }
		
		internal Vector3 BottomLeft { get; }
		
		internal Vector3 TopRight { get; }
		
		internal Vector3 BottomRight { get; }

		#endregion

		#region Setup

		// internal Plane(float width, float height)
		// {
		// }

		#endregion
	}
}