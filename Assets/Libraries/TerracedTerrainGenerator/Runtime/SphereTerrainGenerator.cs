using System;
using LazySquirrelLabs.TerracedTerrainGenerator.Data;
using LazySquirrelLabs.TerracedTerrainGenerator.Sculpting;
using LazySquirrelLabs.TerracedTerrainGenerator.ShapeGeneration;
using LazySquirrelLabs.TerracedTerrainGenerator.TerraceGeneration;
using Unity.Collections;

namespace LazySquirrelLabs.TerracedTerrainGenerator
{
	internal class SphereTerrainGenerator : TerrainGenerator
	{
		#region Setup

		public SphereTerrainGenerator(float minimumHeight, float maximumHeight, float[] relativeTerraceHeights, 
			SculptSettings sculptSettings, ushort depth) : base(minimumHeight, maximumHeight, relativeTerraceHeights, depth)
		{
			if (minimumHeight <= 0)
				throw new ArgumentOutOfRangeException(nameof(maximumHeight), "Minimum height must be greater than zero.");
			
			if (minimumHeight > maximumHeight)
				throw new ArgumentOutOfRangeException(nameof(minimumHeight), "Minimum height must be less than maximum height");
            
			if (maximumHeight <= 0)
				throw new ArgumentOutOfRangeException(nameof(maximumHeight), "Maximum height must be greater than zero.");

			ShapeGenerator = new SphereGenerator(minimumHeight);
			Sculptor = new SphereSculptor(sculptSettings, minimumHeight, maximumHeight);
		}

		#endregion

		#region Protected

		private protected override Terracer GetTerracer(SimpleMeshData meshData, Allocator allocator)
		{
			return new SphereTerracer(meshData, TerraceHeights, allocator);
		}

		#endregion
	}
}