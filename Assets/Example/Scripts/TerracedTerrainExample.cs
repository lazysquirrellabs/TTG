using System;
using System.Threading;
using System.Threading.Tasks;
using SneakySquirrelLabs.TerracedTerrainGenerator;
using UnityEngine;

public class TerracedTerrainExample : MonoBehaviour
{
    #region Serialized fields

    [SerializeField, Range(3,10)] private ushort _sides;
    [SerializeField, Range(0, 10)] private ushort _depth;
    [SerializeField, Range(1,100)] private float _radius;
    [SerializeField, Range(0.1f, 10)] private float _height;
    [SerializeField, Range(0.01f, 1f)] private float _frequency;
    [SerializeField, Range(1, 50)] private int _terraceCount;
    [SerializeField] private MeshFilter _meshFilter;
    [SerializeField] private AnimationCurve _heightCurve;
    [SerializeField] private bool _async;

    #endregion

    #region Fields

    private CancellationTokenSource _cancellationTokenSource;

    #endregion
    
    #region Setup

    private void Awake()
    {
        Application.targetFrameRate = 30;
        _cancellationTokenSource = new CancellationTokenSource();
    }

    private async void Start()
    {
        var generator = new TerrainGenerator(_sides, _radius, _height, _frequency, _depth, _terraceCount, _heightCurve);
        if (_async)
            await GenerateAsync(generator);
        else
            GenerateSynchronously(generator);

        void GenerateSynchronously(TerrainGenerator terrainGenerator)
        {
            _meshFilter.mesh = terrainGenerator.GenerateTerrain();
        }

        async Task GenerateAsync(TerrainGenerator terrainGenerator)
        {
            var token = _cancellationTokenSource.Token;

            try
            {
                _meshFilter.mesh = await terrainGenerator.GenerateTerrainAsync(token);
            }
            catch (OperationCanceledException)
            {
                Debug.Log("Terrain generation was cancelled.");
            }
        }
    }

    private void OnDestroy()
    {
        _cancellationTokenSource.Cancel();
    }

    #endregion
}
