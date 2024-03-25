using LazySquirrelLabs.TerracedTerrainGenerator.Data;
using Random = System.Random;

namespace LazySquirrelLabs.TerracedTerrainGenerator.Sculpting
{
	/// <summary>
	/// Sculpts a terrain mesh using a Perlin filter.
	/// </summary>
	internal abstract class Sculptor 
	{
		#region Properties

		/// <summary>
		/// The height of the highest possible vertex after sculpting.
		/// </summary>
		protected float MaximumHeight { get; }

		/// <summary>
		/// The settings used for sculpting.
		/// </summary>
		protected SculptSettings Settings { get; }
		
		/// <summary>
		/// The random generator used to calculate noise offsets.
		/// </summary>
		protected Random Random { get; }

		#endregion

		#region Setup

		/// <summary>
		/// Creates a <see cref="Sculptor"/> with the given settings.
		/// </summary>
		/// <param name="sculptSettings">The settings used for sculpting.</param>
		/// <param name="maximumHeight"> The height of the highest possible vertex after sculpting.</param>
		protected Sculptor(SculptSettings sculptSettings, float maximumHeight)
		{
			Settings = sculptSettings;
			MaximumHeight = maximumHeight;
			Random = new Random(Settings.Seed);
		}

		#endregion
		
		#region Internal

		/// <summary>
		/// Sculpts the given terrain mesh.
		/// </summary>
		/// <param name="meshData">The mesh data to be sculpted.</param>
		internal abstract void Sculpt(SimpleMeshData meshData);

		#endregion
	}
}