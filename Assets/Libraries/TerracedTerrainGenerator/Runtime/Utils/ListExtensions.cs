using Unity.Collections;

namespace LazySquirrelLabs.TerracedTerrainGenerator.Utils
{
    internal static class ListExtensions
    {
        /// <summary>
        /// Combines two native lists, copying data from the provided lists into a new one.
        /// </summary>
        /// <param name="l1">The first list to be combined.</param>
        /// <param name="l2">The second list to be combined.</param>
        /// <param name="allocator">The allocation strategy to allocate the new native list.</param>
        /// <typeparam name="T">The type of the list' elements.</typeparam>
        /// <returns>A new native list, containing the elements of both lists.</returns>
        internal static NativeList<T> Combine<T>(this NativeList<T> l1, NativeList<T> l2, Allocator allocator) 
            where T : unmanaged
        {
            var length1 = l1.Length;
            var length2 = l2.Length;
            var combined = l1.Copy(length1 + length2, allocator);
            combined.AddRange(l2.AsArray());
            return combined;
        }
        
        /// <summary>
        /// Creates a copy of a <see cref="NativeList{T}"/>, respecting a given <paramref name="capacity"/>.
        /// </summary>
        /// <param name="list">The original list.</param>
        /// <param name="capacity">The initial capacity of the copy list. It's assumed to be at least the length of the
        /// original list, but it can also be greater.</param>
        /// <param name="allocator">The allocation strategy used when creating vertex and index buffers.</param>
        /// <typeparam name="T">The type of the elements of both the original <paramref name="list"/> and the copy
        /// list.</typeparam>
        /// <returns>A <see cref="NativeList{T}"/> containing all elements of the original list and potentially more
        /// elements.</returns>
        internal static NativeList<T> Copy<T>(this NativeList<T> list, int capacity, Allocator allocator) 
            where T : unmanaged
        {
            // Allocate the list.
            var newList = new NativeList<T>(capacity, allocator);
            // Copy data into the list.
            newList.AddRange(list.AsArray());
            // Clear (set value to default) trailing items by setting the list's length.
            newList.Length = capacity;
            return newList;
        }
    }
}