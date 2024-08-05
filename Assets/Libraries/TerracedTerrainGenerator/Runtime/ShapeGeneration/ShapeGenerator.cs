using LazySquirrelLabs.TerracedTerrainGenerator.Data;
using Unity.Collections;

namespace LazySquirrelLabs.TerracedTerrainGenerator.ShapeGeneration
{
	/// <summary>
	/// Base class for all shape generators.
	/// </summary>
	internal abstract class ShapeGenerator
	{
		#region Internal

		/// <summary>
		/// Generates the shape.
		/// </summary>
		/// <param name="allocator">The allocation strategy used when creating vertex and index buffers.</param>
		/// <returns>The generated mesh.</returns>
		internal abstract SimpleMeshData Generate(Allocator allocator);

		#endregion
	}
}