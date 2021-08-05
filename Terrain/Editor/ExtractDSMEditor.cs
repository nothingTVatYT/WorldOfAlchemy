using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(ExtractDSM))]
public class ExtractDSMEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

		ExtractDSM myScript = (ExtractDSM)target;

		if (GUILayout.Button("Crop"))
        {
			myScript.Crop();
		} else if (GUILayout.Button("Analyze")) {
			myScript.Analyze ();
		}
    }
}
