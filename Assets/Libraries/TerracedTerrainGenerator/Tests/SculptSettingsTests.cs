using System;
using NUnit.Framework;
using SneakySquirrelLabs.TerracedTerrainGenerator.Sculpting;
using UnityEngine;

namespace SneakySquirrelLabs.TerracedTerrainGenerator.Tests
{
	internal class SculptSettingsTests
	{
		[Test]
		public void Constructor()
		{
			var baseFrequency = -1.5f;
			uint octaves = 3;
			var persistence = 0.5f;
			var lacunarity = 1.5f;
			var heightDistribution = AnimationCurve.Linear(0, 0, 1, 1);
			
			// Negative frequency
			Assert.Throws<ArgumentOutOfRangeException>(Create);
			baseFrequency = 0.1f;
			octaves = 0;
			// No octaves
			Assert.Throws<ArgumentOutOfRangeException>(Create);
			octaves = 1;
			persistence = -1.2f;
			// Negative persistence
			Assert.Throws<ArgumentOutOfRangeException>(Create);
			persistence = 0.5f;
			lacunarity = -2.5f;
			// Negative lacunarity
			Assert.Throws<ArgumentOutOfRangeException>(Create);
			// Height distribution can be null
			heightDistribution = null;
			lacunarity = 1.5f;
			Create();

			void Create()
			{
				_ = new SculptSettings(6, baseFrequency, octaves, persistence, lacunarity,
					heightDistribution);
			}
		}
	}
}