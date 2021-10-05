using System;
using System.Collections.Generic;
using UnityEngine;

namespace SneakySquirrelLabs.TerracedTerrainGenerator.MeshFragmentation
{
    internal class MeshFragmenter
    {
        #region Fields

        private readonly Mesh _mesh;
        private readonly ushort _depth;

        #endregion

        #region Setup

        internal MeshFragmenter(Mesh mesh, ushort depth)
        {
            _mesh = mesh;
            _depth = depth;
        }

        #endregion

        #region Internal

        internal void Fragment()
        {
            if (_depth == 0)
                return;

            var vertices = _mesh.vertices;
            var triangles = _mesh.triangles;

            var initialTriangleCount = triangles.Length / 3;
            var newTriangleIxCount = GetTriangleIxCountForDepth(initialTriangleCount, _depth);

            var readTriangles = new int[newTriangleIxCount];
            var writeTriangles = new int[newTriangleIxCount];
            var readVertices = new Vector3[newTriangleIxCount / 2];
            var writeVertices = new Vector3[newTriangleIxCount / 2];
            Array.Copy(triangles, readTriangles, triangles.Length);
            Array.Copy(vertices, readVertices, vertices.Length);
            double currentDepthTotalTriangles = initialTriangleCount;

            for (var i = 1; i <= _depth; i++)
            {
                FragmentAllTriangles(currentDepthTotalTriangles);
                currentDepthTotalTriangles = GetTriangleIxCountForDepth(initialTriangleCount, i);
                (readTriangles, writeTriangles) = (writeTriangles, readTriangles);
                (readVertices, writeVertices) = (writeVertices, readVertices);
            }

            _mesh.vertices = readVertices;
            _mesh.triangles = readTriangles;

            static uint GetTriangleIxCountForDepth(int initialTriangleCount, int depth)
            {
                return (uint) (3 * (Math.Pow(4, depth) * initialTriangleCount));
            }

            void FragmentAllTriangles(double triangleIndexCount)
            {
                for (var i = 0; i < triangleIndexCount; i += 3)
                {
                    var writeTriangleIx = i * 4;
                    var writeVertexIx = i * 2;
                    FragmentTriangle(i, writeTriangleIx, readTriangles, writeTriangles, writeVertexIx, readVertices, writeVertices);
                }
            }
            
            static void FragmentTriangle(int readTriangleIx, int writeTriangleIx, IReadOnlyList<int> readTriangles, 
                int[] writeTriangles, int writeVertexIx, IReadOnlyList<Vector3> readVertices, Vector3[] writeVertices)
            {
                var indexVertex1 = readTriangles[readTriangleIx];
                var indexVertex2 = readTriangles[readTriangleIx+1];
                var indexVertex3 = readTriangles[readTriangleIx+2];
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