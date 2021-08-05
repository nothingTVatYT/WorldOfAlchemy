using UnityEngine;

public class CommandPanelUI : MonoBehaviour {

	public GameObject commandPanel;
	bool initialized;
	SlotHandler[] commandSlots;

	// Use this for initialization
	void Start () {
		initialized = false;
		commandSlots = commandPanel.GetComponentsInChildren<SlotHandler> ();
	}
	
	// Update is called once per frame
	void Update () {
		if (!initialized) {
			if (GameSystem.gameSystem.playerIsLoaded) {
				updateCommandSlots (GameSystem.gameSystem.player.GetComponent<BasePlayer> ());
				initialized = true;
			}
		}
	}

	private void updateCommandSlots (BasePlayer playerData)
	{
		int commandIndex = 0;
		foreach (WOAItem w in playerData.commands) {
			if (commandSlots [commandIndex] != null) {
				commandSlots [commandIndex].setItem (w);
			}
			commandIndex++;
		}
	}
}
