using System.Collections.Generic;
using UnityEngine;

namespace SneakySquirrelLabs.TerracedTerrainGenerator.TerraceGeneration
{
    internal readonly struct Triangle
    {
        #region Properties

        internal Vector3 V1 { get; }
        internal Vector3 V2 { get; }
        internal Vector3 V3 { get; }

        #endregion

        #region Setup

        internal Triangle(IReadOnlyList<int> triangles, IReadOnlyList<Vector3> vertices, ref int index)
        {
            var ix1 = triangles[index];
            index++;
            var ix2 = triangles[index];
            index++;
            var ix3 = triangles[index];
            index++;

            var a = vertices[ix1];
            var b = vertices[ix2];
            var c = vertices[ix3];

            (V1, V2, V3) = SortPositions(a, b, c);

            static (Vector3, Vector3, Vector3) SortPositions(Vector3 a, Vector3 b, Vector3 c)
            {
                if (a.y < b.y)
                {
                    if (a.y < c.y)
                        return b.y < c.y ? (a, b, c) : (a, c, b);
                    return (c, a, b);
                }

                if (a.y > c.y)
                    return b.y > c.y ? (c, b, a) : (b, c, a);
                return (b, a, c);
            }
        }

        #endregion

        #region Public

        public override string ToString()
        {
            return $"({V1}, {V2}, {V3})";
        }

        #endregion

        #region Internal

        internal int GetPointsAbove(float y)
        {
            if (V1.y > y)
                return 3;
            
            if (V2.y < y)
                return V3.y < y ? 0 : 1;
            return V3.y < y ? 1 : 2;
        }

        #endregion
    }
}