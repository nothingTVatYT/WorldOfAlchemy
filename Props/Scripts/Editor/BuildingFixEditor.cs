using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(BuildingFix))]
public class BuildingFixEditor : Editor {

	public override void OnInspectorGUI() {
		DrawDefaultInspector ();
		if (GUILayout.Button("Install Doors")) {
			BuildingFix g = target as BuildingFix;
			g.InstallAllDoors ();
		}
		if (GUILayout.Button("Hide Collision Meshes")) {
			BuildingFix g = target as BuildingFix;
			g.HideCollisionMeshes ();
		}
	}
}
