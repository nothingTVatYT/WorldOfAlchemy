using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TargetPanel : MonoBehaviour {

	GameObject target;
	GameObject player;
	public GameObject targetPanel;
	Text targetName;
	Progressbar healthbar;
	public Toggle following;
	BasePlayer playerData;
	BaseCharacter targetCharacter;

	public void targetChanged() {
		target = playerData.target;
		if (target != null) {
			targetPanel.SetActive (true);
			targetCharacter = BaseCharacter.getCharacterFromGameObject (target);
			if (targetCharacter != null)
				targetName.text = targetCharacter.characterName;
			else
				targetName.text = target.name;
			following.isOn = playerData.IsFollowing ();
		} else {
			targetName.text = "";
			healthbar.setDetailedValues (0, 0);
			healthbar.value = 0;
			targetCharacter = null;
			following.isOn = false;
			StartCoroutine (DelayedClose (0.7f));
		}
	}

	public void SetAutoFollow(bool what) {
		playerData.FollowTarget (what);
	}

	// Use this for initialization
	void Start () {
		player = GameSystem.gameSystem.player;
		playerData = player.GetComponent<BasePlayer> ();
		targetName = targetPanel.GetComponentInChildren<Text> ();
		healthbar = targetPanel.GetComponentInChildren<Progressbar> ();
		targetPanel.SetActive (false);
		playerData.onTargetChangedCallback += targetChanged;
	}
	
	// Update is called once per frame
	void Update () {
		if (targetCharacter != null) {
			healthbar.value = (float)targetCharacter.currentHP / targetCharacter.effectiveMaxHP;
			healthbar.setDetailedValues (targetCharacter.currentHP, targetCharacter.effectiveMaxHP);
		}
	}

	IEnumerator DelayedClose(float delay) {
		yield return new WaitForSeconds (delay);
		if (target == null)
			targetPanel.SetActive (false);
	}
}
