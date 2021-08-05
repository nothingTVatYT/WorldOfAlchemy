using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SubSceneManager : MonoBehaviour {

	[System.Serializable]
	public struct SceneAndBounds {
		public string name;
		public Bounds bounds;
	}

	[System.Serializable]
	public struct SceneManagerSettings {
		public float loadScenesAtDistance;
		public float unloadScenesAtDistance;
		public float sceneTileSize;
		public int tileOffsetX;
		public int tileOffsetY;
		public List<SceneAndBounds> scenes;
	}

	class SceneData {
		public SceneAndBounds scene;
		public bool loaded;
		public bool loadPending;
		public bool unloadPending;
		public override string ToString() {
			return string.Format("{0} bounds={1}, loaded={2}, loadPending={3}, unloadPending={4}", scene.name, scene.bounds, loaded, loadPending, unloadPending);
		}
	}

	public SceneManagerSettings sceneManagerSettings;
	private List<SceneData> scenes = new List<SceneData> ();
	Regex nameIndexRegex = new Regex ("[^⁻+]-(\\d+)-(\\d+)");

	void Start () {
		foreach (SceneAndBounds sab in sceneManagerSettings.scenes) {
			SceneData sd = new SceneData ();
			sd.scene = sab;
			sd.loaded = SceneManager.GetSceneByName (sab.name).isLoaded;
			sd.loadPending = false;
			sd.unloadPending = false;
			scenes.Add (sd);
		}
	}
	
	void LateUpdate() {
		if (GameSystem.gameSystem == null || GameSystem.gameSystem.player == null)
			return;
		Vector3 playerPosition = GameSystem.gameSystem.player.transform.position;
		float loadDistSquared = sceneManagerSettings.loadScenesAtDistance * sceneManagerSettings.loadScenesAtDistance;
		float unloadDistSquared = sceneManagerSettings.unloadScenesAtDistance * sceneManagerSettings.unloadScenesAtDistance;
		for (int i = 0; i < scenes.Count; i++) {
			float distanceSquared = scenes[i].scene.bounds.SqrDistance (playerPosition);
			if (distanceSquared <= loadDistSquared)
				ensureSceneLoaded (scenes [i]);
			else if (distanceSquared >= unloadDistSquared) {
				mayUnloadScene (scenes [i]);
			}
		}
	}

	void ensureSceneLoaded(SceneData sceneData) {
		if (!sceneData.loaded && !sceneData.loadPending) {
			sceneData.loadPending = true;
			StartCoroutine(SceneLoader (sceneData));
		}
	}

	void mayUnloadScene(SceneData sceneData) {
		if (!sceneData.loaded || sceneData.unloadPending)
			return;
		sceneData.unloadPending = true;
		StartCoroutine (SceneUnloader (sceneData));
	}

	public void addScene(Scene scene) {
		SceneAndBounds sab = new SceneAndBounds ();
		sab.name = scene.name;
		Match m = nameIndexRegex.Match (scene.name);
		if (!m.Success) {
			Debug.LogError ("the scene name does not match the pattern: " + scene.name);
			return;
		}
		int tileX = int.Parse (m.Groups[1].Value);
		int tileY = int.Parse (m.Groups[2].Value);
		sab.bounds = new Bounds (new Vector3 (
			(tileX + sceneManagerSettings.tileOffsetX) * sceneManagerSettings.sceneTileSize+sceneManagerSettings.sceneTileSize/2,
			0,
			(tileY + sceneManagerSettings.tileOffsetY) * sceneManagerSettings.sceneTileSize + sceneManagerSettings.sceneTileSize/2),
			new Vector3 (sceneManagerSettings.sceneTileSize, 600, sceneManagerSettings.sceneTileSize));
		sceneManagerSettings.scenes.Add (sab);
	}

	IEnumerator SceneUnloader(SceneData sceneData) {
		AsyncOperation job = SceneManager.UnloadSceneAsync (sceneData.scene.name);
		while (!job.isDone)
			yield return null;

		sceneData.loaded = false;
		yield break;
	}

	IEnumerator SceneLoader(SceneData sceneData) {
		AsyncOperation job = SceneManager.LoadSceneAsync (sceneData.scene.name, LoadSceneMode.Additive);
		while (!job.isDone)
			yield return null;
		sceneData.loaded = true;
	}
}
