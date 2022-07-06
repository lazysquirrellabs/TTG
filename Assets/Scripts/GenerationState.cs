using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace SneakySquirrelLabs.TerracedTerrainGenerator
{
    internal sealed class GenerationState
    {
        #region Fields

        private readonly SemaphoreSlim _semaphore;
        private Mesh _mesh;

        #endregion

        #region Properties

        internal Mesh Mesh
        {
            set
            {
                _mesh = value;
                _semaphore.Release();
            }
        }

        #endregion

        #region Setup

        internal GenerationState()
        {
            _semaphore = new SemaphoreSlim(0);
        }

        #endregion

        #region Internal

        internal async Task<Mesh> WaitForCompletion(CancellationToken token)
        {
            await _semaphore.WaitAsync(token);
            return _mesh;
        }

        #endregion
    }
}