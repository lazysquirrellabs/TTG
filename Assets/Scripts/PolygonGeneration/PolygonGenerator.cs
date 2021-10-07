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
        
        protected PolygonGenerator(float radius)
        {
            if (Radius <= 0f)
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