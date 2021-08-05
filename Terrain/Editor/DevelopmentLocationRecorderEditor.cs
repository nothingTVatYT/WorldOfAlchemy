using System;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(DevelopmentLocationRecorder))]
public class DevelopmentLocationRecorderEditor : Editor
{
	public override void OnInspectorGUI()
	{
		DrawDefaultInspector ();

		if (GUILayout.Button("Teleport to last location")) {
			((DevelopmentLocationRecorder)target).teleportToLastPosition ();
		}
	}
}

