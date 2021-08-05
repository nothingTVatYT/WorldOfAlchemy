using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GroundHelper))]
public class GroundHelperEditor : Editor
{
	public override void OnInspectorGUI() {
		DrawDefaultInspector();
		GroundHelper gh = target as GroundHelper;
		if (GUILayout.Button("Flatten terrain under object")) {
			gh.FlattenTerrain ();
		}
		if (GUILayout.Button("Restore terrain under object")) {
			gh.RestoreHeights ();
		}
	}
}
