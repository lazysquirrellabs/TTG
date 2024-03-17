using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LazySquirrelLabs.TerracedTerrainGenerator.SphereGeneration
{
	internal class SphereGenerator : MonoBehaviour
	{
		[SerializeField] private MeshFilter _meshFilter;
		[SerializeField] private Vector3 _v1;
		[SerializeField] private Vector3 _v2;

		private const float Width = 0.85065080835157f;
		private const float Height = 0.525731112119134f;

		private static readonly Vector3[] Points = 
		{
			new(-Width,  Height), // Top left
			new(-Width, -Height), // Bottom left
			new( Width,  Height), // Top Right
			new( Width, -Height)  // Bottom right
		};
		private void Update()
		{
			var vertices = new List<Vector3>();
			var indices = new List<int>();
			var uvs = new List<Vector2>();
			var mesh = new Mesh();
			AddPlane(Quaternion.identity, 0);
			var planeRotation = Quaternion.Euler(90, -90, 0);
			AddPlane(planeRotation, 1);
			planeRotation = Quaternion.Euler(0, -90, 90);
			AddPlane(planeRotation, 2);
			mesh.SetVertices(vertices);
			mesh.SetIndices(indices, MeshTopology.Triangles, 0);
			mesh.SetUVs(0, uvs);
			mesh.RecalculateNormals();
			mesh.RecalculateBounds();
			_meshFilter.mesh = mesh;

			void AddPlane(Quaternion rotation, int count)
			{
				var rotatedPoints = Points.Select(p => rotation * p);
				vertices.AddRange(rotatedPoints);
				var indexOffset = count * 4;
				indices.Add(indexOffset);
				indices.Add(indexOffset + 2);
				indices.Add(indexOffset + 3);
				indices.Add(indexOffset);
				indices.Add(indexOffset + 3);
				indices.Add(indexOffset + 1);
				uvs.Add(new Vector2(0, 1));
				uvs.Add(new Vector2(0,0));
				uvs.Add(new Vector2(1,1));
				uvs.Add(new Vector2(1,0));
			}
		}
	}
}