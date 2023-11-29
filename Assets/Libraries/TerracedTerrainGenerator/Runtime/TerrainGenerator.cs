using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SneakySquirrelLabs.TerracedTerrainGenerator.Data;
using SneakySquirrelLabs.TerracedTerrainGenerator.MeshFragmentation;
using SneakySquirrelLabs.TerracedTerrainGenerator.PolygonGeneration;
using SneakySquirrelLabs.TerracedTerrainGenerator.Sculpting;
using SneakySquirrelLabs.TerracedTerrainGenerator.TerraceGeneration;
using Unity.Collections;
using UnityEngine;

namespace SneakySquirrelLabs.TerracedTerrainGenerator
{
    /// <summary>
    /// Top-most entity responsible for the terraced terrain generation.
    /// </summary>
    public class TerrainGenerator
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
        /// The polygon generator used to create the terrain's basic shape.
        /// </summary>
        private readonly PolygonGenerator _polygonGenerator;
        /// <summary>
        /// The mesh fragmenter used to fragment a basic shape, creating a more detailed mesh.
        /// </summary>
        private readonly MeshFragmenter _fragmenter;
        /// <summary>
        /// The sculptor used to create hills/valleys on the mesh.
        /// </summary>
        private readonly Sculptor _sculptor;
        /// <summary>
        /// The height of the terraces (in units), in ascending order.
        /// </summary>
        private readonly float[] _terraceHeights;

        #endregion

        #region Setup

        /// <summary>
        /// <see cref="TerrainGenerator"/>'s constructor.
        /// </summary>
        /// <param name="sides">Number of sides of the terrain's basic shape. Value must be between 3 and 10. </param>
        /// <param name="radius">The terrain's radius?</param>
        /// <param name="maximumHeight">The maximum height of the terrain, in units. In order words, distance
        /// between its lowest and highest point.</param>
        /// <param name="relativeTerraceHeights">Terrace heights, relative to the terrain's maximum height. Values
        /// must be in the  [0, 1] range, in ascending order. Each terrace's final height will be calculated by
        /// multiplying the relative height by the terrain's height.</param>
        /// <param name="sculptSettings">The settings used during the sculpting phase.</param>
        /// <param name="depth">Depth to fragment the basic mesh. Value must be greater than zero.</param>
        /// <exception cref="NotImplementedException">Thrown if the provided number of sides is not supported.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown whenever <paramref name="radius"/> is less or equal
        /// than zero or whenever <paramref name="relativeTerraceHeights"/> is either empty or if its values are
        /// invalid (not between [0,1] and in ascending order).
        /// </exception>
        public TerrainGenerator(ushort sides, float radius, float maximumHeight, float[] relativeTerraceHeights, 
	        SculptSettings sculptSettings, ushort depth)
        {
	        if (sides < 3)
		        throw new ArgumentOutOfRangeException(nameof(sides), "Sides must be greater than 2.");
	        
            if (radius <= 0)
                throw new ArgumentOutOfRangeException(nameof(radius), "Radius must be greater than zero.");
            
            if (maximumHeight <= 0)
	            throw new ArgumentOutOfRangeException(nameof(maximumHeight), "Height must be greater than zero.");
            
            if (depth == 0)
	            throw new ArgumentOutOfRangeException(nameof(depth), "Depth must be greater than zero.");

            if (relativeTerraceHeights.Length == 0)
                throw new ArgumentOutOfRangeException(nameof(relativeTerraceHeights), "Relative heights is empty.");

            // Check if relative terrace heights are valid.
            for (var i = 0; i < relativeTerraceHeights.Length; i++)
            {
	            var relativeHeight = relativeTerraceHeights[i];
	            if (relativeHeight is < 0 or > 1)
	            {
		            throw new ArgumentOutOfRangeException(nameof(relativeTerraceHeights),
			            "Relative heights must be greater than 0 and less than 1.");
	            }

	            if (i != 0 && relativeHeight <= relativeTerraceHeights[i - 1])
	            {
		            throw new ArgumentOutOfRangeException(nameof(relativeTerraceHeights),
			            "Relative heights must be in ascending order.");
	            }
            }
            
            _polygonGenerator = sides switch
            {
                3 => new TriangleGenerator(radius),
                4 => new SquareGenerator(radius),
                <= 10 => new RegularPolygonGenerator(sides, radius),
                _ => throw new NotImplementedException($"Polygon with {sides} not implemented")
            };

            _fragmenter = new MeshFragmenter(depth);
            _sculptor = new Sculptor(sculptSettings, maximumHeight);
            _terraceHeights = relativeTerraceHeights.Select(h => h * maximumHeight).ToArray();
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
            var terracer = new Terracer(meshData, _terraceHeights, SyncAllocator);
            terracer.CreateTerraces();
            terracer.BakeMeshData(SyncAllocator);
            var mesh = terracer.CreateMesh();
            terracer.Dispose();
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
                var t = new Terracer(meshData, _terraceHeights, AsyncAllocator);
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

        #region Private

        private SimpleMeshData GenerateTerrainData(Allocator allocator)
        {
            var meshData = _polygonGenerator.Generate(allocator);
            var fragmentedMeshData = _fragmenter.Fragment(meshData, allocator);
            meshData.Dispose();
            _sculptor.Sculpt(fragmentedMeshData);
            return fragmentedMeshData;
        }

        #endregion
    }
}