using System;
using System.Threading;
using System.Threading.Tasks;
using SneakySquirrelLabs.TerracedTerrainGenerator.Data;
using SneakySquirrelLabs.TerracedTerrainGenerator.Deformation;
using SneakySquirrelLabs.TerracedTerrainGenerator.MeshFragmentation;
using SneakySquirrelLabs.TerracedTerrainGenerator.PolygonGeneration;
using SneakySquirrelLabs.TerracedTerrainGenerator.TerraceGeneration;
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
        {
            _polygonGenerator = sides switch
            {
                3 => new TriangleGenerator(radius),
                4 => new SquareGenerator(radius),
                <= 10 => new RegularPolygonGenerator(sides, radius),
                _ => throw new NotImplementedException($"Polygon with {sides} not implemented")
            };
            
            var random = new System.Random();
            var seed = random.Next();
            _fragmenter = new MeshFragmenter(depth);
            _deformer = new PerlinDeformer(seed, maximumHeight, frequency, heightDistribution);
            _terraces = terraces;
        }
        
        #endregion
        
        #region Public

        /// <summary>
        /// Generates the entire terraced terrain.
        /// </summary>
        /// <returns>The generated <see cref="Mesh"/>.</returns>
        public Mesh GenerateTerrain()
        {
            var meshData = GenerateTerrainData();
            var terracer = new Terracer(meshData, _terraces);
            terracer.CreateTerraces();
            return terracer.CreateMesh();
        }

        public async Task<Mesh> GenerateTerrainAsync(CancellationToken token)
        {
            var synchronizationContext = SynchronizationContext.Current;
            var terracer =  await Task.Run(GenerateTerracedTerrainData, token);
            var generationState = new GenerationState();
            synchronizationContext.Send(CreateMesh, generationState);
            var mesh = await generationState.WaitForCompletion(token);
            return mesh;

            Terracer GenerateTerracedTerrainData()
            {
                var meshDaTa = GenerateTerrainData();
                var t = new Terracer(meshDaTa, _terraces);
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

        private SimpleMeshData GenerateTerrainData()
        {
            var meshData = _polygonGenerator.Generate();
            meshData = _fragmenter.Fragment(meshData);
            _deformer.Deform(meshData);
            return meshData;
        }

        #endregion
    }
}