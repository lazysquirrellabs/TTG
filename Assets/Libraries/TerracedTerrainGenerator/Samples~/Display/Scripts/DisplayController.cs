using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace LazySquirrelLabs.TerracedTerrainGenerator.Samples.Display
{
	internal class DisplayController : MonoBehaviour
	{
		#region Serialized fields

		[SerializeField, Range(1, 60)] private float _periodSeconds;
		[SerializeField] private TerrainSetup[] _setups;

		#endregion

		#region Fields

		private readonly CancellationTokenSource _cancellationTokenSource = new();

		#endregion

		#region Setup

		private async void Start()
		{
			try
			{
				var tasks = _setups.Select(WarmupAndShowSetup);
				await Task.WhenAll(tasks);
			}
			catch (OperationCanceledException)
			{
				Debug.Log("Terrain switching stopped because the operation was cancelled.");
			}

			return;

			async Task WarmupAndShowSetup(TerrainSetup setup)
			{
				var token = _cancellationTokenSource.Token;
				await setup.WarmUpAsync(token);
				token.ThrowIfCancellationRequested();
				setup.Show();
			}
		}

		private void OnDestroy()
		{
			_cancellationTokenSource?.Cancel();
			_cancellationTokenSource?.Dispose();
		}

		#endregion
	}
}