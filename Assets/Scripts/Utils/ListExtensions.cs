using System.Collections.Generic;
using Unity.Collections;

namespace SneakySquirrelLabs.TerracedTerrainGenerator.Utils
{
    internal static class ListExtensions
    {
        internal static T[] Combine<T>(this List<T> l1, List<T> l2)
        {
            var lenght1 = l1.Count;
            var length2 = l2.Count;
            var combined = new T[lenght1 + length2];
            l1.CopyTo(combined, 0);
            l2.CopyTo(combined, lenght1);
            return combined;
        }

        internal static NativeArray<T> ToNativeNoAlloc<T>(this List<T> list, int capacity) where T : struct
        {
            var array = new NativeArray<T>(capacity, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            for (var i = 0; i < list.Count; i++)
                array[i] = list[i];
            return array;
        }
    }
}