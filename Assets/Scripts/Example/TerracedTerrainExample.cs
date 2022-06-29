using UnityEngine;

namespace SneakySquirrelLabs.TerracedTerrainGenerator.Example
{
    public class TerracedTerrainExample : MonoBehaviour
    {
        #region Serialized fields

        [SerializeField, Range(3,10)] private ushort _sides;
        [SerializeField, Range(0, 10)] private ushort _depth;
        [SerializeField, Range(1,100)] private float _radius;
        [SerializeField, Range(0.1f, 10)] private float _height;
        [SerializeField, Range(0.01f, 1f)] private float _frequency;
        [SerializeField, Range(1, 50)] private uint _terraceCount;
        [SerializeField] private Material _material;

        #endregion
        
        #region Setup

        private void Awake()
        {
            Application.targetFrameRate = 30;
        }

        private void Start()
        {
            var origin = Vector3.zero;
            var generator = new TerrainGenerator(_sides, _radius, _height, _frequency, _depth, origin, _terraceCount);
            var meshRenderer = generator.GenerateTerrain();
            meshRenderer.material = _material;
        }

        #endregion
    }
}