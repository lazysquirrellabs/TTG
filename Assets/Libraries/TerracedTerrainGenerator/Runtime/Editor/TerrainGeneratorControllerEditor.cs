using UnityEditor;
using UnityEngine;

namespace LazySquirrelLabs.TerracedTerrainGenerator.Editor
{
	internal abstract class TerrainGeneratorControllerEditor : UnityEditor.Editor
	{
		#region Fields

		// Properties
		private SerializedProperty _generateOnStart;
		private SerializedProperty _renderer;
		private SerializedProperty _meshFilter;
		private SerializedProperty _depth;
		private SerializedProperty _useCustomHeights;
		private SerializedProperty _relativeHeights;
		private SerializedProperty _maximumHeight;
		private SerializedProperty _frequency;
		private SerializedProperty _octaves;
		private SerializedProperty _persistence;
		private SerializedProperty _lacunarity;
		private SerializedProperty _heightCurve;

		// Tooltips
		private const string GenerateOnStartTooltip = "Should a new, random terrain be generated on start";
		private const string DepthTooltip = 
			"How many times the basic shape will be fragmented to form the terrain. The larger the value, the greater" +
			" the level of detail will be (more triangles and vertices) and the longer the generation process takes.";
		private const string HeightTooltip = "The maximum height of the generated terrain, in units.";
		private const string BaseFrequencyTooltip = 
			"The base (first octave's) degree of detail (hills and valleys) in a given area.";
		private const string OctavesTooltip = 
			"How many octaves (iterations) the sculpting will run. Each octave will run on a higher frequency and " +
			"lower relevance than the previous one. The higher the value, the more variation the terrain will " +
			"contain, but there is a point of diminishing returns due to persistence.";
		private const string PersistenceTooltip = 
			"Also called gain, it determines how much of an octave's amplitude will be carried to the next octave. " +
			"The lower the value, the quicker octave details \"disappear\" with each iteration.";
		private const string LacunarityTooltip =
			"How the frequency will be affected (multiplication factor) between octaves. In other words, how much" +
			" detail each octave will contain, when compared to the previous one.";
		private const string HeightCurveTooltip =
			"Height distribution over the terrain: how low valleys and how high hills should be and everything in " +
			"between. This curve must start in (0,0) and end in (1,1).";
		private const string UseCustomHeightsTooltip =
			"Whether custom terrace heights should be used. Toggle off to evenly space the therrace heights between" +
			"0 and the terrain's maximum height.";
		private const string RelativeHeightsTooltip =
			"Terrace heights, relative to the terrain's height. Values must be in the [0, 1] range, in ascending " +
			"order. Each terrace's final height will be calculated by multiplying the relative height by the " +
			"terrain's height.";
		private const string TerracesCountTooltip = "How many terraces the terrain will contain.";

		#endregion

		#region Setup

		protected virtual void OnEnable()
		{
			_generateOnStart = serializedObject.FindProperty("_generateOnStart");
			_renderer = serializedObject.FindProperty("_renderer");
			_meshFilter = serializedObject.FindProperty("_meshFilter");
			_depth = serializedObject.FindProperty("_depth");
			_useCustomHeights = serializedObject.FindProperty("_useCustomHeights");
			_relativeHeights = serializedObject.FindProperty("_relativeHeights");
			_maximumHeight = serializedObject.FindProperty("_maximumHeight");
			_frequency = serializedObject.FindProperty("_baseFrequency");
			_octaves = serializedObject.FindProperty("_octaves");
			_persistence = serializedObject.FindProperty("_persistence");
			_lacunarity = serializedObject.FindProperty("_lacunarity");
			_heightCurve = serializedObject.FindProperty("_heightCurve");
		}

		#endregion
		
		#region Update

		public override void OnInspectorGUI()
		{
			serializedObject.Update();
			DrawProperty(_generateOnStart, GenerateOnStartTooltip);
			DrawProperty(_renderer, string.Empty);
			DrawProperty(_meshFilter, string.Empty);
			DrawGenerationSettings();
			EditorGUILayout.Space();
			DrawHeightFields();
			DrawSpacedHeader("Sculpt settings");
			DrawFloatSlider(_frequency, BaseFrequencyTooltip, 0.01f, 1f);
			DrawIntSlider(_octaves, OctavesTooltip, 1, 10);
			if (_octaves.intValue > 1)
			{
				DrawFloatSlider(_persistence, PersistenceTooltip, 0.01f, 1f);
				DrawFloatSlider(_lacunarity, LacunarityTooltip, 1.1f, 20f);
			}

			DrawProperty(_heightCurve, HeightCurveTooltip);
			
			serializedObject.ApplyModifiedProperties();
		}

		#endregion

		#region Protected

		protected static void DrawSpacedHeader(string header)
		{
			EditorGUILayout.Space();
			EditorGUILayout.LabelField(header, EditorStyles.boldLabel);
		}
		
		protected static void DrawIntSlider(SerializedProperty property, string tooltip, int min, int max)
		{
			var content = new GUIContent(property.displayName, tooltip);
			property.intValue = EditorGUILayout.IntSlider(content, property.intValue, min, max);
		}
			
		protected static void DrawFloatSlider(SerializedProperty property, string tooltip, float min, float max)
		{
			var content = new GUIContent(property.displayName, tooltip);
			property.floatValue = EditorGUILayout.Slider(content, property.floatValue, min, max);
		}

		protected virtual void DrawHeightFields()
		{
			DrawFloatSlider(_maximumHeight, HeightTooltip, 0.1f, 100f);
			var useCustomHeights = _useCustomHeights.boolValue;
			DrawProperty(_useCustomHeights, UseCustomHeightsTooltip);
			var newUseCustomHeights = _useCustomHeights.boolValue;
			if (_useCustomHeights.boolValue)
			{
				DrawProperty(_relativeHeights, RelativeHeightsTooltip);
			}
			else
			{
				var length = _relativeHeights.arraySize;
				DrawArraySize(_relativeHeights, "Terrace count", TerracesCountTooltip, 1, 50);
				var newLength = _relativeHeights.arraySize;
				var justChanged = useCustomHeights != newUseCustomHeights;
				if (justChanged || length != newLength)
				{
					// Reset heights
					for (var i = 0; i < newLength; i++)
						_relativeHeights.GetArrayElementAtIndex(i).floatValue = (float)i / (newLength - 1);
				}
			}

			return;
			
			static void DrawArraySize(SerializedProperty property, string label, string tooltip, int min, int max)
			{
				var content = new GUIContent(label, tooltip);
				property.arraySize = EditorGUILayout.IntSlider(content, property.arraySize, min, max);
			}
		}

		protected virtual void DrawGenerationSettings()
		{
			DrawSpacedHeader("Generation settings");
			DrawIntSlider(_depth, DepthTooltip, 1, 10);
		}

		#endregion

		#region Private

		private static void DrawProperty(SerializedProperty property, string tooltip)
		{
			var content = new GUIContent(property.displayName, tooltip);
			EditorGUILayout.PropertyField(property, content);
		}

		#endregion
	}
}