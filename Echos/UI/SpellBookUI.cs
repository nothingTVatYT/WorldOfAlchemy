using UnityEngine;

public class SpellBookUI : MonoBehaviour {

	[SerializeField] private GameObject player;
	[SerializeField] private GameObject spellBookPanel;
	[SerializeField] private GameObject spellSlots;
	[SerializeField] private GameObject spellSlotTemplate;
	[SerializeField] private AudioClip openSound;
	[SerializeField] private AudioClip closeSound;
	BaseCharacter baseCharacter;
	AudioSource audioSource;
	bool initialized;
	const string PREFS_NAME = "SpellBookUI";

	// Use this for initialization
	void Start () {
		initialized = false;
		audioSource = GetComponent<AudioSource> ();
		PanelPrefs.RestoreTransformPosition (PREFS_NAME, spellBookPanel.transform);
		spellBookPanel.SetActive (false);
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetButtonDown ("SpellBook")) {
			if (!spellBookPanel.activeSelf && !initialized) {
				if (player == null) {
					player = GameSystem.gameSystem.player;
				}
				baseCharacter = player.GetComponent<BaseCharacter> ();
				updateSpellUI ();
				baseCharacter.onSpellsChangedCallback += addToSpellUI;
				initialized = true;
			}
			ToggleUI ();
		}
	}

	public void ToggleUI() {
		if (spellBookPanel.activeSelf)
			HideSpellBookUI ();
		else
			ShowSpellBookUI ();
	}

	public void addToSpellUI(WOASpell newSpell) {
		updateSpellUI ();
	}

	public void updateSpellUI() {
		SpellSlot[] existingSlots = spellSlots.GetComponentsInChildren<SpellSlot> ();
		for (int i = 0; i < baseCharacter.spellsCount; i++) {
			SpellSlot spellSlot;
			if (existingSlots.Length < (i + 1)) {
				GameObject slotObject = Instantiate (spellSlotTemplate);
				slotObject.transform.SetParent (spellSlots.transform);
				spellSlot = slotObject.GetComponent<SpellSlot> ();
			} else
				spellSlot = existingSlots [i];
			spellSlot.setSpell(baseCharacter.Spell(i));
		}
		for (int i = existingSlots.Length - 1; i >= baseCharacter.spellsCount; i--) {
			Destroy (existingSlots [i].gameObject);
		}
	}

	public void HideSpellBookUI() {
		if (spellBookPanel.activeSelf) {
			PanelPrefs.SaveTransformPosition (PREFS_NAME, spellBookPanel.transform);
			spellBookPanel.SetActive (false);
			if (closeSound != null)
				audioSource.PlayOneShot (closeSound);
		}
	}

	public void ShowSpellBookUI() {
		if (!spellBookPanel.activeSelf) {
			spellBookPanel.SetActive (true);
			if (openSound != null)
				audioSource.PlayOneShot (openSound);
		}
	}
}
