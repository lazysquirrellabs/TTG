using LazySquirrelLabs.TerracedTerrainGenerator.Data;
using Unity.Collections;
using UnityEngine;

namespace LazySquirrelLabs.TerracedTerrainGenerator.TerraceGeneration.Plane
{
	/// <summary>
	/// Modifies an existing plane terrain mesh by creating terraces on it. The strategy used is similar to the one
	/// described in https://icospheric.com/blog/2016/07/17/making-terraced-terrain/.
	/// Terraces are created based on a vertex's height (Y coordinate value).
	/// </summary>
	internal sealed class PlaneTerracer : Terracer
	{
		#region Setup

		/// <summary>
		/// <see cref="PlaneTerracer" />'s constructor.
		/// </summary>
		/// <param name="meshData">The terrain's original mesh data. It will be used to read data from and remains
		/// unmodified.</param>
		/// <param name="terraceHeights">The heights of all terraces (in units), in ascending order.</param>
		/// <param name="allocator">The allocation strategy used when creating vertex and index buffers.</param>
		public PlaneTerracer(SimpleMeshData meshData, float[] terraceHeights, Allocator allocator)
			: base(meshData, terraceHeights, allocator) { }

		#endregion

		#region Protected

		protected override float GetVertexHeight(Vector3 vertex)
		{
			// On a plane terrain, a vertex's height is its Y coordinate.
			return vertex.y;
		}

		protected override Vector3 SetVertexHeight(Vector3 vertex, float height)
		{
			// On a plane terrain, a vertex's height is its Y coordinate.
			vertex.y = height;
			return vertex;
		}

		#endregion
	}
}