using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace LazySquirrelLabs.TerracedTerrainGenerator.Samples.Display
{
	internal class TerrainSwitcher : MonoBehaviour
	{
		#region Serialized fields

		[SerializeField] private bool _warmUpOnStart;
		[SerializeField, Range(1, 60)] private float _periodSeconds;
		[SerializeField] private TerrainSetup[] _setups;

		#endregion

		#region Fields

		private readonly CancellationTokenSource _cancellationTokenSource = new();

		#endregion

		#region Setup

		private async void Start()
		{
			var token = _cancellationTokenSource.Token;
			var warmedUp = false;

			if (_warmUpOnStart)
			{
				foreach (var setup in _setups)
				{
					await setup.WarmUpAsync(token);
				}

				warmedUp = true;
			}
			else
			{
				await _setups[0].WarmUpAsync(token);
			}

			var periodMilli = (int)(_periodSeconds * 1_000);

			try
			{
				var index = 0;

				while (true)
				{
					if (index >= _setups.Length) // Loop
					{
						index = 0;
						warmedUp = true;
					}

					var currentSetup = _setups[index];
					currentSetup.Show();
					var nextSetupIndex = index == _setups.Length - 1 ? 0 : index + 1;
					var delayTask = Task.Delay(periodMilli, token);

					if (warmedUp)
					{
						await delayTask;
					}
					else
					{
						var warmUpTask = _setups[nextSetupIndex].WarmUpAsync(token);
						await Task.WhenAll(delayTask, warmUpTask);
					}

					token.ThrowIfCancellationRequested();
					currentSetup.Hide();
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
			_cancellationTokenSource?.Dispose();
		}

		#endregion
	}
}