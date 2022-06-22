using System;
using SneakySquirrelLabs.TerracedTerrainGenerator.Deformation;
using SneakySquirrelLabs.TerracedTerrainGenerator.MeshFragmentation;
using SneakySquirrelLabs.TerracedTerrainGenerator.PolygonGeneration;
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
        /// The position of the generated terrain, in world space.
        /// </summary>
        private readonly Vector3 _position;
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

        #endregion

        #region Setup

        /// <summary>
        /// <see cref="TerrainGenerator"/>'s constructor.
        /// </summary>
        /// <param name="sides">Number of sides of the terrain's basic shape. Value must be between 3 and 10. </param>
        /// <param name="radius">The terrain's radius?</param>
        /// <param name="height">The maximum height of the generated terrain.</param>
        /// <param name="frequency">The frequency of deformation.</param>
        /// <param name="depth">Depth to fragment the basic mesh.</param>
        /// <param name="position">The position of the generated terrain (in world space).</param>
        /// <exception cref="NotImplementedException">Thrown if the provided number of sides is
        /// not supported.</exception>
        public TerrainGenerator(ushort sides, float radius, float height, float frequency, ushort depth, Vector3 position)
        {
            _polygonGenerator = sides switch
            {
                3 => new TriangleGenerator(radius),
                4 => new SquareGenerator(radius),
                _ when sides <= 10  => new RegularPolygonGenerator(sides, radius),
                _ => throw new NotImplementedException($"Polygon with {sides} not implemented")
            };

            _height = height;
            _frequency = frequency;
            _depth = depth;
            _position = position;
        }

        #endregion
        
        #region Public

        /// <summary>
        /// Generates the entire terraced terrain.
        /// </summary>
        /// <returns>The <see cref="GameObject"/> which holds the generated terrain.</returns>
        public MeshRenderer GenerateTerrain()
        {
            var rootGameObject = new GameObject("Terraced Terrain");
            rootGameObject.transform.position = _position;
            var meshFilter = rootGameObject.AddComponent<MeshFilter>();
            var meshRenderer = rootGameObject.AddComponent<MeshRenderer>();
            var mesh = _polygonGenerator.Generate(false);
            var fragmenter = new MeshFragmenter(mesh, _depth);
            fragmenter.Fragment(false);
            var deformer = new PerlinDeformer();
            deformer.Deform(mesh, _height, _frequency, true);
            meshFilter.mesh = mesh;
            return meshRenderer;
        }

        #endregion
    }
}