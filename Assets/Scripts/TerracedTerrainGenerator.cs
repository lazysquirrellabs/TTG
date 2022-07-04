using System;
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
        /// The seed used to feed the randomizer.
        /// </summary>
        private readonly int _seed;
        /// <summary>
        /// The polygon generator used to create the terrain's basic shape.
        /// </summary>
        private readonly PolygonGenerator _polygonGenerator;
        /// <summary>
        /// How many iterations of the fragmentation should be performed.
        /// </summary>
        private readonly ushort _depth;
        /// <summary>
        /// The maximum height of the generated terrain.
        /// </summary>
        private readonly float _height;
        /// <summary>
        /// The frequency of the deformation.
        /// </summary>
        private readonly float _frequency;
        /// <summary>
        /// The number of terraces to create.
        /// </summary>
        private readonly int _terraces;
        /// <summary>
        /// The curve used to modify the height distribution during the deformation (valley/ hill) generation phase. 
        /// </summary>
        private readonly AnimationCurve _heightDistribution;

        #endregion

        #region Setup

        /// <summary>
        /// <see cref="TerrainGenerator"/>'s constructor.
        /// </summary>
        /// <param name="seed">Seed used by the randomizer to generate the terrain. Use this call if you'd like
        ///     reproducible generation.</param>
        /// <param name="sides">Number of sides of the terrain's basic shape. Value must be between 3 and 10. </param>
        /// <param name="radius">The terrain's radius?</param>
        /// <param name="height">The maximum height of the generated terrain.</param>
        /// <param name="frequency">The frequency of deformation.</param>
        /// <param name="depth">Depth to fragment the basic mesh.</param>
        /// <param name="terraces">The number of terraces to create.</param>
        /// <param name="heightDistribution">The curve used to modify the height distribution during the deformation
        /// (valley/hill) generation phase. If no curve is provided, the distribution won't be modified (thus it will be
        /// linear).</param>
        /// <exception cref="NotImplementedException">Thrown if the provided number of sides is
        /// not supported.</exception>
        public TerrainGenerator(int seed, ushort sides, float radius, float height, float frequency, ushort depth, 
            int terraces, AnimationCurve heightDistribution = null)
        {
            _polygonGenerator = sides switch
            {
                3 => new TriangleGenerator(radius),
                4 => new SquareGenerator(radius),
                <= 10 => new RegularPolygonGenerator(sides, radius),
                _ => throw new NotImplementedException($"Polygon with {sides} not implemented")
            };
            
            _seed = seed;
            _height = height;
            _frequency = frequency;
            _depth = depth;
            _terraces = terraces;
            _heightDistribution = heightDistribution;
        }

        /// <summary>
        /// <see cref="TerrainGenerator"/>'s constructor.
        /// </summary>
        /// <param name="sides">Number of sides of the terrain's basic shape. Value must be between 3 and 10. </param>
        /// <param name="radius">The terrain's radius?</param>
        /// <param name="height">The maximum height of the generated terrain.</param>
        /// <param name="frequency">The frequency of deformation.</param>
        /// <param name="depth">Depth to fragment the basic mesh.</param>
        /// <param name="terraces">The number of terraces to create.</param>
        /// <param name="heightDistribution">The curve used to modify the height distribution during the deformation
        /// (valley/hill) generation phase. If no curve is provided, the distribution won't be modified (thus it will be
        /// linear).</param>
        /// <exception cref="NotImplementedException">Thrown if the provided number of <paramref name="sides"/> is
        /// not supported.</exception>
        public TerrainGenerator(ushort sides, float radius, float height, float frequency, ushort depth, int terraces, 
            AnimationCurve heightDistribution = null)
        {
            _polygonGenerator = sides switch
            {
                3 => new TriangleGenerator(radius),
                4 => new SquareGenerator(radius),
                <= 10 => new RegularPolygonGenerator(sides, radius),
                _ => throw new NotImplementedException($"Polygon with {sides} not implemented")
            };
            
            var random = new System.Random();
            _seed = random.Next();
            _height = height;
            _frequency = frequency;
            _depth = depth;
            _terraces = terraces;
            _heightDistribution = heightDistribution;
        }

        #endregion
        
        #region Public

        /// <summary>
        /// Generates the entire terraced terrain.
        /// </summary>
        /// <returns>The generated <see cref="Mesh"/>.</returns>
        public Mesh GenerateTerrain()
        {
            var meshData = _polygonGenerator.Generate();
            var fragmenter = new MeshFragmenter(meshData, _depth);
            meshData = fragmenter.Fragment();
            var deformer = new PerlinDeformer(_seed, meshData, _heightDistribution);
            deformer.Deform(_height, _frequency);
            var terracer = new Terracer(meshData, _terraces);
            return terracer.CreateTerraces();
        }

        #endregion
    }
}