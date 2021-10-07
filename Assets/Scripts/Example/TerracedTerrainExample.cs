using UnityEngine;

namespace SneakySquirrelLabs.TerracedTerrainGenerator.Example
{
    public class TerracedTerrainExample : MonoBehaviour
    {
        #region Serialized fields

        [SerializeField, Range(3,10)] private ushort _sides;
        [SerializeField, Range(0, 5)] private ushort _depth;
        [SerializeField, Range(1,100)] private float _radius;

        #endregion
        
        #region Setup

        private void Start()
        {
            var generator = new TerrainGenerator(_sides, _radius, _depth, Vector3.zero);
            generator.GenerateTerrain();
        }

        #endregion
    }
}