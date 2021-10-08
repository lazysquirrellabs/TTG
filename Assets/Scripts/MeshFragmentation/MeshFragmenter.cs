using System;
using System.Collections.Generic;
using UnityEngine;

namespace SneakySquirrelLabs.TerracedTerrainGenerator.MeshFragmentation
{
    /// <summary>
    /// Fragments a mesh into sub-triangles given an arbitrary depth. The original mesh is modified.
    /// </summary>
    internal class MeshFragmenter
    {
        #region Fields

        /// <summary>
        /// The mesh to be fragmented.
        /// </summary>
        private readonly Mesh _mesh;
        /// <summary>
        /// The depth (how many consecutive times) the mesh will be fragmented.
        /// </summary>
        private readonly ushort _depth;

        #endregion

        #region Setup

        /// <summary>
        /// <see cref="MeshFragmenter"/>'s constructor. To actually fragment a mesh, call <see cref="Fragment"/>.
        /// </summary>
        /// <param name="mesh">The mesh to be fragmented.</param>
        /// <param name="depth">The depth (how many consecutive times) the mesh will be fragmented.</param>
        internal MeshFragmenter(Mesh mesh, ushort depth)
        {
            _mesh = mesh;
            _depth = depth;
        }

        #endregion

        #region Internal

        /// <summary>
        /// Actually fragments the mesh. It modifies the original mesh instead of returning a new one.
        /// <param name="recalculateNormals">Whether the vertices' normals should be recalculated after
        /// fragmentation.</param>
        /// </summary>
        internal void Fragment(bool recalculateNormals)
        {
            if (_depth == 0)
                return;

            var vertices = _mesh.vertices;
            var triangles = _mesh.triangles;

            // The Mesh's triangles field contains indexes of the triangles' vertices. So to find the number of
            // triangles, we just divide it by 3
            var initialTriangleCount = triangles.Length / 3;
            // Find the number of triangles in the final mesh (at maximum depth)
            var finalTriangleCount = GetTriangleCountForDepth(initialTriangleCount, _depth);
            var finalTriangleIxCount = finalTriangleCount * 3;
            
            // Instead of creating a new array for each depth, we use the same 2 arrays everywhere: 1 for reading and
            // one for writing. Both have exactly the number of elements necessary for the final depth.
            // Every time we step into a new depth, we swap them to read from the last depth's write array.
            var readTriangles = new int[finalTriangleIxCount];
            var writeTriangles = new int[finalTriangleIxCount];
            var readVertices = new Vector3[finalTriangleIxCount / 2];
            var writeVertices = new Vector3[finalTriangleIxCount / 2];
            Array.Copy(triangles, readTriangles, triangles.Length);
            Array.Copy(vertices, readVertices, vertices.Length);
            double currentDepthTotalTriangles = initialTriangleCount;

            for (var i = 1; i <= _depth; i++)
            {
                FragmentAllTriangles(currentDepthTotalTriangles);
                currentDepthTotalTriangles = GetTriangleCountForDepth(initialTriangleCount, i);
                (readTriangles, writeTriangles) = (writeTriangles, readTriangles);
                (readVertices, writeVertices) = (writeVertices, readVertices);
            }

            _mesh.SetVertices(readVertices);
            _mesh.SetTriangles(readTriangles, 0, false);
            if (recalculateNormals)
                _mesh.RecalculateNormals();
            
            static uint GetTriangleCountForDepth(int initialTriangleCount, int depth)
            {
                return (uint) (Math.Pow(4, depth) * initialTriangleCount);
            }

            void FragmentAllTriangles(double triangleCount)
            {
                for (var i = 0; i < triangleCount; i++)
                    FragmentTriangle(i, readTriangles, writeTriangles, readVertices, writeVertices);
            }

            static void FragmentTriangle(int triangleIx, IReadOnlyList<int> readTriangles, int[] writeTriangles, 
                IReadOnlyList<Vector3> readVertices, Vector3[] writeVertices)
            {
                // Each original triangle has 3 vertices, so we need to offset that from reading
                var readTriangleIx = triangleIx * 3;
                // The fragmented triangle will have 4 triangles, each one with 3 vertices, so we need to offset by 12
                var writeTriangleIx = triangleIx * 12;
                // The fragmented triangle will have 6 vertices, so we need to offset by 6
                var writeVertexIx = triangleIx * 6;
                
                // Read the original vertex data
                var indexVertex1 = readTriangles[readTriangleIx];
                var indexVertex2 = readTriangles[readTriangleIx+1];
                var indexVertex3 = readTriangles[readTriangleIx+2];
                
                // Calculate the new vertices and add them
                var v1 = readVertices[indexVertex1];
                var v2 = readVertices[indexVertex2];
                var v3 = readVertices[indexVertex3];
                var v4 = (v1 + v2) / 2;
                var v5 = (v2 + v3) / 2;
                var v6 = (v3 + v1) / 2;

                var ix1 = AddVertex(v1);
                var ix2 = AddVertex(v2);
                var ix3 = AddVertex(v3);
                var ix4 = AddVertex(v4);
                var ix5 = AddVertex(v5);
                var ix6 = AddVertex(v6);
                
                // Add the new triangles using the indices of the new vertices
                AddTriangle(ix1, ix4, ix6);
                AddTriangle(ix4, ix5, ix6);
                AddTriangle(ix5, ix3, ix6);
                AddTriangle(ix4, ix2, ix5);

                int AddVertex(Vector3 v)
                {
                    writeVertices[writeVertexIx] = v;
                    var index = writeVertexIx;
                    writeVertexIx++;
                    return index;
                }

                void AddTriangle(int i1, int i2, int i3)
                {
                    writeTriangles[writeTriangleIx] = i1;
                    writeTriangles[writeTriangleIx+1] = i2;
                    writeTriangles[writeTriangleIx+2] = i3;
                    writeTriangleIx += 3;
                }
            }
        }

        #endregion
    }
}