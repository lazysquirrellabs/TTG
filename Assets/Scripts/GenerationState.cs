using System;
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
        /// Function used to generate the final mesh.
        /// </summary>
        private readonly Func<Mesh> _generateMesh;
        /// <summary>
        /// The mesh created as outcome of the generation process.
        /// </summary>
        private Mesh _mesh;

        #endregion

        #region Setup

        /// <summary>
        /// <see cref="GenerationState"/>'s constructor.
        /// </summary>
        /// <param name="generateMesh">Function used to generate the final mesh once the mesh data is generated.</param>
        internal GenerationState(Func<Mesh> generateMesh)
        {
            _semaphore = new SemaphoreSlim(0);
            _generateMesh = generateMesh;
        }

        #endregion

        #region Internal

        /// <summary>
        /// Creates the actual mesh data.
        /// </summary>
        internal void CreateMesh()
        {
            _mesh = _generateMesh();
            _semaphore.Release();
        }
        
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