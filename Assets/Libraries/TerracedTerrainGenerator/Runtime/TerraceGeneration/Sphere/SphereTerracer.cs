using LazySquirrelLabs.TerracedTerrainGenerator.Data;
using Unity.Collections;
using UnityEngine;

namespace LazySquirrelLabs.TerracedTerrainGenerator.TerraceGeneration.Sphere
{
	/// <summary>
	/// Modifies an existing spherical terrain mesh by creating terraces on it. The strategy used is similar to the one
	/// described in https://icospheric.com/blog/2016/07/17/making-terraced-terrain/.
	/// Terraces are created based on a vertex's height (distance from sphere's center).
	/// </summary>
	internal sealed class SphereTerracer : Terracer
	{
		#region Setup
		
		/// <summary>
		/// <see cref="SphereTerracer"/>'s constructor.
		/// </summary>
		/// <param name="meshData">The terrain's original mesh data. It will be used to read data from and remains
		/// unmodified.</param>
		/// <param name="terraceHeights">The heights of all terraces (in units), in ascending order.</param>
		/// <param name="allocator">The allocation strategy used when creating vertex and index buffers.</param>
		public SphereTerracer(SimpleMeshData meshData, float[] terraceHeights, Allocator allocator) 
			: base(meshData, terraceHeights, allocator)
		{
		}
		
		#endregion

		#region Protected
		
		protected override float GetVertexHeight(Vector3 vertex)
		{
			// On a spherical terrain, a vertex's height is how far that vertex is from the sphere center, which is 
			// exactly what its magnitude represents.
			return vertex.magnitude;
		}

		protected override Vector3 SetVertexHeight(Vector3 vertex, float height)
		{
			// On a spherical terrain, a vertex's height is how far that vertex is from the sphere center. To set that,
			// we find its direction with 1 unit of magnitude (normalized delivers that) and multiply it by the height.
			return vertex.normalized * height;
		}
		
		#endregion	
	}
}