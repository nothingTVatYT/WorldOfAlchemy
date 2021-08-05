using UnityEngine;
using UnityEngine.UI;

public class QuestLogUI : MonoBehaviour {

	[SerializeField] GameObject questLogPanel;
	[SerializeField] Transform questList;
	[SerializeField] Text descriptionText;
	[SerializeField] AudioClip openSound;
	[SerializeField] AudioClip closeSound;

	const string PREFS_NAME = "QuestLogUI";
	WOAQuestState[] questStates;
	bool showFinished;
	BasePlayer player;
	AudioSource audioSource;

	int selectedItemIndex;

	void Start () {
		questStates = new WOAQuestState[0];
		audioSource = GetComponent<AudioSource> ();
		PanelPrefs.RestoreTransformPosition (PREFS_NAME, questLogPanel.transform);
		questLogPanel.SetActive (false);
		showFinished = false;
		player = GameSystem.gameSystem.player.GetComponent<BasePlayer> ();
		player.onQuestsChangedCallback += UpdateUI;
	}

	void Update() {
		if (Input.GetButtonDown("QuestLog")) {
			ToggleUI ();
		}
	}

	public void ToggleUI() {
		if (!questLogPanel.activeSelf)
			ShowQuestLogUI ();
		else
			HideQuestLogUI ();
	}

	void UpdateUI() {
		// save the selected quest
		string selectedQuestId = "";
		if (selectedItemIndex < questStates.Length)
			selectedQuestId = questStates [selectedItemIndex].id;
		questStates = showFinished ? player.questStates.ToArray () : player.questStates.FindAll (x => !x.finished).ToArray ();
		int currentItems = questList.childCount;
		// make sure there are enough items
		GameObject firstItem = questList.GetChild(0).gameObject;
		while (currentItems < questStates.Length) {
			Object.Instantiate (firstItem, questList);
			currentItems++;
		}
		// update texts
		for (int i = 0; i < questStates.Length; i++) {
			WOAQuest quest = player.GetQuest (questStates [i].id);
			if (quest == null)
				Debug.LogError ("there is a quest state without a quest for " + questStates [i].id);
			else {
				if (selectedQuestId == quest.id)
					selectedItemIndex = i;
				GameObject item = questList.GetChild (i).gameObject;
				item.SetActive (true);
				item.GetComponent<Text> ().text = quest.name;
			}
		}
		// disable superfluous items
		for (int i = questStates.Length; i < currentItems; i++) {
			questList.GetChild (i).gameObject.SetActive (false);
		}
		// update highLighting
		if (questStates.Length > 0) {
			Text t = questList.GetChild (selectedItemIndex).gameObject.GetComponent<Text> ();
			t.text = "<b>" + t.text + "</b>";
		}
		UpdateDescription ();
	}

	void UpdateDescription() {
		if (selectedItemIndex < questStates.Length) {
			WOAQuest quest = player.GetQuest (questStates [selectedItemIndex].id);
			descriptionText.text = "<b>" + quest.name + "</b>\n\n" + quest.QuestState ();
		} else
			descriptionText.text = "";
	}

	public void OnItemClicked(int index) {
		selectedItemIndex = index;
		UpdateUI ();
	}

	public void OnShowFinishedToggle(bool showFinished) {
		this.showFinished = showFinished;
		UpdateUI ();
	}

	public void HideQuestLogUI() {
		if (questLogPanel.activeSelf) {
			PanelPrefs.SaveTransformPosition (PREFS_NAME, questLogPanel.transform);
			questLogPanel.SetActive (false);
			if (audioSource != null && closeSound != null) {
				audioSource.PlayOneShot (closeSound);
			}
		}
	}

	public void ShowQuestLogUI() {
		if (!questLogPanel.activeSelf) {
			questLogPanel.SetActive (true);
			UpdateUI ();
			if (audioSource != null && openSound != null) {
				audioSource.PlayOneShot (openSound);
			}
		}
	}
}
