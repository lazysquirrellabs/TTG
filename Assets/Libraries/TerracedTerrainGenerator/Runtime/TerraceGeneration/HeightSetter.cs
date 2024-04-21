using UnityEngine;

namespace LazySquirrelLabs.TerracedTerrainGenerator.TerraceGeneration
{
	/// <summary>
	/// A method that returns a copy of a <paramref name="vertex" /> set at a given <paramref name="height" />.
	/// </summary>
	internal delegate Vector3 HeightSetter(Vector3 vertex, float height);
}