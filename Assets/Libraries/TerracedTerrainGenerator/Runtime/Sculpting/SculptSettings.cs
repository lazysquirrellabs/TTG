using System;
using UnityEngine;

namespace SneakySquirrelLabs.TerracedTerrainGenerator.Sculpting
{
	/// <summary>
	/// Settings used during terrain sculpting, the step that creates hills and valleys on the terrain.
	/// </summary>
	public readonly struct SculptSettings
	{
		#region Properties

		/// <summary>
		/// Seed used by the randomizer to get random noise values.
		/// </summary>
		public int Seed { get; }

		/// <summary>
		/// The base (first octave's) degree of detail (hills and valleys) in a given area.
		/// </summary>
		public float BaseFrequency { get; }
		
		/// <summary>
		/// How many octaves (iterations) the sculpting will run. Each octave will run on a higher frequency and lower
		/// relevance than the previous one. The higher the value, the more variation the terrain will contain, but
		/// there is a point of diminishing returns due to persistence."
		/// </summary>
		public uint Octaves { get; }
		
		/// <summary>
		/// >How much of an octave's amplitude will be carried to the next octave. The lower the value, the quicker
		/// octave details disappear with each iteration.
		/// </summary>
		public float Persistence { get; }
		
		/// <summary>
		/// How the frequency will be affected (multiplication factor) between octaves. In other words, how much" detail
		/// each octave will contain, when compared to the previous one.
		/// </summary>
		public float Lacunarity { get; }

		/// <summary>
		/// The curve used to change the height distribution.
		/// </summary>
		public AnimationCurve HeightDistribution { get; }

		#endregion

		#region Setup

		/// <summary>
		/// <see cref="SculptSettings"/>'s constructor. Initializes the sculptor with a random seed.
		/// </summary>
		/// <param name="baseFrequency">The number of terrain features (hills and valleys) in a given area on the first
		/// iteration (a.k.a. octave) of the sculpting process. Value must be greater than zero.</param>
		/// <param name="octaves">How many octaves (iterations) the sculpting will run. Each octave will run on a
		/// higher frequency and lower relevance than the previous one. The higher the value, the more variation
		/// the terrain will contain, but there is a point of diminishing returns due to
		/// <paramref name="persistence"/>. Value must be greater than zero.</param>
		/// <param name="persistence">How much of an octave's amplitude will be carried to the next octave. The lower
		/// the value, the quicker octave details disappear with each iteration. If <paramref name="octaves"/> is
		/// greater than 1, this value must be greater than zero and less than 1.</param>
		/// <param name="lacunarity">How much the frequency increases (multiplication factor) between octaves.
		/// In other words, how much detail each octave will contain, when compared to the previous one. If
		/// <paramref name="octaves"/> is greater than 1, this value must be greater than one.</param>
		/// <param name="heightDistribution">The curve used to change the height distribution. If this value is null, a
		/// canonical value (a linear curve that won't affect the distribution) will be used.</param>
		/// <exception cref="ArgumentOutOfRangeException">Thrown if any of the arguments is out of range. Checks 
		/// individual arguments for valid ranges.</exception>
		public SculptSettings(float baseFrequency, uint octaves, float persistence, float lacunarity, 
			AnimationCurve heightDistribution) 
			: this(GetRandomSeed(), baseFrequency, octaves, persistence, lacunarity, heightDistribution) { }

		/// <summary>
		/// <see cref="SculptSettings"/>'s constructor.
		/// </summary>
		/// <param name="seed">Seed used by the randomizer.</param>
		/// <param name="baseFrequency">The number of terrain features (hills and valleys) in a given area on the first
		/// iteration (a.k.a. octave) of the sculpting process. Value must be greater than zero.</param>
		/// <param name="octaves">How many octaves (iterations) the sculpting will run. Each octave will run on a
		/// higher frequency and lower relevance than the previous one. The higher the value, the more variation
		/// the terrain will contain, but there is a point of diminishing returns due to
		/// <paramref name="persistence"/>. Value must be greater than zero.</param>
		/// <param name="persistence">How much of an octave's amplitude will be carried to the next octave. The lower
		/// the value, the quicker octave details disappear with each iteration. If <paramref name="octaves"/> is
		/// greater than 1, this value must be greater than zero and less than 1.</param>
		/// <param name="lacunarity">How much the frequency increases (multiplication factor) between octaves.
		/// In other words, how much detail each octave will contain, when compared to the previous one. If
		/// <paramref name="octaves"/> is greater than 1, this value must be greater than one.</param>
		/// <param name="heightDistribution">The curve used to change the height distribution. If this value is null, a
		/// canonical value (a linear curve that won't affect the distribution) will be used.</param>
		/// <exception cref="ArgumentOutOfRangeException">Thrown if any of the arguments is out of range. Checks 
		/// individual arguments for valid ranges.</exception>
		public SculptSettings(int seed, float baseFrequency, uint octaves, float persistence, float lacunarity, 
			AnimationCurve heightDistribution)
		{
			if (baseFrequency <= 0)
				throw new ArgumentOutOfRangeException(nameof(baseFrequency));

			if (octaves == 0)
				throw new ArgumentOutOfRangeException(nameof(octaves), "Generation must contain at least one octave.");
			
			// Only check persistence if there is more than 1 octave, otherwise its value is irrelevant.
			if (octaves > 1 && persistence is <= 0 or >= 1)
				throw new ArgumentOutOfRangeException(nameof(persistence), "Persistence must be in the (0,1) range.");

			// Only check lacunarity if there is more than 1 octave, otherwise its value is irrelevant.
			if (octaves > 1 && lacunarity <= 1)
				throw new ArgumentOutOfRangeException(nameof(lacunarity), "Lacunarity must be greater than one.");

			Seed = seed;
			BaseFrequency = baseFrequency;
			Octaves = octaves;
			HeightDistribution = heightDistribution;
			Persistence = persistence;
			Lacunarity = lacunarity;
		}

		#endregion

		#region Private

		private static int GetRandomSeed()
		{
			var random = new System.Random();
			return random.Next();
		}

		#endregion
	}
}