using LazySquirrelLabs.TerracedTerrainGenerator.Data;
using Unity.Collections;
using UnityEngine;

namespace LazySquirrelLabs.TerracedTerrainGenerator.TerraceGeneration.Plane
{
	internal class PlaneTerracer : Terracer
	{
		#region Setup
		
		public PlaneTerracer(SimpleMeshData meshData, float[] terraceHeights, Allocator allocator) 
			: base(meshData, terraceHeights, allocator)
		{
		}
		
		#endregion

		#region Protected
		
		protected override float GetVertexHeight(Vector3 vertex)
		{
			return vertex.y;
		}

		protected override Vector3 SetVertexHeight(Vector3 vertex, float height)
		{
			vertex.y = height;
			return vertex;
		}
		
		#endregion		
	}
}