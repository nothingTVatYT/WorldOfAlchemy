using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InventorySlotHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler {

	private GameObject dragItem;
	private Vector3 initialDragPosition;
	private WOAItem item;
	public Image icon;
	public Text countText;
	private InventoryPanel inventoryPanel;
	private Text itemDescription;

	// Use this for initialization
	void Start () {
		inventoryPanel = transform.parent.parent.GetComponent<InventoryPanel> ();
		itemDescription = inventoryPanel.itemDescription;
		dragItem = inventoryPanel.dragItem;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void setItem(WOAItem newItem) {
		item = newItem;
		if (item != null) {
			icon.sprite = item.icon;
			icon.gameObject.SetActive (true);
			WOASubstance substance = newItem as WOASubstance;
			if (substance != null)
				countText.text = "" + substance.currentStackSize;
			else
				countText.text = "";
			icon.GetComponent<Button> ().interactable = true;
		} else {
			countText.text = "";
			icon.gameObject.SetActive (false);
			icon.GetComponent<Button> ().interactable = false;
		}
			
	}

	public WOAItem getItem() {
		return item;
	}

	public void OnSlotClicked() {
		if (item != null)
			item.use (GameSystem.gameSystem.player);
	}

	public void OnBeginDrag (PointerEventData eventData) {
		dragItem.SetActive (true);
		dragItem.transform.GetChild (0).GetComponent<Image> ().sprite = icon.sprite;
		dragItem.transform.position = eventData.position;
	}

	public void OnDrag (PointerEventData eventData) {
		dragItem.transform.position = eventData.position;
	}

	public void OnEndDrag (PointerEventData eventData) {
		dragItem.SetActive (false);
	}

	public void OnPointerEnter (PointerEventData eventData) {
		if (itemDescription != null) {
			itemDescription.text = item == null ? "" : item.itemName;
		}
	}

	public void OnPointerExit (PointerEventData eventData) {
		if (itemDescription != null) {
			itemDescription.text = "";
		}
	}

}
