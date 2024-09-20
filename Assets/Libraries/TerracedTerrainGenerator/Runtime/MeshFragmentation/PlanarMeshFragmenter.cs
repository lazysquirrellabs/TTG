using UnityEngine;

namespace LazySquirrelLabs.TerracedTerrainGenerator.MeshFragmentation
{
	/// <summary>
	/// Fragments a planar mesh into sub-triangles given an arbitrary depth. The original mesh is modified.
	/// </summary>
	internal class PlanarMeshFragmenter : MeshFragmenter
	{
		#region Setup

		/// <summary>
		/// <see cref="PlanarMeshFragmenter"/>'s constructor. To actually fragment a mesh, call
		/// <see cref="MeshFragmenter.Fragment"/>.
		/// </summary>
		/// <param name="depth">The depth (how many consecutive times) the mesh will be fragmented.</param>
		internal PlanarMeshFragmenter(ushort depth) : base(depth) { }

		#endregion

		#region Internal

		/// <summary>
		/// Finds the middle point between the given positions.
		/// </summary>
		/// <param name="v1">The first position to find the middle point for.</param>
		/// <param name="v2">The second position to find the middle point for.</param>
		/// <returns>The middle point between <paramref name="v1"/> and <paramref name="v2"/>.</returns>
		protected override Vector3 FindMiddlePoint(Vector3 v1, Vector3 v2)
		{
			return (v1 + v2) / 2;
		}

		#endregion
	}
}