using System;
using System.Threading;
using System.Threading.Tasks;
using SneakySquirrelLabs.TerracedTerrainGenerator.Data;
using SneakySquirrelLabs.TerracedTerrainGenerator.Deformation;
using SneakySquirrelLabs.TerracedTerrainGenerator.MeshFragmentation;
using SneakySquirrelLabs.TerracedTerrainGenerator.PolygonGeneration;
using SneakySquirrelLabs.TerracedTerrainGenerator.Settings;
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
        /// <param name="sides">Number of sides of the terrain's basic shape. Value must be between 3 and 10. </param>
        /// <param name="radius">The terrain's radius?</param>
        /// <param name="deformationSettings">The settings used during the deformation phase.</param>
        /// <param name="depth">Depth to fragment the basic mesh.</param>
        /// <param name="terraces">The number of terraces to create.</param>
        /// <exception cref="NotImplementedException">Thrown if the provided number of sides is
        /// not supported.</exception>
        public TerrainGenerator(ushort sides, float radius, DeformationSettings deformationSettings, ushort depth, 
            int terraces)
        {
            if (radius <= 0)
                throw new ArgumentOutOfRangeException(nameof(radius));

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
            _deformer = new PerlinDeformer(deformationSettings);
            _terraces = terraces;
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
            // Run the mesh data generation (the heaviest part of the process) on the thread pool
            using var terracer =  await Task.Run(GenerateTerracedTerrainData, token);
            var mesh = terracer.CreateMesh();
            return mesh;

            Terracer GenerateTerracedTerrainData()
            {
                var meshData = GenerateTerrainData(AsyncAllocator);
                var t = new Terracer(meshData, _terraces, AsyncAllocator);
                t.CreateTerraces();
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
            _deformer.Deform(fragmentedMeshData);
            return fragmentedMeshData;
        }

        #endregion
    }
}