using UnityEngine;

namespace LazySquirrelLabs.TerracedTerrainGenerator.Samples.Display
{
	internal class Rotator : MonoBehaviour
	{
		#region Serialized fields

		[SerializeField, Range(0.1f, 100f)] private float _speed;

		#endregion

		#region Fields

		private Transform _transform;
		private Vector3 _up;

		#endregion

		#region Setup

		private void Awake()
		{
			_transform = transform;
			_up = _transform.up;
		}

		#endregion

		#region Update

		private void Update()
		{
			transform.Rotate(_up, _speed * Time.deltaTime);
		}

		#endregion
	}
}