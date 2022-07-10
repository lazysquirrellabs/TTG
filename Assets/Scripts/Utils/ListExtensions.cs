using System.Collections.Generic;
using Unity.Collections;

namespace SneakySquirrelLabs.TerracedTerrainGenerator.Utils
{
    internal static class ListExtensions
    {
        /// <summary>
        /// Combines two lists into a new array, copying data from the provided lists into the array.
        /// </summary>
        /// <param name="l1">The first list to be combined.</param>
        /// <param name="l2">The second list to be combined.</param>
        /// <typeparam name="T">The type of the list' elements.</typeparam>
        /// <returns>A new array, containing the elements of both lists.</returns>
        internal static T[] Combine<T>(this List<T> l1, List<T> l2)
        {
            var lenght1 = l1.Count;
            var length2 = l2.Count;
            var combined = new T[lenght1 + length2];
            l1.CopyTo(combined, 0);
            l2.CopyTo(combined, lenght1);
            return combined;
        }

        /// <summary>
        /// Creates a <see cref="NativeArray{T}"/> containing all elements of the given <paramref name="list"/>. 
        /// </summary>
        /// <param name="list">The list whose elements will be copied into the array.</param>
        /// <param name="capacity">The initial capacity of the native array. It's assumed to be at least the length
        /// of the list, but it can also be greater. In that case, the remaining elements will be left
        /// uninitialized.</param>
        /// <typeparam name="T">The type of the elements of both the provided <paramref name="list"/> and the returned
        /// array.</typeparam>
        /// <returns>A <see cref="NativeArray{T}"/> containing all elements of the provided list and potentially some
        /// uninitialized elements.</returns>
        internal static NativeArray<T> ToNativeNoAlloc<T>(this List<T> list, int capacity) where T : struct
        {
            // Allocate the array
            var array = new NativeArray<T>(capacity, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            // copy data into the array.
            for (var i = 0; i < list.Count; i++)
                array[i] = list[i];
            return array;
        }
    }
}