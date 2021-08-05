using System;
using UnityEngine;

public class NPCDialog : MonoBehaviour {

	[Serializable]
	public struct Paragraph {
		public string text;
		public string reply;
	}

	public bool inProgress { get { return dialogUI != null && dialogUI.gameObject.activeSelf; }}
	public Paragraph greeting;
	public WOAQuestID[] startsQuests;
	DialogUI dialogUI;
	BasePlayer basePlayer;
	BaseCharacter npc;
	int state;

	public void InitializeState(BasePlayer player) {
		basePlayer = player;
		state = 0;
		dialogUI = GameSystem.gameSystem.canvas.GetComponentInChildren<DialogUI> (true);
		if (!TryNextAvailableQuest ()) {
			dialogUI.ShowDialog (npc.characterName, greeting.text, greeting.reply, OptionClicked);
			//inProgress = true;
		}
	}

	public void OptionClicked() {
		state++;
		if (state >= 1) {
			dialogUI.CloseDialog ();
			//inProgress = false;
		}
	}

	bool TryNextAvailableQuest() {
		foreach (WOAQuestID questId in startsQuests) {
			Debug.Log("check " + questId.id + " for " + basePlayer.characterName);
			if (basePlayer.CanRunQuest (questId.id)) {
				//inProgress = true;
				Debug.Log ("running quest " + questId.id);
				basePlayer.RunQuest (questId.id, npc);
				return true;
			}
		}
		return false;
	}

	// Use this for initialization
	void Start () {
		npc = GetComponent<BaseCharacter> ();
		if (npc == null)
			Debug.LogError ("There is no BaseCharacter as a speaker.");
	}
}
