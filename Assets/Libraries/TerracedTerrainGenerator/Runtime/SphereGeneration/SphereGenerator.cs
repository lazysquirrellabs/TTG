using UnityEngine;

namespace LazySquirrelLabs.TerracedTerrainGenerator.SphereGeneration
{
	internal class SphereGenerator : MonoBehaviour
	{
		[SerializeField] private MeshFilter _otherMeshFilter;

		private readonly int[] _indices = {
			0, 1, 2, 0, 3, 1, 0, 2, 4, 3, 0, 5, 0, 4, 5, 1, 3, 6, 1, 7, 2, 7, 1, 6, 4, 2, 8, 7, 8, 2, 9, 3, 5, 6, 3, 9,
			5, 4, 10, 4, 8, 10, 9, 5, 10, 7, 6, 11, 7, 11, 8, 11, 6, 9, 8, 11, 10, 10, 11, 9
		};
		
		private readonly Vector3[] _vertices =
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
			new(-0.8506508f, -0.5257311f, 0f),
		};
		
		private void Start()
		{
			var mesh = new Mesh();
			mesh.SetVertices(_vertices);
			mesh.SetIndices(_indices, MeshTopology.Triangles, 0);
			mesh.RecalculateNormals();
			mesh.RecalculateBounds();
			_otherMeshFilter.mesh = mesh;
		}
	}
}