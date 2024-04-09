using LazySquirrelLabs.TerracedTerrainGenerator.Data;
using Unity.Collections;
using UnityEngine;

namespace LazySquirrelLabs.TerracedTerrainGenerator.ShapeGeneration.Sphere
{
	internal class SphereGenerator : ShapeGenerator
	{
		#region Fields
		
		private static readonly int[] Indices = {
			0, 1, 2, 
			0, 3, 1, 
			0, 2, 4, 
			3, 0, 5, 
			0, 4, 5, 
			1, 3, 6, 
			1, 7, 2, 
			7, 1, 6, 
			4, 2, 8, 
			7, 8, 2, 
			9, 3, 5, 
			6, 3, 9,
			5, 4, 10, 
			4, 8, 10, 
			9, 5, 10, 
			7, 6, 11, 
			7, 11, 8, 
			11, 6, 9, 
			8, 11, 10, 
			10, 11, 9
		};
		
		private static readonly Vector3[] Vertices =
		{
			new(0.8506508f, 0.5257311f, 0f),
			new(0.000000101405476f, 0.8506507f, -0.525731f),
			new(0.000000101405476f, 0.8506506f, 0.525731f),
			new(0.5257309f, -0.00000006267203f, -0.85065067f),
			new(0.52573115f, -0.00000006267203f, 0.85065067f),
			new(0.8506508f, -0.5257311f, 0f),
			new(-0.52573115f, 0.00000006267203f, -0.85065067f),
			new(-0.8506508f, 0.5257311f, 0f),
			new(-0.5257309f, 0.00000006267203f, 0.85065067f),
			new(-0.000000101405476f, -0.8506506f, -0.525731f),
			new(-0.000000101405476f, -0.8506507f, 0.525731f),
			new(-0.8506508f, -0.5257311f, 0f)
		};

		private readonly float _radius;

		#endregion

		#region Setup

		public SphereGenerator(float radius)
		{
			_radius = radius;
		}

		#endregion

		#region Internal

		internal override SimpleMeshData Generate(Allocator allocator)
		{
			var vertices = new NativeArray<Vector3>(Vertices, allocator);
			for (var i = 0; i < Vertices.Length; i++)
				vertices[i] = Vertices[i] * _radius;
			var indices = new NativeList<int>(Indices.Length, allocator);
			foreach (var t in Indices)
				indices.Add(t);

			return new SimpleMeshData(vertices, indices);
		}

		#endregion

		
	}
}