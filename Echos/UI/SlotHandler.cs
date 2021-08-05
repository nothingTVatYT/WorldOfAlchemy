using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SlotHandler : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler,IPointerExitHandler, IDropHandler
{

	private int index = 0;
	private WOAItem item;
	private static Color emptySlotColor = new Color (1f, 1f, 1f, 0f);
	[SerializeField] Image iconImage;
	[SerializeField] Text indexLabel;
	[SerializeField] Progressbar stateProgressbar;
	[SerializeField] GameObject bubbleHelp;
	[SerializeField] Text bubbleHelpText;
	[SerializeField] AudioClip dockSound;
	private AudioSource audioSource;

	// Use this for initialization
	void Start ()
	{
		audioSource = GetComponent<AudioSource> ();
		stateProgressbar.gameObject.SetActive (false);
		RectTransform rect = bubbleHelp.GetComponent<RectTransform> ();
		rect.anchoredPosition = new Vector2 (0, GetComponent<RectTransform>().sizeDelta.y + 1);
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (index == 0) {
			index = transform.GetSiblingIndex () + 1;
			indexLabel.text = "" + index;
		}
		if (item != null) {
			if (Input.GetButtonDown ("Command" + index)) {
				item.use (GameSystem.gameSystem.player);
			}
			if (item is WOAItemWithCooldown) {
				WOAItemWithCooldown itemWithCooldown = (WOAItemWithCooldown)item;
				stateProgressbar.value = itemWithCooldown.coolingProgress;
			}
		}
	}

	public void setItem (WOAItem woaItem)
	{
		item = woaItem;
		if (item != null) {
			iconImage.sprite = item.icon;
			iconImage.color = Color.white;
			if (item is WOAItemWithCooldown) {
				stateProgressbar.gameObject.SetActive (true);
				stateProgressbar.value = 0;
			} else {
				stateProgressbar.setDetailedValues (0, 0);
				stateProgressbar.gameObject.SetActive (false);
			}
		} else {
			iconImage.sprite = null;
			iconImage.color = emptySlotColor;
			stateProgressbar.gameObject.SetActive (false);
		}
	}

	void updatePlayerData() {
		BasePlayer bp = GameSystem.gameSystem.player.GetComponent<BasePlayer> ();
		bp.setCommand (index, item);
	}

	public void OnPointerClick (PointerEventData eventData)
	{
		if (item != null) {
			Debug.Log (string.Format ("Clicked on {0}", index));
			item.use (GameSystem.gameSystem.player);
		}
	}

	public void OnDrop (PointerEventData eventData) {
		InventorySlotHandler inv = eventData.pointerDrag.GetComponent<InventorySlotHandler> ();
		if (inv != null) {
			WOAItem droppedItem = inv.getItem ();
			Debug.Log ("item  " + droppedItem.itemName + " has been dropped in the slot #" + index);
			setItem (droppedItem);
			updatePlayerData ();
			if (dockSound != null && audioSource != null)
				audioSource.PlayOneShot (dockSound);
		} else {
			SpellSlot spellSlot = eventData.pointerDrag.GetComponent<SpellSlot> ();
			if (spellSlot != null) {
				WOASpell droppedSpell = spellSlot.getSpell ();
				Debug.Log ("spell  " + droppedSpell.itemName + " has been dropped in the slot #" + index);
				setItem (droppedSpell);
				updatePlayerData ();
				if (dockSound != null && audioSource != null)
					audioSource.PlayOneShot (dockSound);
			}
		}
	}

	public void OnPointerEnter (PointerEventData eventData) {
		if (bubbleHelp != null && item != null) {
			bubbleHelpText.text = item.itemName;
			bubbleHelp.SetActive(true);
		}
	}

	public void OnPointerExit (PointerEventData eventData) {
		if (bubbleHelp != null) {
			bubbleHelp.SetActive(false);
		}
	}
}
