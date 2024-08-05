using System;
using System.Threading;
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
				var token = _cancellationTokenSource.Token;
				{
					foreach (var setup in _setups)
					{
						await setup.WarmUpAsync(token);
						setup.Show();
					}
				}
			}
			catch (OperationCanceledException)
			{
				Debug.Log("Terrain switching stopped because the operation was cancelled.");
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