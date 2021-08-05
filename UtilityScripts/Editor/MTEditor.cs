using System;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MeshTransformer))]
public class MTEditor : Editor
{
	public override void OnInspectorGUI() {
		if (GUILayout.Button("Apply rotation to mesh")) {
			((MeshTransformer)target).ApplyRotation ();
		}
	}
}

