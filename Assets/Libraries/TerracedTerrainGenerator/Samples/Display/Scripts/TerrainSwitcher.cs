using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace SneakySquirrelLabs.TerracedTerrainGenerator.Samples.Display
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
				var index = 0;
				while (true)
				{
					if (index >= _setups.Length)
						index = 0;
					var setup = _setups[index];
					setup.Show();
					await Task.Delay(periodMilli, token);
					token.ThrowIfCancellationRequested();
					setup.Hide();
					index++;
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
		}

		#endregion
	}
}