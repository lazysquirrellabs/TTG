using UnityEditor;
using UnityEngine;

namespace SneakySquirrelLabs.TerracedTerrainGenerator.Editor
{
	[CustomEditor(typeof(TerrainGeneratorController))]
	internal class TerrainGeneratorControllerEditor : UnityEditor.Editor
	{
		#region Fields

		// Properties
		private SerializedProperty _generateOnStart;
		private SerializedProperty _renderer;
		private SerializedProperty _meshFilter;
		private SerializedProperty _sides;
		private SerializedProperty _radius;
		private SerializedProperty _depth;
		private SerializedProperty _useCustomHeights;
		private SerializedProperty _relativeHeights;
		private SerializedProperty _height;
		private SerializedProperty _frequency;
		private SerializedProperty _heightCurve;
		
		// Tooltips
		private const string GenerateOnStartTooltip = "Should a new, random terrain be generated on start";
		private const string SidesTooltip = "The number of sides of the terrain's basic shape.";
		private const string RadiusTooltip = "The greatest distance between the center of the mesh and all of its " +
		                                     "vertices (ignoring their position's Y coordinate)";
		private const string DepthTooltip = 
			"How many times the basic shape will be fragmented to form the terrain. The larger the value, the greater" +
			" the level of detail will be (more triangles and vertices) and the longer the generation process takes.";
		private const string HeightTooltip = "The maximum height of the generated terrain, in units.";
		private const string FrequencyTooltip = 
			"The degree of detail in the generated terrain (hills and valleys) in a given area.";
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

		private void OnEnable()
		{
			_generateOnStart = serializedObject.FindProperty("_generateOnStart");
			_renderer = serializedObject.FindProperty("_renderer");
			_meshFilter = serializedObject.FindProperty("_meshFilter");
			_sides = serializedObject.FindProperty("_sides");
			_radius = serializedObject.FindProperty("_radius");
			_depth = serializedObject.FindProperty("_depth");
			_useCustomHeights = serializedObject.FindProperty("_useCustomHeights");
			_relativeHeights = serializedObject.FindProperty("_relativeHeights");
			_height = serializedObject.FindProperty("_height");
			_frequency = serializedObject.FindProperty("_frequency");
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
			DrawSpacedHeader("Generation settings");
			DrawIntSlider(_sides, SidesTooltip, 3, 10);
			DrawFloatSlider(_radius, RadiusTooltip, 1f, 100f);
			DrawIntSlider(_depth, DepthTooltip, 0, 10);
			DrawSpacedHeader("Sculpting settings");
			DrawFloatSlider(_frequency, FrequencyTooltip, 0.01f, 1f);
			DrawProperty(_heightCurve, HeightCurveTooltip);
			EditorGUILayout.Space();
			DrawFloatSlider(_height, HeightTooltip, 0.1f, 100f);
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
			
			serializedObject.ApplyModifiedProperties();
			
			static void DrawProperty(SerializedProperty property, string tooltip)
			{
				var content = new GUIContent(property.displayName, tooltip);
				EditorGUILayout.PropertyField(property, content);
			}

			static void DrawIntSlider(SerializedProperty property, string tooltip, int min, int max)
			{
				var content = new GUIContent(property.displayName, tooltip);
				property.intValue = EditorGUILayout.IntSlider(content, property.intValue, min, max);
			}
			
			static void DrawFloatSlider(SerializedProperty property, string tooltip, float min, float max)
			{
				var content = new GUIContent(property.displayName, tooltip);
				property.floatValue = EditorGUILayout.Slider(content, property.floatValue, min, max);
			}

			static void DrawArraySize(SerializedProperty property, string label, string tooltip, int min, int max)
			{
				var content = new GUIContent(label, tooltip);
				property.arraySize = EditorGUILayout.IntSlider(content, property.arraySize, min, max);
			}
			
			static void DrawSpacedHeader(string header)
			{
				EditorGUILayout.Space();
				EditorGUILayout.LabelField(header, EditorStyles.boldLabel);
			}
		}

		#endregion
	}
}