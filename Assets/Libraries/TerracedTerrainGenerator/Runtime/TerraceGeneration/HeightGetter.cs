using UnityEngine;

namespace LazySquirrelLabs.TerracedTerrainGenerator.TerraceGeneration
{
	/// <summary>
	/// A method that finds the height of a <paramref name="vertex"/>.
	/// </summary>
	internal delegate float HeightGetter(Vector3 vertex);
}