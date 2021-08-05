using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Runtime.Serialization;
using System;
using System.Xml;

public class GlobalGameState : MonoBehaviour
{

	public static GlobalGameState gameState;
	private List<string> scenesToLoad;
	private bool loadPlayer = false;
	private List<GameStates> savedGames = new List<GameStates> ();
	private string path;
	private GameStates gameToLoad;

	public GameStates loadedGame { get { return gameToLoad; } }

	public GameObject basePlayerHelper;
	public List<BaseCharacterDefaults> allBaseCharacterDefaults = new List<BaseCharacterDefaults> ();

	AudioListener audioListener;
	List<AudioListener> audioListeners;

	void Awake ()
	{
		gameState = this;
		Refresh ();
	}

	void Start ()
	{
		audioListener = GetComponent<AudioListener>();
		if (audioListener == null) {
			audioListener = gameObject.AddComponent<AudioListener>();
			audioListener.enabled = false;
		}
		audioListeners = new List<AudioListener>();
		audioListeners.Add(audioListener);
		Scene menu = SceneManager.GetSceneByName ("menu");
		Scene playerScene = SceneManager.GetSceneByName ("player");
		Scene islandScene = SceneManager.GetSceneByName ("island");
		if (playerScene.isLoaded)
			SceneManager.UnloadSceneAsync (playerScene);
		if (islandScene.isLoaded)
			SceneManager.UnloadSceneAsync (islandScene);
		if (!menu.IsValid () || !menu.isLoaded) {
			audioListener.enabled = true;
			SceneManager.LoadSceneAsync ("menu", LoadSceneMode.Additive);
		}
	}

	public void Refresh() {
		path = Application.persistentDataPath + "/Characters";
		ReadAllPlayers ();
	}

	void ReadAllPlayers ()
	{
		if (!Directory.Exists (path))
			Directory.CreateDirectory (path);
		string[] fileNames = Directory.GetFiles (path, "*.bpl");

		List<GameStates> newList = new List<GameStates> ();
		foreach (string fileName in fileNames) {
			GameStates game = LoadGameStates (fileName);
			newList.Add (game);
		}
		savedGames = newList;
	}

	public void registerAudioListener(AudioListener listener)  {
		if (listener != null) {
			// switch off others
			foreach (AudioListener l in audioListeners) {
				if (l != null)
					l.enabled = false;
			}
			if (!audioListeners.Contains(listener))
				audioListeners.Add(listener);
		}
	}

	public void unregisterAudioListener(AudioListener listener) {
		audioListeners.Remove(listener);
		bool otherIsActive = false;
		foreach (AudioListener l in audioListeners) {
			if (l != null && l.enabled)
				otherIsActive = true;
		}
		if (!otherIsActive && audioListener != null)
			audioListener.enabled = true;
	}

	// Update is called once per frame
	void Update ()
	{
		if (scenesToLoad != null) {
			foreach (string sceneName in scenesToLoad) {
				Scene s = SceneManager.GetSceneByName (sceneName);
				if (!s.isLoaded && !sceneName.StartsWith("scene-")) {
					Debug.Log ("load " + sceneName);
					SceneManager.LoadScene (sceneName, LoadSceneMode.Additive);
				}
			}
			scenesToLoad = null;
		}
		if (loadPlayer) {
			Debug.Log ("unload menu");
			SceneManager.UnloadSceneAsync ("menu");
			Debug.Log ("load player");
			SceneManager.LoadScene ("player", LoadSceneMode.Additive);
			loadPlayer = false;
		}
	}

	public void StartGame (GameStates game)
	{
		loadPlayer = true;
		gameToLoad = game;
		scenesToLoad = new List<string>(game.scenes);
	}

