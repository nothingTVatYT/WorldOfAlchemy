using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PathRenderer))]
public class PathRendererEditor : Editor {

	public override void OnInspectorGUI() {
		DrawDefaultInspector ();
		if (GUILayout.Button("Create Path Objects")) {
			PathRenderer rl = (PathRenderer)target;
			foreach (RecordedLocations.NamedPosition pos in rl.recorder.locations) {
				GameObject newObject = Instantiate (rl.pathObjectTemplate, pos.position, Quaternion.Euler(pos.rotation));
				newObject.name = pos.name;
				newObject.transform.parent = rl.transform;
			}
		}
	}
}
