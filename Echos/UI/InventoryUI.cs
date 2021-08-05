using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour {

	[SerializeField] Transform slotsPanel;
	[SerializeField] Text moneyText;
	[SerializeField] GameObject inventoryUI;
	[SerializeField] AudioClip openSound;
	[SerializeField] AudioClip closeSound;
	[SerializeField] GameObject player;
	InventorySlotHandler[] slots;
	BaseCharacter character;
	AudioSource audioSource;
	const string PREFS_NAME = "InventoryUI";

	// Use this for initialization
	void Start () {
		character = player.GetComponent<BaseCharacter> ();
		character.onItemChangedCallback += UpdateUI;
		slots = slotsPanel.GetComponentsInChildren<InventorySlotHandler> ();
		audioSource = GetComponent<AudioSource> ();
		PanelPrefs.RestoreTransformPosition (PREFS_NAME, inventoryUI.transform);
		inventoryUI.SetActive (false);
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetButtonDown ("Inventory")) {
			ToggleUI ();
		}
	}

	public void ToggleUI() {
		if (!inventoryUI.activeSelf) {
			UpdateUI ();
			ShowInventoryUI ();
		} else {
			HideInventoryUI ();
		}
	}

	void UpdateUI() {
		for (int i = 0; i < slots.Length; i++) {
			if (i < character.inventoryCount) {
				slots [i].setItem (character.inventory(i));
			} else {
				slots [i].setItem (null);
			}
		}
		moneyText.text = character.money.ToString ();
	}

	public void HideInventoryUI() {
		if (inventoryUI.activeSelf) {
			PanelPrefs.SaveTransformPosition (PREFS_NAME, inventoryUI.transform);
			inventoryUI.SetActive (false);
			if (closeSound != null)
				audioSource.PlayOneShot (closeSound);
		}
	}

	public void ShowInventoryUI() {
		if (!inventoryUI.activeSelf) {
			inventoryUI.SetActive (true);
			if (openSound != null)
				audioSource.PlayOneShot (openSound);
		}
	}
}
