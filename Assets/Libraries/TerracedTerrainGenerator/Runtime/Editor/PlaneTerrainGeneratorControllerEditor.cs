using LazySquirrelLabs.TerracedTerrainGenerator.Controllers;
using UnityEditor;

namespace LazySquirrelLabs.TerracedTerrainGenerator.Editor
{
	[CustomEditor(typeof(PlaneTerrainGeneratorController))]
	internal class PlaneTerrainGeneratorControllerEditor : TerrainGeneratorControllerEditor
	{
		#region Fields

		// Properties
		private SerializedProperty _sides;
		private SerializedProperty _radius;

		// Tooltips
		private const string SidesTooltip = "The number of sides of the terrain's basic shape.";

		private const string RadiusTooltip = "The greatest distance between the center of the mesh and all of its " +
		                                     "vertices (ignoring their position's Y coordinate)";

		#endregion

		#region Setup

		protected override void OnEnable()
		{
			base.OnEnable();
			_sides = serializedObject.FindProperty("_sides");
			_radius = serializedObject.FindProperty("_radius");
		}

		#endregion

		#region Protected

		protected override void DrawGenerationSettings()
		{
			base.DrawGenerationSettings();
			DrawIntSlider(_sides, SidesTooltip, 3, 10);
			DrawFloatSlider(_radius, RadiusTooltip, 1f, 100f);
		}

		protected override void DrawHeightFields()
		{
			DrawSpacedHeader("Height");
			base.DrawHeightFields();
		}

		#endregion
	}
}