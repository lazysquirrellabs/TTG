using System;
using System.Threading;
using System.Threading.Tasks;
using SneakySquirrelLabs.TerracedTerrainGenerator.Settings;
using UnityEngine;

namespace SneakySquirrelLabs.TerracedTerrainGenerator
{
	[RequireComponent(typeof(Renderer), typeof(MeshFilter))]
	public class TerrainGeneratorController : MonoBehaviour
	{
		#region Serialized fields

		[Tooltip("Should a new, random terrain be generated on start?")]
		[SerializeField] private bool _generateOnStart;
		[SerializeField] private Renderer _renderer;
		[SerializeField] private MeshFilter _meshFilter;
		
		[Header("Generation settings")]
		[Tooltip("The number of sides of the terrain's basic shape.")]
		[SerializeField, Range(3, 10)] private ushort _sides;
		[Tooltip("The greatest distance between the center of the mesh and all of its vertices " +
		         "(ignoring their position's Y coordinate).")]
		[SerializeField, Range(1, 100)] private float _radius;
		[Tooltip("How many terraces the terrain will contain.")]
		[SerializeField, Range(1, 50)] private int _terraceCount;
		[Tooltip("How many times the basic shape will be fragmented to form the terrain. " +
		         "The larger the value, the greater the level of detail will be (more triangles and vertices) and " +
		         "the longer the generation process takes.")]
		[SerializeField, Range(0, 10)] private ushort _depth;
		
		[Header("Deformation settings")]
		[Tooltip("The maximum height of the generated terrain, in units.")]
		[SerializeField, Range(0.1f, 100)] private float _height;
		[Tooltip("The degree of detail in the generated terrain (hills and valleys) in a given area.")]
		[SerializeField, Range(0.01f, 1f)] private float _frequency;
		[Tooltip("Height distribution over the terrain: how low valleys and how high hills should be, " +
		         "and everything in between. This curve must start in (0,0) and end in (1,1).")]
		[SerializeField] private AnimationCurve _heightCurve;

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
		}

		#endregion

		#region Event handlers

		private void OnValidate()
		{
			if (_renderer == null) return;

			// If there's more materials then terraces, don't do anything.
			var materials = _renderer.sharedMaterials;
			if (materials.Length >= _terraceCount) return;
			
			// If the current number of materials is less than the terrace count, create more materials. This simply
			// avoids forgetting to assign enough materials and can be easily discarded.
			var newMaterials = new Material[_terraceCount];
			Array.Copy(materials, newMaterials, materials.Length);
			var lastMaterial = materials[^1];
			for (var i = materials.Length; i < _terraceCount; i++)
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
			var generator = new TerrainGenerator(_sides, _radius, deformationSettings, _depth, _terraceCount);
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
			var generator = new TerrainGenerator(_sides, _radius, deformationSettings, _depth, _terraceCount);
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