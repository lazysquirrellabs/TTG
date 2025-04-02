using UnityEngine;

namespace LazySquirrelLabs.TerracedTerrainGenerator.MeshFragmentation
{
	/// <summary>
	/// Fragments a spherical mesh into sub-triangles given an arbitrary depth. The original mesh is modified.
	/// </summary>
	internal class SphericalMeshFragmenter : MeshFragmenter
	{
		#region Setup

		/// <summary>
		/// <see cref="SphericalMeshFragmenter"/>'s constructor. To actually fragment a mesh, call
		/// <see cref="MeshFragmenter.Fragment"/>.
		/// </summary>
		/// <param name="depth">The depth (how many consecutive times) the mesh will be fragmented.</param>
		internal SphericalMeshFragmenter(ushort depth) : base(depth) { }

		#endregion

		#region Protected

		/// <summary>
		/// Finds the middle point between the given directions.
		/// </summary>
		/// <param name="v1">The first direction to find the middle point for.</param>
		/// <param name="v2">The second direction to find the middle point for.</param>
		/// <returns>The middle point between <paramref name="v1"/> and <paramref name="v2"/>.</returns>
		protected override Vector3 FindMiddlePoint(Vector3 v1, Vector3 v2)
		{
			return Vector3.Slerp(v1, v2, 0.5f);
		}

		#endregion
	}
}