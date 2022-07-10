using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace SneakySquirrelLabs.TerracedTerrainGenerator
{
    /// <summary>
    /// State used to control asynchronous mesh generation.
    /// </summary>
    internal sealed class GenerationState
    {
        #region Fields

        /// <summary>
        /// Semaphore used to communicate generation completion.
        /// </summary>
        private readonly SemaphoreSlim _semaphore;
        /// <summary>
        /// The mesh created as outcome of the generation process.
        /// </summary>
        private Mesh _mesh;

        #endregion

        #region Properties

        /// <summary>
        /// <inheritdoc cref="_mesh"/>.
        /// </summary>
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

        /// <summary>
        /// Waits for the completion of the generation process, asynchronously.
        /// </summary>
        /// <param name="token">Token used for task cancellation.</param>
        /// <returns>A task that represents the waiting. Should be awaited to retrieved the generated
        /// <see cref="Mesh"/>.</returns>
        internal async Task<Mesh> WaitForCompletionAsync(CancellationToken token)
        {
            await _semaphore.WaitAsync(token);
            return _mesh;
        }

        #endregion
    }
}