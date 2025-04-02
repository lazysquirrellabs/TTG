using System;
using Unity.Collections;
using UnityEngine;

namespace LazySquirrelLabs.TerracedTerrainGenerator.Data
{
	/// <summary>
	/// Represents all the data necessary to build a simple mesh - a mesh with just 1 sub mesh.
	/// </summary>
	internal sealed class SimpleMeshData : MeshData
	{
		#region Fields

		/// <summary>
		/// The index of the next vertex.
		/// </summary>
		private int _nextVertexIndex;

		/// <summary>
		/// All the vertices in the mesh data.
		/// </summary>
		private NativeArray<Vector3> _vertices;

		#endregion

		#region Properties

		/// <summary>
		/// All the vertices in the mesh data.
		/// </summary>
		internal NativeArray<Vector3> Vertices => _vertices;

		/// <summary>
		/// All the (triangle) indices in the mesh data.
		/// </summary>
		internal NativeList<int> Indices => IndicesPerSubMesh[0];

		#endregion

		#region Setup

		/// <summary>
		/// Creates mesh data for a simple mesh with the provided mesh data.
		/// </summary>
		/// <param name="vertices">The initial mesh vertices.</param>
		/// <param name="indices">The initial mesh (triangle) indices.</param>
		/// <param name="allocator">The allocation strategy used when creating vertex and index buffers.</param>
		internal SimpleMeshData(NativeArray<Vector3> vertices, NativeList<int> indices, Allocator allocator)
		{
			_vertices = vertices;
			// A simple mesh on has 1 sub mesh
			IndicesPerSubMesh = new NativeArray<NativeList<int>>(1, allocator);
			IndicesPerSubMesh[0] = indices;
		}

		/// <summary>
		/// Creates mesh data for a simple mesh, allocating space for the provided vertex and indices amounts.
		/// </summary>
		/// <param name="vertexCount">The initial amount of mesh vertices.</param>
		/// <param name="indicesCount">The initial amount of mesh (triangle) indices.</param>
		/// <param name="allocator">The allocation strategy used when creating vertex and index buffers.</param>
		internal SimpleMeshData(int vertexCount, int indicesCount, Allocator allocator)
		{
			_vertices = new NativeArray<Vector3>(vertexCount, allocator, NativeArrayOptions.UninitializedMemory);
			// A simple mesh on has 1 sub mesh
			IndicesPerSubMesh = new NativeArray<NativeList<int>>(1, allocator);
			IndicesPerSubMesh[0] = new NativeList<int>(indicesCount, allocator);
		}

		#endregion

		#region Public

		public override void Dispose()
		{
			base.Dispose();
			_vertices.Dispose();
		}

		#endregion

		#region Internal

		/// <summary>
		/// Maps a function <paramref name="f"/> to all vertices of the mesh.
		/// </summary>
		/// <param name="f">The function to be applied on all vertices.</param>
		internal void Map(Func<Vector3, Vector3> f)
		{
			for (var i = 0; i < _vertices.Length; i++)
			{
				var vertex = Vertices[i];
				vertex = f(vertex);
				_vertices[i] = vertex;
			}
		}

		/// <summary>
		/// Adds a triangle to the mesh. Points are provided in clockwise order as seen from the rendered surface.
		/// </summary>
		/// <param name="v1">The first point of the triangle.</param>
		/// <param name="v2">The second point of the triangle.</param>
		/// <param name="v3">The third point of the triangle.</param>
		internal void AddTriangle(Vector3 v1, Vector3 v2, Vector3 v3)
		{
			AddTriangleAt(v1, v2, v3, 0, ref _nextVertexIndex);
		}

		/// <summary>
		/// Adds a quadrilateral to the mesh. Points are provided in clockwise order as seen from the rendered surface.
		/// </summary>
		/// <param name="v1">The bottom left corner of the quadrilateral.</param>
		/// <param name="v2">The top left corner of the quadrilateral.</param>
		/// <param name="v3">The top right corner of the quadrilateral.</param>
		/// <param name="v4">The bottom right corner of the quadrilateral.</param>
		internal void AddQuadrilateral(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4)
		{
			AddQuadrilateralAt(v1, v2, v3, v4, 0, ref _nextVertexIndex);
		}

		#endregion

		#region Protected

		protected override int AddVertex(Vector3 vertex, ref int index)
		{
			_vertices[index] = vertex;
			var newIndex = index;
			index++;
			return newIndex;
		}

		#endregion
	}
}