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
        internal static T[] Combine<T>(this NativeList<T> l1, NativeList<T> l2) where T : unmanaged
        {
            var lenght1 = l1.Length;
            var length2 = l2.Length;
            var combined = new T[lenght1 + length2];
            CopyTo(l1, combined, 0);
            CopyTo(l2, combined, l1.Length);
            return combined;

            void CopyTo(NativeList<T> source, IList<T> destination, int offset)
            {
                for (var i = offset; i < source.Length + offset; i++)
                    destination[i] = source[i - offset];
            }
        }
        /// <summary>
        /// Creates a copy of a <see cref="NativeList{T}"/>, respecting a given <paramref name="capacity"/>.
        /// </summary>
        /// <param name="list">The original list.</param>
        /// <param name="capacity">The initial capacity of the copy list. It's assumed to be at least the length of the
        /// original list, but it can also be greater.</param>
        /// <typeparam name="T">The type of the elements of both the original <paramref name="list"/> and the copy
        /// list.</typeparam>
        /// <returns>A <see cref="NativeList{T}"/> containing all elements of the original list and potentially more
        /// elements.</returns>
        internal static NativeList<T> Copy<T>(this NativeList<T> list, int capacity) where T : unmanaged
        {
            // Allocate the list.
            var newList = new NativeList<T>(capacity, Allocator.TempJob);
            // Copy data into the list.
            foreach (var t in list)
                newList.Add(t);
            // Add remaining empty items
            for (var i = list.Length; i < capacity; i++)
                newList.Add(default);
            return newList;
        }
    }
}