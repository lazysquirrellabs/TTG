using System.Collections.Generic;
using UnityEngine;

namespace SneakySquirrelLabs.TerracedTerrainGenerator.TerraceGeneration
{
    internal sealed class Terracer
    {
        #region Fields

        private readonly List<Vector3> _vertices;
        private readonly List<int> _triangles;
        private readonly Mesh _mesh;
        private readonly float[] _planeHeights;
        private readonly uint _terraces;

        #endregion

        #region Setup

        internal Terracer(Mesh mesh, uint terraces)
        {
            // In the base case, there will be at least the same amount of vertices
            _vertices = new List<Vector3>(mesh.vertexCount);
            _triangles = new List<int>(mesh.triangles.Length);
            _mesh = mesh;
            _planeHeights = GetHeights(terraces);

            float[] GetHeights(uint count)
            {
                var lowestPoint = float.PositiveInfinity;
                var highestPoint = float.NegativeInfinity;
                var heights = new float[count];

                foreach (var vertex in mesh.vertices)
                {
                    lowestPoint = Mathf.Min(vertex.y, lowestPoint);
                    highestPoint = Mathf.Max(vertex.y, highestPoint);
                }

                var delta = (highestPoint - lowestPoint) / count;

                for (var i = 0; i < count; i++)
                    heights[i] = lowestPoint + i * delta;

                return heights;
            }

        }

        #endregion
    }
}