	public void SaveGame(GameObject player) {
		GameStates gameStates = new GameStates();
		gameStates.playerLocation = player.transform.position;
		gameStates.playerRotation = player.transform.rotation.eulerAngles;
		List<string> scenesLoaded = new List<string> ();
		for (int i = 0; i < SceneManager.sceneCount; i++) {
			Scene s = SceneManager.GetSceneAt (i);
			if (s.isLoaded && !s.name.StartsWith ("Global") && !s.name.StartsWith ("player"))
				scenesLoaded.Add (s.name);
		}
		gameStates.scenes = scenesLoaded;
		BasePlayer bp = player.GetComponent<BasePlayer> ();
		gameStates.playerData = new BaseCharacterData(bp.volatileData);
		gameStates.playerData.templateName = bp.defaults.name;
		SaveGame (gameStates);
	}

	public void SaveGame (GameStates gameStates)
	{
		path = Application.persistentDataPath + "/Characters";
		if (!Directory.Exists (path))
			Directory.CreateDirectory (path);
		string saveGamePath = path + "/" + gameStates.playerData.name + ".bpl";

		if (File.Exists (saveGamePath))
			File.Delete (saveGamePath);

		FileStream file = new FileStream(saveGamePath, FileMode.CreateNew);
		DataContractSerializer serializer = new DataContractSerializer (typeof(GameStates));
		serializer.WriteObject (file, gameStates);

		file.Close ();
		Debug.Log ("game states have been saved to " + saveGamePath);
	}

	private GameStates LoadGameStates (string saveGamePath)
	{
		if (!File.Exists (saveGamePath)) {
			Debug.LogError ("Cannot load saved game - there is no file named " + saveGamePath);
			return null;
		}

		DataContractSerializer serializer = new DataContractSerializer (typeof(GameStates));
		FileStream file = new FileStream (saveGamePath, FileMode.Open);
		GameStates loaded = (GameStates)serializer.ReadObject (file);
		file.Close ();

		Debug.Log ("loaded: " + loaded);
		return loaded;
	}

	public GameStates CreateNewDefaultGameStates (string newName)
	{
		GameStates newGameStates = new GameStates ();
		newGameStates.scenes = new List<string>();
		newGameStates.scenes.Add("island");
		newGameStates.scenes.Add("scene-1-2");
		newGameStates.playerLocation = new Vector3 (-237.1f, 108.14f, 619.5f);
		newGameStates.playerRotation = new Vector3 (0, 0, 0);
		// temporarily add a player object
		BasePlayer newPlayer = basePlayerHelper.GetComponent<BasePlayer> ();
		if (newPlayer == null)
			newPlayer = basePlayerHelper.AddComponent<BasePlayer> ();
		newPlayer.defaults = allBaseCharacterDefaults [0];
		newPlayer.InitializeFromDefaults ();
		newPlayer.volatileData.name = newName;
		newPlayer.volatileData.templateName = newPlayer.defaults.name;
		newGameStates.playerData = new BaseCharacterData(newPlayer.volatileData);
		return newGameStates;
	}

	public void GotoMenu ()
	{
		List<string> names = new List<string> ();
		for (int i = 0; i < SceneManager.sceneCount; i++) {
			Scene s = SceneManager.GetSceneAt (i);
			if (s.isLoaded && !s.name.StartsWith ("Global"))
				names.Add (s.name);
		}
		foreach (string n in names)
			SceneManager.UnloadSceneAsync (n);
		SceneManager.LoadSceneAsync ("menu", LoadSceneMode.Additive);
		Refresh ();
	}

	public List<GameStates> getAllSavedGames ()
	{
		return savedGames;
	}

	public int GetBaseCharacterDefaultsId (BaseCharacterDefaults characterDefaults)
	{
		return allBaseCharacterDefaults.IndexOf (characterDefaults);
	}

	public BaseCharacterDefaults GetBaseCharacterDefaults (int i)
	{
		return allBaseCharacterDefaults [i];
	}

	public static UnityEngine.Object Resolve(string classname, string objectname) {
		Type classType = Type.GetType (classname);
		if (classType == null) {
			Debug.LogError ("cannot find a class named " + classname);
			return null;
		}
		UnityEngine.Object[] objects = Resources.FindObjectsOfTypeAll (classType);
		foreach (UnityEngine.Object o in objects) {
			if (objectname.Equals (o.name))
				return o;
		}
		return null;
	}
}
