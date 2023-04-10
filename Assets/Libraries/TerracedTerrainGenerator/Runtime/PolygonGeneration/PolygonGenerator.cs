using SneakySquirrelLabs.TerracedTerrainGenerator.Data;
using Unity.Collections;
using UnityEngine;

namespace SneakySquirrelLabs.TerracedTerrainGenerator.PolygonGeneration
{
    /// <summary>
    /// Base class for all polygon generators.
    /// </summary>
    internal abstract class PolygonGenerator
    {
        #region Properties

        /// <summary>
        /// The distance between the the center and the most distant point of the terrain.
        /// </summary>
        protected float Radius { get; }

        #endregion

        #region Setup
        
        /// <summary>
        /// Creates a polygon generator. Used by subclasses only.
        /// </summary>
        /// <param name="radius">The radius of the generated polygon (distance between the center and its
        /// farthest point).</param>
        protected PolygonGenerator(float radius)
        {
            Radius = radius;
        }

        #endregion
        
        #region Internal
        
        /// <summary>
        /// Generates the polygon <see cref="Mesh"/>.
        /// </summary>
        /// <param name="allocator">The allocation strategy used when creating vertex and index buffers.</param>
        /// <returns>The generated mesh.</returns>
        internal abstract SimpleMeshData Generate(Allocator allocator);

        #endregion
    }
}