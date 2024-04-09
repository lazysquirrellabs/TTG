using LazySquirrelLabs.TerracedTerrainGenerator.Sculpting;
using UnityEngine;

namespace LazySquirrelLabs.TerracedTerrainGenerator.Controllers
{
	public class SphereTerrainGeneratorController : TerrainGeneratorController
	{
		#region Serialzied fields

		[SerializeField] private float _minimumHeight = 20;

		#endregion
		
		#region Protected
		
		private protected override TerrainGenerator GetGenerator(float maximumHeight, float[] relativeHeights, 
			SculptSettings sculptSettings, ushort depth)
		{
			return new SphereTerrainGenerator(_minimumHeight, maximumHeight, relativeHeights, sculptSettings, depth);
		}
		
		#endregion
	}
}