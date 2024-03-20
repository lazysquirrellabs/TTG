using UnityEngine;

namespace LazySquirrelLabs.TerracedTerrainGenerator.SphereGeneration
{
	internal class SphereGenerator : MonoBehaviour
	{
		[SerializeField] private MeshFilter _otherMeshFilter;
		[SerializeField] private int[] _otherIndices;
		[SerializeField] private Vector3[] _vertices;
		[SerializeField] private bool _useNewVertices;

		private readonly Vector3[] _newVertices =
		{
			new(-0.8506508f, 0.5257311f, 0f),
			new(-0.8506508f, -0.5257311f, 0f),
			new(0.8506508f, 0.5257311f, 0f),
			new(0.8506508f, -0.5257311f, 0f),
			new(-0.52573115f, 0.00000006267203f, -0.85065067f),
			new(0.5257309f, -0.00000006267203f, -0.85065067f),
			new(-0.5257309f, 0.00000006267203f, 0.85065067f),
			new(0.52573115f, -0.00000006267203f, 0.85065067f),
			new(-0.000000101405476f, -0.8506506f, -0.525731f),
			new(-0.000000101405476f, -0.8506507f, 0.525731f),
			new(0.000000101405476f, 0.8506507f, -0.525731f),
			new(0.000000101405476f, 0.8506506f, 0.525731f)
		};
		
		private void Update()
		{
			var mesh = new Mesh();
			var vertices = _useNewVertices ? _newVertices : _vertices;
			mesh.SetVertices(vertices);
			mesh.SetIndices(_otherIndices, MeshTopology.Triangles, 0);
			mesh.RecalculateNormals();
			mesh.RecalculateBounds();
			_otherMeshFilter.mesh = mesh;
		}
	}
}