using LazySquirrelLabs.TerracedTerrainGenerator.Data;
using Unity.Collections;
using UnityEngine;

namespace LazySquirrelLabs.TerracedTerrainGenerator.TerraceGeneration
{
	internal class SphereTerracer : Terracer
	{
		#region Setup
		
		public SphereTerracer(SimpleMeshData meshData, float[] terraceHeights, Allocator allocator) 
			: base(meshData, terraceHeights, allocator)
		{
		}
		
		#endregion

		#region Protected
		
		protected override float GetVertexHeight(Vector3 vertex)
		{
			return vertex.magnitude;
		}

		protected override Vector3 SetVertexHeight(Vector3 vertex, float height)
		{
			return vertex.normalized * height;
		}
		
		#endregion	
	}
}