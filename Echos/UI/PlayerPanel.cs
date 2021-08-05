using UnityEngine;
using UnityEngine.UI;

public class PlayerPanel : MonoBehaviour {

	GameObject player;
	BasePlayer basePlayer;
	[SerializeField] Text playerName;
	[SerializeField] Progressbar healthBar;
	[SerializeField] GameObject skillPanel;
	[SerializeField] Text skillName;
	[SerializeField] Progressbar skillProgress;
	[SerializeField] float hideSkillProgressDelay = 7;
	[SerializeField] YesNoDialog yesNoDialog;
	float updateSkillTime;

	void Start () {
		player = GameSystem.gameSystem.player;
		basePlayer = (BasePlayer)BaseCharacter.getCharacterFromGameObject (player);
		basePlayer.onSkillsChangedCallback += UpdateSkill;
	}
	
	// Update is called once per frame
	void Update () {
		healthBar.value = (float)basePlayer.currentHP / basePlayer.effectiveMaxHP;
		healthBar.setDetailedValues(basePlayer.currentHP, basePlayer.effectiveMaxHP);
		playerName.text = basePlayer.characterName;
		if (skillPanel.activeSelf && Time.time > updateSkillTime + hideSkillProgressDelay)
			HideSkillPanel ();
		if (Input.GetButtonDown("Quit")) {
			Logout ();
		}
	}

	public void Logout() {
		yesNoDialog.ShowDialog ("Do you really want to log out?", GameSystem.gameSystem.Logout, () => {
		});
	}

	void HideSkillPanel() {
		skillName.text = "<none>";
		skillProgress.value = 0;
		skillPanel.SetActive (false);
	}

	void UpdateSkill(WOASkill skill) {
		if (skill == null) {
			HideSkillPanel ();
		} else {
			updateSkillTime = Time.time;
			skillPanel.SetActive (true);
			float currentLevel = skill.currentLevel;
			int intLevel = Mathf.FloorToInt (currentLevel);
			skillName.text = "(" + intLevel + ") " + skill.name;
			skillProgress.value = currentLevel - intLevel;
		}
	}
}
