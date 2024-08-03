using System;
using LazySquirrelLabs.TerracedTerrainGenerator.Controllers;
using UnityEngine;

namespace LazySquirrelLabs.TerracedTerrainGenerator.Samples.Display
{
	[Serializable]
	internal struct TerrainSetup
	{
		#region Serialized fields

		[SerializeField] private TerrainGeneratorController _controller;
		[SerializeField] private int _seed;
		[SerializeField] private Vector3 _lightRotation;

		#endregion

		#region Internal

		internal void WarmUp()
		{
			_controller.GenerateTerrain(_seed);
		}

		internal void Show(Transform lightTransform)
		{
			lightTransform.eulerAngles = _lightRotation;
			_controller.gameObject.SetActive(true);
		}

		internal void Hide()
		{
			_controller.gameObject.SetActive(false);
		}

		#endregion
	}
}