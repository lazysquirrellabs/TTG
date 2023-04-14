using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace SneakySquirrelLabs.TerracedTerrainGenerator.Marketing
{
	internal class TerrainSwitcher : MonoBehaviour
	{
		#region Serialized fields

		[SerializeField, Range(1, 60)] private float _periodSeconds;
		[SerializeField] private TerrainSetup[] _setups;

		#endregion

		#region Fields

		private CancellationTokenSource _cancellationTokenSource;

		#endregion

		#region Setup

		private void Awake()
		{
			_cancellationTokenSource = new CancellationTokenSource();
		}

		private async void Start()
		{
			foreach (var setup in _setups)
				setup.WarmUp();

			var token = _cancellationTokenSource.Token;
			var periodMilli = (int)(_periodSeconds * 1_000);

			try
			{
				foreach (var setup in _setups)
				{
					setup.Show();
					await Task.Delay(periodMilli, token);
					token.ThrowIfCancellationRequested();
					setup.Hide();
				}
			}
			catch (OperationCanceledException)
			{
				Debug.Log($"Terrain switching stopped because the operation was cancelled.");
			}
		}

		private void OnDestroy()
		{
			_cancellationTokenSource?.Cancel();
		}

		#endregion
	}
}