using LazySquirrelLabs.TerracedTerrainGenerator.Controllers;
using UnityEditor;

namespace LazySquirrelLabs.TerracedTerrainGenerator.Editor
{
	[CustomEditor(typeof(SphereTerrainGeneratorController))]
	internal class SphereTerrainGeneratorControllerEditor : TerrainGeneratorControllerEditor
	{
		#region Fields

		// Properties
		private SerializedProperty _minimumHeight;
		
		// Tooltips
		private const string MinimumHeightHeightTooltip = "The minimum height of the generated terrain, in units.";

		#endregion

		#region Setup

		protected override void OnEnable()
		{
			base.OnEnable();
			_minimumHeight = serializedObject.FindProperty("_minimumHeight");
		}

		#endregion

		#region Protected

		protected override void DrawHeightFields()
		{
			DrawSpacedHeader("Height");
			DrawFloatSlider(_minimumHeight, MinimumHeightHeightTooltip, 1f, 100f);
			base.DrawHeightFields();
		}

		#endregion
	}
}