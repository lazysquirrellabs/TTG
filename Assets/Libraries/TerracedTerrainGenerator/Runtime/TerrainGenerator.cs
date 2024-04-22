using System;
using System.Threading;
using System.Threading.Tasks;
using LazySquirrelLabs.TerracedTerrainGenerator.Data;
using LazySquirrelLabs.TerracedTerrainGenerator.MeshFragmentation;
using LazySquirrelLabs.TerracedTerrainGenerator.Sculpting;
using LazySquirrelLabs.TerracedTerrainGenerator.ShapeGeneration;
using LazySquirrelLabs.TerracedTerrainGenerator.TerraceGeneration;
using Unity.Collections;
using UnityEngine;

namespace LazySquirrelLabs.TerracedTerrainGenerator
{
	/// <summary>
	/// Base class for all terrain generators.
	/// </summary>
	public abstract class TerrainGenerator
	{
		#region Fields

		/// <summary>
		/// Allocation strategy used whenever the synchronous terrain generation is performed.
		/// </summary>
		private const Allocator SyncAllocator = Allocator.TempJob;

		/// <summary>
		/// Allocation strategy used whenever the asynchronous terrain generation is performed.
		/// </summary>
		private const Allocator AsyncAllocator = Allocator.Persistent;

		/// <summary>
		/// The allocation strategy used by this generator.
		/// </summary>
		private readonly Allocator _allocator;

		/// <summary>
		/// The mesh fragmenter used to fragment a basic shape, creating a more detailed mesh.
		/// </summary>
		private readonly MeshFragmenter _fragmenter;

		#endregion

		#region Protected

		/// <summary>
		/// The polygon generator used to create the terrain's basic shape.
		/// </summary>
		private protected ShapeGenerator ShapeGenerator { private get; set; }

		/// <summary>
		/// The sculptor used to create hills/valleys on the mesh.
		/// </summary>
		private protected Sculptor Sculptor { private get; set; }

		/// <summary>
		/// The height of the terraces (in units), in ascending order.
		/// </summary>
		private protected float[] TerraceHeights { get; }

		#endregion

		#region Setup

		/// <summary>
		/// <see cref="PlaneTerrainGenerator"/>'s constructor.
		/// </summary>
		/// <param name="minHeight">The minimum height of the terrain, in units.</param>
		/// <param name="maxHeight">The maximum height of the terrain, in units.</param>
		/// <param name="relativeTerraceHeights">Terrace heights, relative to the terrain's maximum height. Values
		/// must be in the [0, 1] range, in ascending order. Each terrace's final height will be calculated by
		/// multiplying the relative height by the terrain's height.</param>
		/// <param name="depth">Depth to fragment the basic mesh. Value must be greater than zero.</param>
		/// <exception cref="ArgumentOutOfRangeException">Thrown if any of the arguments is out of range. Checks
		/// individual arguments for valid ranges.</exception>
		private protected TerrainGenerator(float minHeight, float maxHeight, float[] relativeTerraceHeights,
		                                   ushort depth)
		{
			if (maxHeight <= 0)
			{
				throw new ArgumentOutOfRangeException(nameof(maxHeight), "Height must be greater than zero.");
			}

			if (depth == 0)
			{
				throw new ArgumentOutOfRangeException(nameof(depth), "Depth must be greater than zero.");
			}

			if (relativeTerraceHeights.Length == 0)
			{
				throw new ArgumentOutOfRangeException(nameof(relativeTerraceHeights), "Relative heights is empty.");
			}

			// Check if relative terrace heights are valid.
			for (var i = 0; i < relativeTerraceHeights.Length; i++)
			{
				var relativeHeight = relativeTerraceHeights[i];

				if (relativeHeight is < 0 or > 1)
				{
					throw new ArgumentOutOfRangeException(nameof(relativeTerraceHeights),
					                                      "Relative heights must be greater than 0 and less than 1."
					);
				}

				if (i != 0 && relativeHeight <= relativeTerraceHeights[i - 1])
				{
					throw new ArgumentOutOfRangeException(nameof(relativeTerraceHeights),
					                                      "Relative heights must be in ascending order."
					);
				}
			}

			_fragmenter = new MeshFragmenter(depth);

			var heightDelta = maxHeight - minHeight;
			TerraceHeights = new float[relativeTerraceHeights.Length];

			for (var i = 0; i < TerraceHeights.Length; i++)
			{
				TerraceHeights[i] = minHeight + relativeTerraceHeights[i] * heightDelta;
			}
		}

		#endregion

		#region Public

		/// <summary>
		/// Generates the entire terraced terrain synchronously.
		/// </summary>
		/// <returns>The generated <see cref="Mesh"/>.</returns>
		public Mesh GenerateTerrain()
		{
			var meshData = GenerateTerrainData(SyncAllocator);
			using var terracer = GetTerracer(meshData, SyncAllocator);
			terracer.CreateTerraces();
			terracer.BakeMeshData(SyncAllocator);
			var mesh = terracer.CreateMesh();
			return mesh;
		}

		/// <summary>
		/// Generates the entire terraced terrain asynchronously.
		/// </summary>
		/// <param name="token">Token used for task cancellation.</param>
		/// <returns>A task that represents the generation process. Should be awaited to retrieve the generated
		/// <see cref="Mesh"/>.</returns>
		public async Task<Mesh> GenerateTerrainAsync(CancellationToken token)
		{
			// Run the mesh data generation (the heaviest part of the process) on the thread pool
			using var terracer = await Task.Run(GenerateTerracedTerrainData, token);
			var mesh = terracer.CreateMesh();
			return mesh;

			Terracer GenerateTerracedTerrainData()
			{
				var meshData = GenerateTerrainData(AsyncAllocator);
				var t = GetTerracer(meshData, AsyncAllocator);

				try
				{
					t.CreateTerraces();
					t.BakeMeshData(AsyncAllocator);
				}
				catch (Exception)
				{
					t.Dispose();
					throw;
				}

				return t;
			}
		}

		#endregion

		#region Protected

		/// <summary>
		/// Gets a <see cref="Terracer"/> instance.
		/// </summary>
		/// <param name="meshData">The mesh data used to build the <see cref="Terracer"/>.</param>
		/// <param name="allocator">The allocator used by the <see cref="Terracer"/>.</param>
		/// <returns>A new instance of <see cref="Terracer"/>.</returns>
		private protected abstract Terracer GetTerracer(SimpleMeshData meshData, Allocator allocator);

		#endregion

		#region Private

		private SimpleMeshData GenerateTerrainData(Allocator allocator)
		{
			var meshData = ShapeGenerator.Generate(allocator);
			var fragmentedMeshData = _fragmenter.Fragment(meshData, allocator);
			meshData.Dispose();
			Sculptor.Sculpt(fragmentedMeshData);
			return fragmentedMeshData;
		}

		#endregion
	}
}