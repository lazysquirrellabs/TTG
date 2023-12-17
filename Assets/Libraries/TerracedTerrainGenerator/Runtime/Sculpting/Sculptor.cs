using System;
using SneakySquirrelLabs.TerracedTerrainGenerator.Data;
using Unity.Collections;
using UnityEngine;
using Random = System.Random;

namespace SneakySquirrelLabs.TerracedTerrainGenerator.Sculpting
{
	/// <summary>
	/// Sculpts a terrain mesh using a planar Perlin filter. The sculpting is applied on the Y axis, upwards.
	/// </summary>
	internal class Sculptor : IDisposable
	{
		#region Fields

		/// <summary>
		/// The Y coordinate of the highest possible vertex after sculpting.
		/// </summary>
		private readonly float _maximumHeight;

		/// <summary>
		/// The settings used for sculpting.
		/// </summary>
		private readonly SculptSettings _settings;
		
		/// <summary>
		/// The random generator used to calculate noise offsets.
		/// </summary>
		private readonly Random _random;
		
		/// <summary>
		/// The octave offsets.
		/// </summary>
		private readonly NativeArray<Vector2> _offsets;
		
		/// <summary>
		/// The octave amplitudes.
		/// </summary>
		private readonly NativeArray<float> _amplitudes;
		
		/// <summary>
		/// The octave frequencies.
		/// </summary>
		private readonly NativeArray<float> _frequencies;

		#endregion

		#region Setup

		/// <summary>
		/// Creates a <see cref="Sculptor"/> with the given settings.
		/// </summary>
		/// <param name="sculptSettings">The settings used for sculpting.</param>
		/// <param name="maximumHeight"> The Y coordinate of the highest possible vertex after sculpting.</param>
		internal Sculptor(SculptSettings sculptSettings, float maximumHeight)
		{
			_settings = sculptSettings;
			_maximumHeight = maximumHeight;
			
			var octaves = (int)_settings.Octaves;
			// Cache the octaves data instead of calculating them on every Sculpt call to reduce memory allocations.
			_offsets = CreateArray<Vector2>(octaves);
			_amplitudes = CreateArray<float>(octaves);
			_frequencies = CreateArray<float>(octaves);
			_random = new Random(_settings.Seed);

			// Calculate the octaves data.
			var amplitude = 1f;
			var frequency = _settings.BaseFrequency;
			
			for (var i = 0; i < _settings.Octaves; i++)
			{
				amplitude *= _settings.Persistence;
				frequency *= _settings.Lacunarity;
				_amplitudes[i] = amplitude;
				_frequencies[i] = frequency;
			}
			
			static NativeArray<T> CreateArray<T>(int count) where T : struct
			{
				return new NativeArray<T>(count, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
			}
		}

		#endregion

		#region Public

		public void Dispose()
		{
			DisposeArray(_offsets);
			DisposeArray(_amplitudes);
			DisposeArray(_frequencies);

			static void DisposeArray<T>(NativeArray<T> array) where T : struct
			{
				array.Dispose();
			}
		}

		#endregion
		
		#region Internal

		/// <summary>
		/// Sculpts the given terrain mesh.
		/// </summary>
		/// <param name="meshData">The mesh data to be sculpted.</param>
		internal void Sculpt(SimpleMeshData meshData)
		{
			
			var offsets = _offsets;
			for (var i = 0; i < _settings.Octaves; i++)
			{
				var xOffset = _random.Next(-10_000, 10_000);
				var yOffset = _random.Next(-10_000, 10_000);
				offsets[i] = new Vector2(xOffset, yOffset);
			}
			
			var highestPointRelative = float.MinValue;
			// Actually apply the Perlin noise modifier.
			meshData.Map(CalculateVertexNoise);
			// At this point, the vertices' Y coordinate store noise data, which will be used to calculate the final 
			// heights. But first, we need to normalize the noise data, bringing all values to the [0,1] range. To do
			// so, we find by how much we need to multiply the largest noise value among the vertices in order to bring
			// it down to 1. This value will be used on the next step to normalize all vertices.
			var dropFactor = 1f / highestPointRelative;
			// Now that we've got noise data and a way to normalize it, we can actually apply the final height modifier,
			// bringing all vertices' Y coordinate to the [0, maximumHeight] range.
			meshData.Map(ApplyHeight);

			Vector3 CalculateVertexNoise(Vector3 vertex)
			{
				var height = GetNoise(vertex.x, vertex.z, _settings, _offsets, _amplitudes, _frequencies);
				if (height > highestPointRelative)
					highestPointRelative = height;
				vertex.y = height;
				return vertex;

				static float GetNoise(float x, float y, SculptSettings settings, NativeArray<Vector2> offsets, 
					NativeArray<float> amplitudes, NativeArray<float> frequencies)
				{
					float relativeHeight = 0;
					for (var i = 0; i < settings.Octaves; i++)
					{
						var frequency = frequencies[i];
						var amplitude = amplitudes[i];
						var offset = offsets[i];
						var filterX = x * frequency + offset.x;
						var filterY = y * frequency + offset.y;
						var noise = Mathf.PerlinNoise(filterX, filterY);
						relativeHeight += amplitude * noise;
					}

					return relativeHeight;
				}
			}

			Vector3 ApplyHeight(Vector3 vertex)
			{
				// Normalize the height data, so it's in the [0, 1] range.
				var relativeHeight = vertex.y * dropFactor;
				// Fetch the height distribution modifier
				var modifier = _settings.HeightDistribution.Evaluate(relativeHeight);
				vertex.y = _maximumHeight * relativeHeight * modifier;
				return vertex;
			}
		}
		
		#endregion
	}
}