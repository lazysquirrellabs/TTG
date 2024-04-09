namespace LazySquirrelLabs.TerracedTerrainGenerator.ShapeGeneration.Plane
{
    /// <summary>
    /// Base class for all flat polygon generators.
    /// </summary>
    internal abstract class PolygonGenerator : ShapeGenerator
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
        /// <param name="radius">The radius of the generated polygon (distance between the center and its farthest
        /// point).</param>
        protected PolygonGenerator(float radius)
        {
            Radius = radius;
        }

        #endregion
    }
}