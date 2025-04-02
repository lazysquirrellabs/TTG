using System;
using LazySquirrelLabs.TerracedTerrainGenerator.Data;
using LazySquirrelLabs.TerracedTerrainGenerator.MeshFragmentation;
using LazySquirrelLabs.TerracedTerrainGenerator.Sculpting;
using LazySquirrelLabs.TerracedTerrainGenerator.Sculpting.Plane;
using LazySquirrelLabs.TerracedTerrainGenerator.ShapeGeneration.Plane.Polygons;
using LazySquirrelLabs.TerracedTerrainGenerator.TerraceGeneration;
using LazySquirrelLabs.TerracedTerrainGenerator.TerraceGeneration.Plane;
using Unity.Collections;

namespace LazySquirrelLabs.TerracedTerrainGenerator
{
	/// <summary>
	/// Top class responsible for the generation of planar terraced terrains.
	/// </summary>
	public class PlanarTerrainGenerator : TerrainGenerator
	{
		#region Setup

		/// <summary>
		/// <see cref="PlanarTerrainGenerator"/>'s constructor.
		/// </summary>
		/// <param name="sides">Number of sides of the terrain's basic shape. Value must be between 3 and 10. </param>
		/// <param name="radius">The terrain's radius. Value must be greater than zero.</param>
		/// <param name="maxHeight">The maximum height of the terrain, in units. In order words, distance
		/// between its lowest and highest point. Value must be greater than zero.</param>
		/// <param name="relativeTerraceHeights">Terrace heights, relative to the terrain's maximum height. Values
		/// must be in the [0, 1] range, in ascending order. Each terrace's final height will be calculated by
		/// multiplying the relative height by the terrain's height.</param>
		/// <param name="sculptSettings">The settings used during the sculpting phase.</param>
		/// <param name="depth">Depth to fragment the basic mesh. Value must be greater than zero.</param>
		/// <exception cref="ArgumentOutOfRangeException">Thrown if any of the arguments is out of range. Checks
		/// individual arguments for valid ranges.</exception>
		/// <exception cref="NotImplementedException">Thrown whenever the provided number of <paramref name="sides"/>
		/// is not supported (greater than 10).</exception>
		public PlanarTerrainGenerator(ushort sides, float radius, float maxHeight, float[] relativeTerraceHeights,
		                              SculptSettings sculptSettings, ushort depth)
			: base(0, maxHeight, relativeTerraceHeights)
		{
			if (depth == 0)
			{
				throw new ArgumentOutOfRangeException(nameof(depth), "Depth must be greater than zero.");
			}

			if (sides < 3)
			{
				throw new ArgumentOutOfRangeException(nameof(sides), "Sides must be greater than 2.");
			}

			if (radius <= 0)
			{
				throw new ArgumentOutOfRangeException(nameof(radius), "Radius must be positive.");
			}

			ShapeGenerator = sides switch
			{
				3     => new TriangleGenerator(radius),
				4     => new SquareGenerator(radius),
				<= 10 => new RegularPolygonGenerator(sides, radius),
				_     => throw new NotImplementedException($"Polygon with {sides} not implemented")
			};

			Fragmenter = new PlanarMeshFragmenter(depth);
			Sculptor = new PlaneSculptor(sculptSettings, maxHeight);
		}

		#endregion

		#region Protected

		private protected override Terracer GetTerracer(SimpleMeshData meshData, Allocator allocator)
		{
			return new PlaneTerracer(meshData, TerraceHeights, allocator);
		}

		#endregion
	}
}