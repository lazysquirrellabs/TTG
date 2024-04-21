using System;
using LazySquirrelLabs.TerracedTerrainGenerator.Sculpting;
using NUnit.Framework;
using UnityEngine;

namespace LazySquirrelLabs.TerracedTerrainGenerator.Tests
{
	internal class TerrainGeneratorTests
	{
		[Test]
		public void PlaneConstructor()
		{
			ushort sides = 0;
			var radius = 12.3f;
			var maximumHeight = 10f;
			var heights = new[] { 0.1f, 0.2f, 0.9f };
			var settings = new SculptSettings(0.1f, 2, 0.5f, 1.5f, AnimationCurve.Linear(0, 0, 1, 1));
			ushort depth = 3;

			// Invalid sides
			Assert.Throws<ArgumentOutOfRangeException>(Create);
			sides = 1;
			Assert.Throws<ArgumentOutOfRangeException>(Create);
			sides = 2;
			Assert.Throws<ArgumentOutOfRangeException>(Create);
			sides = 11;
			Assert.Throws<NotImplementedException>(Create);
			sides = 3;
			// Invalid radius
			radius = -40f;
			Assert.Throws<ArgumentOutOfRangeException>(Create);
			radius = 0;
			Assert.Throws<ArgumentOutOfRangeException>(Create);
			radius = 10;
			// Invalid maximum height
			maximumHeight = -10f;
			Assert.Throws<ArgumentOutOfRangeException>(Create);
			maximumHeight = 0f;
			Assert.Throws<ArgumentOutOfRangeException>(Create);
			maximumHeight = 20f;
			// Invalid custom heights
			heights = Array.Empty<float>();
			Assert.Throws<ArgumentOutOfRangeException>(Create);
			heights = new[] { 0.5f, 0.3f };
			Assert.Throws<ArgumentOutOfRangeException>(Create);
			heights = new[] { -0.1f };
			Assert.Throws<ArgumentOutOfRangeException>(Create);
			heights = new[] { 1.1f };
			Assert.Throws<ArgumentOutOfRangeException>(Create);
			heights = new[] { 0.1f, 0.2f, 0.3f };
			// Invalid depth
			depth = 0;
			Assert.Throws<ArgumentOutOfRangeException>(Create);
			depth = 3;
			// Nothing wrong, just to ensure no other parameters were still invalid
			Create();
			return;

			void Create()
			{
				_ = new PlaneTerrainGenerator(sides, radius, maximumHeight, heights, settings, depth);
			}
		}
	}
}