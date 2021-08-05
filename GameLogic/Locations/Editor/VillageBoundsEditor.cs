using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(VillageBounds))]
public class VillageBoundsEditor : Editor {

	public override void OnInspectorGUI() {
		DrawDefaultInspector();
		VillageBounds vb = target as VillageBounds;
		if (!vb.autoUpdateBounds)
			if (GUILayout.Button("Update")) {
				vb.UpdateBounds ();
			}
	}
}
