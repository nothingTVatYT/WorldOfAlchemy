using UnityEngine;
using UnityEngine.UI;

public class SkillsUI : MonoBehaviour {

	[SerializeField] GameObject skillsPanel;
	[SerializeField] Transform skillsList;
	[SerializeField] AudioClip openSound;
	[SerializeField] AudioClip closeSound;

	const string PREFS_NAME = "SkillsUI";
	BasePlayer player;
	AudioSource audioSource;
	WOASkill[] skills;

	void Start () {
		audioSource = GetComponent<AudioSource> ();
		PanelPrefs.RestoreTransformPosition (PREFS_NAME, skillsPanel.transform);
		skillsPanel.SetActive (false);
		player = GameSystem.gameSystem.player.GetComponent<BasePlayer> ();
		player.onSkillsChangedCallback += UpdateUI;
		skills = new WOASkill[0];
	}

	void Update() {
		if (Input.GetButtonDown("Skills")) {
			ToggleUI ();
		}
	}

	public void ToggleUI() {
		if (!skillsPanel.activeSelf)
			ShowSkillsUI ();
		else
			HideSkillsUI ();
	}

	void UpdateUI(WOASkill s) {
		UpdateUI ();
	}

	void UpdateUI() {
		skills = player.Skills ();
		int currentItems = skillsList.childCount;
		// make sure there are enough items
		GameObject firstItem = skillsList.GetChild(0).gameObject;
		while (currentItems < skills.Length) {
			Object.Instantiate (firstItem, skillsList);
			currentItems++;
		}
		// update texts
		for (int i = 0; i < skills.Length; i++) {
			WOASkill skill = skills [i];
			GameObject item = skillsList.GetChild (i).gameObject;
			item.SetActive (true);
			item.GetComponent<Text> ().text = string.Format ("{0} (Level {1})", skill.name, Mathf.FloorToInt(skill.currentLevel));
			item.GetComponentInChildren<Progressbar> ().value = skill.currentLevel - Mathf.FloorToInt (skill.currentLevel);
		}
		// disable superfluous items
		for (int i = skills.Length; i < currentItems; i++) {
			skillsList.GetChild (i).gameObject.SetActive (false);
		}
	}

	public void HideSkillsUI() {
		if (skillsPanel.activeSelf) {
			PanelPrefs.SaveTransformPosition (PREFS_NAME, skillsPanel.transform);
			skillsPanel.SetActive (false);
			if (audioSource != null && closeSound != null) {
				audioSource.PlayOneShot (closeSound);
			}
		}
	}

	public void ShowSkillsUI() {
		if (!skillsPanel.activeSelf) {
			skillsPanel.SetActive (true);
			UpdateUI ();
			if (audioSource != null && openSound != null) {
				audioSource.PlayOneShot (openSound);
			}
		}
	}
}
