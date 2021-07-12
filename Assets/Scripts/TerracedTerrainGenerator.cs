using System;
using SneakySquirrelLabs.TerracedTerrainGenerator.PolygonGeneration;
using UnityEngine;

namespace SneakySquirrelLabs.TerracedTerrainGenerator
{
    public class TerrainGenerator
    {
        #region Fields

        private readonly Vector3 _position;
        private readonly PolygonGenerator _polygonGenerator;

        #endregion

        #region Setup

        public TerrainGenerator(uint sides, float radius, Vector3 position)
        {
            _polygonGenerator = sides switch
            {
                3 => new TriangleGenerator(radius),
                4 => new SquareGenerator(radius),
                _ when sides <= 10  => new RegularPolygonGenerator(sides, radius),
                _ => throw new NotImplementedException($"Polygon with {sides} not implemented")
            };

            _position = position;
        }

        #endregion
        
        #region Public

        public GameObject GenerateTerrain()
        {
            var rootGameObject = new GameObject("Terraced Terrain");
            rootGameObject.transform.position = _position;
            var meshFilter = rootGameObject.AddComponent<MeshFilter>();
            rootGameObject.AddComponent<MeshRenderer>();
            meshFilter.mesh = _polygonGenerator.Generate();
            return rootGameObject;
        }

        #endregion
    }
}