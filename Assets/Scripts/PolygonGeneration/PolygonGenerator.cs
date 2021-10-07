using System;
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
        /// <exception cref="ArgumentException">Thrown if the radius is not positive.</exception>
        protected PolygonGenerator(float radius)
        {
            if (radius <= 0f)
                throw new ArgumentException("Radius must be positive");
            Radius = radius;
        }

        #endregion
        
        #region Internal

        /// <summary>
        /// Generates the polygon <see cref="Mesh"/>.
        /// </summary>
        /// <returns>The generated mesh.</returns>
        internal abstract Mesh Generate();

        #endregion

    }
}