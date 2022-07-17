using System;
using System.Threading;
using System.Threading.Tasks;
using SneakySquirrelLabs.TerracedTerrainGenerator.Data;
using SneakySquirrelLabs.TerracedTerrainGenerator.Deformation;
using SneakySquirrelLabs.TerracedTerrainGenerator.MeshFragmentation;
using SneakySquirrelLabs.TerracedTerrainGenerator.PolygonGeneration;
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
        /// The deformer used to create hills/valleys on the mesh.
        /// </summary>
        private readonly PerlinDeformer _deformer;
        /// <summary>
        /// The number of terraces to create.
        /// </summary>
        private readonly int _terraces;

        #endregion

        #region Setup

        /// <summary>
        /// <see cref="TerrainGenerator"/>'s constructor.
        /// </summary>
        /// <param name="seed">Seed used by the randomizer to generate the terrain. Use this call if you'd like
        ///     reproducible generation.</param>
        /// <param name="sides">Number of sides of the terrain's basic shape. Value must be between 3 and 10. </param>
        /// <param name="radius">The terrain's radius?</param>
        /// <param name="maximumHeight">The maximum height of the generated terrain.</param>
        /// <param name="frequency">The frequency of deformation.</param>
        /// <param name="depth">Depth to fragment the basic mesh.</param>
        /// <param name="terraces">The number of terraces to create.</param>
        /// <param name="heightDistribution">The curve used to modify the height distribution during the deformation
        /// (valley/hill) generation phase. If no curve is provided, the distribution won't be modified (thus it will be
        /// linear).</param>
        /// <exception cref="NotImplementedException">Thrown if the provided number of sides is
        /// not supported.</exception>
        public TerrainGenerator(int seed, ushort sides, float radius, float maximumHeight, float frequency, ushort depth, 
            int terraces, AnimationCurve heightDistribution = null)
        {
            if (radius <= 0)
                throw new ArgumentOutOfRangeException(nameof(radius));

            if (maximumHeight <= 0)
                throw new ArgumentOutOfRangeException(nameof(maximumHeight));

            if (frequency <= 0)
                throw new ArgumentOutOfRangeException(nameof(frequency));

            if (terraces <= 0)
                throw new ArgumentOutOfRangeException(nameof(terraces));
            
            _polygonGenerator = sides switch
            {
                3 => new TriangleGenerator(radius),
                4 => new SquareGenerator(radius),
                <= 10 => new RegularPolygonGenerator(sides, radius),
                _ => throw new NotImplementedException($"Polygon with {sides} not implemented")
            };

            _fragmenter = new MeshFragmenter(depth);
            _deformer = new PerlinDeformer(seed, maximumHeight, frequency, heightDistribution);
            _terraces = terraces;
        }

        /// <summary>
        /// <see cref="TerrainGenerator"/>'s constructor.
        /// </summary>
        /// <param name="sides">Number of sides of the terrain's basic shape. Value must be between 3 and 10. </param>
        /// <param name="radius">The terrain's radius?</param>
        /// <param name="maximumHeight">The maximum height of the generated terrain.</param>
        /// <param name="frequency">The frequency of deformation.</param>
        /// <param name="depth">Depth to fragment the basic mesh.</param>
        /// <param name="terraces">The number of terraces to create.</param>
        /// <param name="heightDistribution">The curve used to modify the height distribution during the deformation
        /// (valley/hill) generation phase. If no curve is provided, the distribution won't be modified (thus it will be
        /// linear).</param>
        /// <exception cref="NotImplementedException">Thrown if the provided number of <paramref name="sides"/> is
        /// not supported.</exception>
        public TerrainGenerator(ushort sides, float radius, float maximumHeight, float frequency, ushort depth,
            int terraces, AnimationCurve heightDistribution = null)
            : this(GetRandomSeed(), sides, radius, maximumHeight, frequency, depth, terraces, heightDistribution)
        {
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
            var terracer = new Terracer(meshData, _terraces, SyncAllocator);
            terracer.CreateTerraces();
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
            // Capture Unity's main thread's synchronization context
            var synchronizationContext = SynchronizationContext.Current;
            // Run the mesh data generation (the heaviest part of the process) on the thread pool
            using var terracer =  await Task.Run(GenerateTerracedTerrainData, token);
            var generationState = new GenerationState();
            // Use the synchronization context to send the Mesh creation process (the lightest part of the process)
            // to the main thread.
            synchronizationContext.Send(CreateMesh, generationState);
            // Wait for the mesh generation to complete on the main thread
            var mesh = await generationState.WaitForCompletionAsync(token);
            return mesh;

            Terracer GenerateTerracedTerrainData()
            {
                var meshData = GenerateTerrainData(AsyncAllocator);
                var t = new Terracer(meshData, _terraces, AsyncAllocator);
                t.CreateTerraces();
                return t;
            }
            
            void CreateMesh(object s)
            {
                var state = (GenerationState)s;
                state.Mesh = terracer.CreateMesh();
            }
        }

        #endregion

        #region Private

        private SimpleMeshData GenerateTerrainData(Allocator allocator)
        {
            var meshData = _polygonGenerator.Generate(allocator);
            var fragmentedMeshData = _fragmenter.Fragment(meshData, allocator);
            meshData.Dispose();
            _deformer.Deform(fragmentedMeshData);
            return fragmentedMeshData;
        }

        private static int GetRandomSeed()
        {
            var random = new System.Random();
            return random.Next();
        }

        #endregion
    }
}