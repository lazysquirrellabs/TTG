using System;
using System.Threading;
using UnityEngine;

namespace LazySquirrelLabs.TerracedTerrainGenerator.Samples.ParametersTest
{
	internal class ParametersTest : MonoBehaviour
	{
		#region Serialized fields

		[SerializeField] private TerrainGeneratorController _generatorController;
		[SerializeField] private bool _async;

		#endregion

		#region Fields

		private const float Interval = 5f;
		private float _lastGeneration;
		private CancellationTokenSource _cancellationTokenSource;

		#endregion

		#region Setup

		private void Awake()
		{
			Application.targetFrameRate = 60;
			_cancellationTokenSource = new CancellationTokenSource();
		}

		private async void Start()
		{
			await _generatorController.GenerateTerrainAsync(_cancellationTokenSource.Token);
		}

		private void OnDestroy()
		{
			_cancellationTokenSource?.Cancel();
			_cancellationTokenSource?.Dispose();
		}

		#endregion

		#region Update

		private async void Update()
		{
			if (Time.realtimeSinceStartup - _lastGeneration < Interval)
				return;

			var startTime = Time.realtimeSinceStartup;
			_lastGeneration = startTime;
			string synchronicity;
			if (_async)
			{
				synchronicity = "asynchronously";
				try
				{
					await _generatorController.GenerateTerrainAsync(_cancellationTokenSource.Token);
				}
				catch (OperationCanceledException)
				{
					Debug.Log("Terrain generation stopped because the operation was cancelled.");
				}
			}
			else
			{
				synchronicity = "synchronously";
				_generatorController.GenerateTerrain();
			}
			var endTime = Time.realtimeSinceStartup;
			Debug.Log($"Generated terrain {synchronicity} in {(endTime - startTime) * 1_000} milliseconds.");
		}

		#endregion
	}
}
