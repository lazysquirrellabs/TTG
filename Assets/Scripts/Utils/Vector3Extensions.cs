using UnityEngine;

namespace SneakySquirrelLabs.TerracedTerrainGenerator.Utils
{
    internal static class Vector3Extensions
    {
        #region Internal

        internal static Vector3 Rotate(this Vector3 point, float degrees)
        {
            var angle = degrees * Mathf.Deg2Rad;
            var cos = Mathf.Cos(angle);
            var sin = Mathf.Sin(angle);
            var newX = point.x * cos - point.z * sin;
            var newZ = point.x * sin + point.z * cos;
            return new Vector3(newX, point.y, newZ);
        }

        #endregion
    }
}