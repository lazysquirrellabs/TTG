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

		[SerializeField] private bool _generateOnStart = true;
		[SerializeField] private Renderer _renderer;
		[SerializeField] private MeshFilter _meshFilter;
		[SerializeField] private ushort _sides = 8;
		[SerializeField] private float _radius = 20;
		[SerializeField] private ushort _depth = 5;
		[SerializeField] private float _maximumHeight = 10;
		[SerializeField] private float _frequency = 0.075f;
		[SerializeField] private AnimationCurve _heightCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
		[SerializeField] private float[] _relativeHeights;

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
			if (_relativeHeights == null) return;
			// If there's more materials then terraces, don't do anything.
			var materials = _renderer.sharedMaterials;
			var terraceCount = _relativeHeights.Length;
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
			var sculptingSettings = new SculptingSettings(_frequency, _heightCurve);
			Generate(sculptingSettings);
		}

		/// <summary>
		/// Generates and displays a terraced terrain synchronously, based on the given <paramref name="seed"/>.
		/// </summary>
		/// <param name="seed">The seed used to feed the randomizer. The same seed will always generate the same
		/// terrain.</param>
		public void GenerateTerrain(int seed)
		{
			var sculptingSettings = new SculptingSettings(seed, _frequency, _heightCurve);
			Generate(sculptingSettings);
		}
		
		/// <summary>
		/// Generates and displays a random terraced terrain asynchronously.
		/// </summary>
		/// <param name="token">Cancellation token that can be used to request task cancellation.</param>
		/// <returns>An awaitable Task that represents the terrain generation process.</returns>
		public async Task GenerateTerrainAsync(CancellationToken token)
		{
			var sculptingSettings = new SculptingSettings(_frequency, _heightCurve);
			await GenerateAsync(sculptingSettings, token);
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
			var sculptingSettings = new SculptingSettings(seed, _frequency, _heightCurve);
			await GenerateAsync(sculptingSettings, token);
		}

		#endregion

		#region Private

		/// <summary>
		/// Generates a terrain synchronously.
		/// </summary>
		/// <param name="sculptingSettings">The sculpting settings </param>
		private void Generate(SculptingSettings sculptingSettings)
		{
			// Generate
			var generator = new TerrainGenerator(_sides, _radius, _maximumHeight, _relativeHeights, sculptingSettings, 
				_depth);
			var previousMesh = _meshFilter.mesh;
			_meshFilter.mesh = generator.GenerateTerrain();
			// Cleanup
			if (previousMesh == null) return;
			previousMesh.Clear();
			Destroy(previousMesh);
		}
		
		private async Task GenerateAsync(SculptingSettings sculptingSettings, CancellationToken token)
		{
			// Generate
			var generator = new TerrainGenerator(_sides, _radius, _maximumHeight, _relativeHeights, sculptingSettings, 
				_depth);
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