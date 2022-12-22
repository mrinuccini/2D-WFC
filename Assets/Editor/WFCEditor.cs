using UnityEditor;
using UnityEngine;

namespace WFC
{
	[CustomEditor(typeof(WaveFunctionCollapse))]
	public class WFCEditor : UnityEditor.Editor
	{
		public override void OnInspectorGUI()
		{
			WaveFunctionCollapse wfc = (WaveFunctionCollapse)target;

			if (GUILayout.Button("Generate Wave Function Collapse"))
			{
				wfc.DoMagic();
			}

			if (GUILayout.Button("Clear"))
			{
				wfc.Clear();
			}

			base.OnInspectorGUI();
		}
	}
}
