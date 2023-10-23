using System;
using System.Threading;
using System.Threading.Tasks;
using SneakySquirrelLabs.TerracedTerrainGenerator.Sculpting;
using UnityEngine;

namespace SneakySquirrelLabs.TerracedTerrainGenerator
{
	public class TerrainGeneratorController : MonoBehaviour
	{
		#region Serialized fields

		[Tooltip("Should a new, random terrain be generated on start?")]
		[SerializeField] private bool _generateOnStart = true;
		[SerializeField] private Renderer _renderer;
		[SerializeField] private MeshFilter _meshFilter;
		
		[Header("Generation settings")]
		[Tooltip("The number of sides of the terrain's basic shape.")]
		[SerializeField, Range(3, 10)] private ushort _sides = 8;
		[Tooltip("The greatest distance between the center of the mesh and all of its vertices " +
		         "(ignoring their position's Y coordinate).")]
		[SerializeField, Range(1, 100)] private float _radius = 20;
		[Tooltip("How many times the basic shape will be fragmented to form the terrain. " +
		         "The larger the value, the greater the level of detail will be (more triangles and vertices) and " +
		         "the longer the generation process takes.")]
		[SerializeField, Range(0, 10)] private ushort _depth = 5;
		
		[Header("Deformation settings")]
		[Tooltip("The maximum height of the generated terrain, in units.")]
		[SerializeField, Range(0.1f, 100)] private float _height = 10;
		[Tooltip("The degree of detail in the generated terrain (hills and valleys) in a given area.")]
		[SerializeField, Range(0.01f, 1f)] private float _frequency = 0.075f;
		[Tooltip("Height distribution over the terrain: how low valleys and how high hills should be, " +
		         "and everything in between. This curve must start in (0,0) and end in (1,1).")]
		[SerializeField] private AnimationCurve _heightCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
		
		[Space(10)]
		[Tooltip("Terrace heights, relative to the terrain's height. Values must be in the [0, 1] range, in " +
		         "ascending order. Each terrace's final height will be calculated by multiplying the relative height" +
		         " by the terrain's height.")]
		[SerializeField] private float[] _relativeTerraceHeights;

		// Used by its inspector to keep track of whether custom terrace heights are being used.
		[SerializeField] private bool _useCustomHeights;

		#endregion

		#region Fields

		private CancellationTokenSource _cancellationTokenSource;

		#endregion

		#region Setup

		private void Awake()
		{
			_cancellationTokenSource = new CancellationTokenSource();
		}

		private void Start()
		{
			if (_generateOnStart)
				GenerateTerrain();
		}

		private void OnDestroy()
		{
			_cancellationTokenSource?.Cancel();
			var mesh = _meshFilter.mesh;
			if (mesh)
			{
				mesh.Clear();
				Destroy(mesh);
			}
		}

		private void Reset()
		{
			var meshRenderer = GetComponent<MeshRenderer>();
			if (meshRenderer == null)
			{
				meshRenderer = gameObject.AddComponent<MeshRenderer>();
				var urpLit = Shader.Find("Universal Render Pipeline/Lit");
				if (!urpLit)
				{
					Debug.LogError("Failed to create URP Lit material when resetting the terrain generator " +
					                 "controller. Please assign renderer materials manually.");
					return;
				}
				var newMaterial = new Material(urpLit);
				newMaterial.name = "[Replace this] Placeholder material";
				meshRenderer.sharedMaterials = new[] { newMaterial };
			}
			_renderer = meshRenderer;

			var meshFilter = GetComponent<MeshFilter>();
			if (meshFilter == null)
				meshFilter = gameObject.AddComponent<MeshFilter>();
			_meshFilter = meshFilter;
		}

		#endregion

		#region Event handlers

		private void OnValidate()
		{
			if (_renderer == null) return;
			if (_relativeTerraceHeights == null) return;
			// If there's more materials then terraces, don't do anything.
			var materials = _renderer.sharedMaterials;
			var terraceCount = _relativeTerraceHeights.Length;
			if (materials.Length >= terraceCount) return;
			
			// If the current number of materials is less than the terrace count, create more materials. This simply
			// avoids forgetting to assign enough materials and can be easily discarded.
			var newMaterials = new Material[terraceCount];
			Array.Copy(materials, newMaterials, materials.Length);
			var lastMaterial = materials[^1];
			for (var i = materials.Length; i < terraceCount; i++)
				newMaterials[i] = lastMaterial;
			_renderer.sharedMaterials = newMaterials;
		}

		#endregion

		#region Public

		/// <summary>
		/// Generates and displays a random terraced terrain synchronously.
		/// </summary>
		public void GenerateTerrain()
		{
			var deformationSettings = new DeformationSettings(_height, _frequency, _heightCurve);
			Generate(deformationSettings);
		}

		/// <summary>
		/// Generates and displays a terraced terrain synchronously, based on the given <paramref name="seed"/>.
		/// </summary>
		/// <param name="seed">The seed used to feed the randomizer. The same seed will always generate the same
		/// terrain.</param>
		public void GenerateTerrain(int seed)
		{
			var deformationSettings = new DeformationSettings(seed, _height, _frequency, _heightCurve);
			Generate(deformationSettings);
		}
		
		/// <summary>
		/// Generates and displays a random terraced terrain asynchronously.
		/// </summary>
		/// <param name="token">Cancellation token that can be used to request task cancellation.</param>
		/// <returns>An awaitable Task that represents the terrain generation process.</returns>
		public async Task GenerateTerrainAsync(CancellationToken token)
		{
			var deformationSettings = new DeformationSettings(_height, _frequency, _heightCurve);
			await GenerateAsync(deformationSettings, token);
		}

		/// <summary>
		/// Generates and displays a terraced terrain asynchronously, based on the given <paramref name="seed"/>.
		/// </summary>
		/// <param name="seed">The seed used to feed the randomizer. The same seed will always generate the same
		/// terrain.</param>
		/// <param name="token">Cancellation token that can be used to request task cancellation.</param>
		/// <returns>An awaitable Task that represents the terrain generation process.</returns>
		public async Task GenerateTerrainAsync(int seed, CancellationToken token)
		{
			var deformationSettings = new DeformationSettings(seed, _height, _frequency, _heightCurve);
			await GenerateAsync(deformationSettings, token);
		}

		#endregion

		#region Private

		/// <summary>
		/// Generates a terrain synchronously.
		/// </summary>
		/// <param name="deformationSettings">The deformation settings </param>
		private void Generate(DeformationSettings deformationSettings)
		{
			// Generate
			var generator = new TerrainGenerator(_sides, _radius, deformationSettings, _depth, _relativeTerraceHeights);
			var previousMesh = _meshFilter.mesh;
			_meshFilter.mesh = generator.GenerateTerrain();
			// Cleanup
			if (previousMesh == null) return;
			previousMesh.Clear();
			Destroy(previousMesh);
		}
		
		private async Task GenerateAsync(DeformationSettings deformationSettings, CancellationToken token)
		{
			// Generate
			var generator = new TerrainGenerator(_sides, _radius, deformationSettings, _depth, _relativeTerraceHeights);
			var internalToken = _cancellationTokenSource.Token;
			var combinedSource = CancellationTokenSource.CreateLinkedTokenSource(internalToken, token);
			var previousMesh = _meshFilter.mesh;
			_meshFilter.mesh = await generator.GenerateTerrainAsync(combinedSource.Token);
			// Cleanup
			if (previousMesh == null) return;
			previousMesh.Clear();
			Destroy(previousMesh);
		}

		#endregion
	}
}