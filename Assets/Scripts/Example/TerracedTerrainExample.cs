using UnityEngine;

namespace SneakySquirrelLabs.TerracedTerrainGenerator.Example
{
    public class TerracedTerrainExample : MonoBehaviour
    {
        #region Serialized fields

        [SerializeField, Range(3,10)] private uint _sides;
        [SerializeField, Range(1,100)] private float _radius;

        #endregion
        
        #region Setup

        private void Start()
        {
            var generator = new TerrainGenerator(_sides, _radius, Vector3.zero);
            generator.GenerateTerrain();
        }

        #endregion
    }
}