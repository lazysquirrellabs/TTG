using System;
using NUnit.Framework;
using LazySquirrelLabs.TerracedTerrainGenerator.Sculpting;
using UnityEngine;

namespace LazySquirrelLabs.TerracedTerrainGenerator.Tests
{
	internal class SculptSettingsTests
	{
		[Test]
		public void Constructor()
		{
			uint octaves = 3;
			var persistence = 0.5f;
			var lacunarity = 1.5f;
			var heightDistribution = AnimationCurve.Linear(0, 0, 1, 1);
			
			// Negative frequency
			var baseFrequency = -1.5f;
			Assert.Throws<ArgumentOutOfRangeException>(Create);
			
			// No octaves
			baseFrequency = 0.1f;
			octaves = 0;
			Assert.Throws<ArgumentOutOfRangeException>(Create);
			
			// Negative persistence
			octaves = 4;
			persistence = -1.2f;
			Assert.Throws<ArgumentOutOfRangeException>(Create);
			
			// Persistence greater than 1
			persistence = 2.12f;
			Assert.Throws<ArgumentOutOfRangeException>(Create);
			
			// No persistence check if octaves = 1
			octaves = 1;
			Create();
			
			// Negative lacunarity
			octaves = 5;
			persistence = 0.5f;
			lacunarity = -2.5f;
			Assert.Throws<ArgumentOutOfRangeException>(Create);
			
			// Lacunarity smaller than 1
			lacunarity = 0.5f;
			Assert.Throws<ArgumentOutOfRangeException>(Create);
			
			// No lacunarity check if octaves = 1
			octaves = 1;
			Create();
			
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