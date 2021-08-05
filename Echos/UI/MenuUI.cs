using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuUI : MonoBehaviour {

	[SerializeField] private Dropdown playerNames;
	[SerializeField] private GameObject playerNameInputPanel;
	[SerializeField] private Button playerNameCreateButton;
	[SerializeField] private Button startButton;
	private List<GameStates> games;
	private Regex playerNamePattern = new Regex("^[A-Z][-a-z]*");
	private bool aboutToStart = false;

	void Start() {
		if (GlobalGameState.gameState == null)
			Debug.LogError ("no global game state found");
		games = GlobalGameState.gameState.getAllSavedGames ();
		UpdatePlayerNames ();
		playerNameInputPanel.SetActive (games.Count == 0);
	}

	private void UpdatePlayerNames() {
		playerNames.ClearOptions ();
		List<string> names = new List<string> ();
		Debug.Log ("found " + games.Count + " saved games");
		foreach(GameStates p in games) {
			names.Add (p.playerData.name);
		}
		names.Add ("Create new Character");
		playerNames.AddOptions (names);
	}

	public void QuitGame() {
		Application.Quit ();
	}

	public void SetPlayer(int index) {
		Debug.Log ("item #" + index + " selected");
		if (index >= games.Count) {
			playerNameInputPanel.SetActive (true);
			playerNameCreateButton.interactable = false;
			startButton.interactable = false;
		} else
			startButton.interactable = true;
	}

	public void CheckName(string newName) {
		bool valid = false;
		if (!newName.Equals ("") && playerNamePattern.IsMatch (newName))
			valid = true;
		playerNameCreateButton.interactable = valid;
	}

	public void CreatePlayer(string newName) {
		if (!newName.Equals("")) {
			GameStates game = GlobalGameState.gameState.CreateNewDefaultGameStates (newName);
			games.Add (game);
			playerNameInputPanel.SetActive (false);
			playerNames.value = games.Count - 1;
			UpdatePlayerNames ();
			playerNames.RefreshShownValue ();
		}
	}

	public void CreatePlayer() {
		string newName = playerNameInputPanel.GetComponentInChildren<InputField> ().text;
		CreatePlayer (newName);
	}

	public void StartGame() {
		if (aboutToStart)
			return;
		aboutToStart = true;
		int selected = playerNames.value;
		GameStates selectedGame;
		if (selected >= games.Count) {
			SetPlayer (selected);
			return;
		}
		selectedGame = games [selected];
		Debug.Log ("starting game with " + selectedGame);
		GlobalGameState.gameState.StartGame (selectedGame);
	}
}
