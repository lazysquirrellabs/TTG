using UnityEngine;

namespace LazySquirrelLabs.TerracedTerrainGenerator.Samples.Display
{
	internal class Rotator : MonoBehaviour
	{
		#region Serialized fields

		[SerializeField, Range(0.1f, 100f)] private float _speed;

		#endregion

		#region Update

		private void Update()
		{
			transform.Rotate(Vector3.up, _speed * Time.deltaTime);
		}

		#endregion
	}
}