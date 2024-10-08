using System;
using LazySquirrelLabs.TerracedTerrainGenerator.Data;
using LazySquirrelLabs.TerracedTerrainGenerator.MeshFragmentation;
using LazySquirrelLabs.TerracedTerrainGenerator.Sculpting;
using LazySquirrelLabs.TerracedTerrainGenerator.Sculpting.Sphere;
using LazySquirrelLabs.TerracedTerrainGenerator.ShapeGeneration.Sphere;
using LazySquirrelLabs.TerracedTerrainGenerator.TerraceGeneration;
using LazySquirrelLabs.TerracedTerrainGenerator.TerraceGeneration.Sphere;
using Unity.Collections;

namespace LazySquirrelLabs.TerracedTerrainGenerator
{
	/// <summary>
	/// Top class responsible for the generation of spherical terraced terrains.
	/// </summary>
	public class SphericalTerrainGenerator : TerrainGenerator
	{
		#region Setup

		/// <summary>
		/// <see cref="SphericalTerrainGenerator"/>'s constructor.
		/// </summary>
		/// <param name="minHeight">The minimum height of the terrain, in units. In order words, the smallest
		/// distance between a mesh point and the sphere's center. </param>
		/// <param name="maxHeight">The maximum height of the terrain, in units. In order words, the largest
		/// distance between a mesh point and the sphere's center. </param>
		/// <param name="relativeTerraceHeights">Terrace heights, relative to the terrain's maximum height. Values
		/// must be in the [0, 1] range, in ascending order. Each terrace's final height will be calculated by
		/// multiplying the relative height by the delta height (<paramref name="maxHeight"/> -
		/// <paramref name="minHeight"/>) and adding it to <paramref name="minHeight"/>.</param>
		/// <param name="sculptSettings">The settings used during the sculpting phase.</param>
		/// <param name="depth">Depth to fragment the basic mesh. Value must be greater than zero.</param>
		/// <exception cref="ArgumentOutOfRangeException">Thrown if any of the arguments is out of range. Checks
		/// individual arguments for valid ranges.</exception>
		public SphericalTerrainGenerator(float minHeight, float maxHeight, float[] relativeTerraceHeights,
		                                 SculptSettings sculptSettings, ushort depth)
			: base(minHeight, maxHeight, relativeTerraceHeights)
		{
			if (depth == 0)
			{
				throw new ArgumentOutOfRangeException(nameof(depth), "Depth must be greater than zero.");
			}

			if (minHeight <= 0)
			{
				throw new ArgumentOutOfRangeException(nameof(maxHeight), "Minimum height must be greater than zero.");
			}

			if (minHeight > maxHeight)
			{
				throw new ArgumentOutOfRangeException(nameof(minHeight),
				                                      "Minimum height must be less than maximum height");
			}

			ShapeGenerator = new SphereGenerator(minHeight);
			Fragmenter = new SphericalMeshFragmenter(depth);
			Sculptor = new SphereSculptor(sculptSettings, minHeight, maxHeight);
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