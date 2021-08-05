using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;

[CustomEditor(typeof(SubSceneManager))]
public class SubSceneManagerEditor : Editor
{
	private string pattern = "scene-.-.";
	private string sceneName = "scene-0-0";
	private SubSceneManager manager;

	public override void OnInspectorGUI ()
	{
		manager = target as SubSceneManager;
		DrawDefaultInspector ();
		EditorGUILayout.BeginHorizontal ();
		pattern = EditorGUILayout.TextField ("Scene name pattern:", pattern);
		if (GUILayout.Button("Scan build settings")) {
			Regex nameRegex = new Regex (pattern);
			for (int i = 0; i < SceneManager.sceneCount; i++) {
				Scene scene = SceneManager.GetSceneAt (i);
				if (nameRegex.IsMatch(scene.name)) {
					((SubSceneManager)target).addScene (scene);
				}
			}
		}
		EditorGUILayout.EndHorizontal ();

		EditorGUILayout.BeginHorizontal ();
		if (GUILayout.Button("Load all")) {
			loadAllInEditor ();
		}
		GUILayout.FlexibleSpace ();
		sceneName = EditorGUILayout.TextField ("Keep scene:", sceneName);
		if (GUILayout.Button("Unload others")) {
			unloadAllFromEditorBut (sceneName);
		}
		EditorGUILayout.EndHorizontal ();
	}

	public void loadAllInEditor() {
		foreach (SubSceneManager.SceneAndBounds sab in manager.sceneManagerSettings.scenes) {
			Scene scene = SceneManager.GetSceneByName (sab.name);
			//Debug.Log (string.Format ("check scene {0}: valid={1}, loaded={2}, path={3}", sab.name, scene.IsValid(), scene.isLoaded, scene.path));
			if (!scene.isLoaded) {
				string sceneAssetPath = string.Format ("Assets/Scenes/{0}.unity", sab.name);
				EditorSceneManager.OpenScene (sceneAssetPath, OpenSceneMode.Additive);
			}
		}
	}

	public void unloadAllFromEditorBut(string sceneToKeep) {
		SceneSetup[] scenes = EditorSceneManager.GetSceneManagerSetup ();
		List<string> subSceneNames = new List<string> ();
		foreach (SubSceneManager.SceneAndBounds sab in manager.sceneManagerSettings.scenes) {
			subSceneNames.Add (sab.name);
		}
		foreach (SceneSetup sceneSetup in scenes) {
			if (sceneSetup.isLoaded) {
				Scene sceneObject = SceneManager.GetSceneByPath (sceneSetup.path);
				if (subSceneNames.Contains (sceneObject.name) && !sceneObject.name.Equals(sceneToKeep)) {
					if (sceneObject.isDirty)
						EditorSceneManager.SaveScene (sceneObject);
					EditorSceneManager.CloseScene (sceneObject, false);
				}
			}
		}
	}

	public void GenerateLightmaps() {
		foreach (SubSceneManager.SceneAndBounds sab in manager.sceneManagerSettings.scenes) {
			Scene scene = SceneManager.GetSceneByName (sab.name);
			unloadAllFromEditorBut (sab.name);
			if (!scene.isLoaded) {
				string sceneAssetPath = string.Format ("Assets/Scenes/{0}.unity", sab.name);
				EditorSceneManager.OpenScene (sceneAssetPath, OpenSceneMode.Additive);
			}
			SceneManager.SetActiveScene (scene);
			RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Skybox;
			LightmapEditorSettings.enableAmbientOcclusion = true;
			LightmapEditorSettings.realtimeResolution = 0.1f;
			Lightmapping.Bake ();
		}
	}
}

