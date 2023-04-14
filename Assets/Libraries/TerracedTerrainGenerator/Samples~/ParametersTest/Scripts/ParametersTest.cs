using System.Threading;
using UnityEngine;

namespace SneakySquirrelLabs.TerracedTerrainGenerator.Samples.ParametersTest
{
	public class ParametersTest : MonoBehaviour
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
				await _generatorController.GenerateTerrainAsync(_cancellationTokenSource.Token);
			}
			else
			{
				synchronicity = "synchronously";
				_generatorController.GenerateTerrain();
			}
			var endTime = Time.realtimeSinceStartup;
			_lastGeneration = endTime;
			Debug.Log($"Generated terrain {synchronicity} in {(endTime - startTime) * 1_000} milliseconds.");
		}

		#endregion
	}
}
