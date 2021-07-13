using UnityEngine;

namespace SneakySquirrelLabs.TerracedTerrainGenerator.PolygonGeneration
{
    internal abstract class PolygonGenerator
    {
        #region Properties

        protected float Radius { get; }

        #endregion

        #region Setup

        protected PolygonGenerator(float radius)
        {
            Radius = radius;
        }

        #endregion
        
        #region Internal

        internal abstract Mesh Generate();

        #endregion

    }
}