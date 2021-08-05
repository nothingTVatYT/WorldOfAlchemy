using System;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(RecordedLocations))]
public class RecordedLocationsEditor : Editor
{
	public override void OnInspectorGUI() {
		DrawDefaultInspector ();
		if (GUILayout.Button("Save selected")) {
			RecordedLocations rl = (RecordedLocations)target;
			foreach (RecordedLocations.NamedPosition pos in rl.locations) {
				if (pos.save) {
					String name = pos.name;
					// check if it already exists
					string path = "Assets/Scenes/" + name + ".asset";
					NamedLocation assetObject = AssetDatabase.LoadAssetAtPath<NamedLocation>(path);
					if (assetObject == null) {
						NamedLocation newNamedLocation = ScriptableObject.CreateInstance<NamedLocation> ();
						newNamedLocation.name = name;
						newNamedLocation.position = pos.position;
						newNamedLocation.rotation = pos.rotation;
						AssetDatabase.CreateAsset (newNamedLocation, path);
						pos.save = false;
					}
				}
			}
			AssetDatabase.SaveAssets ();
		}
	}
}

