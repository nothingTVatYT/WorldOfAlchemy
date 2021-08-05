using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SpellSlot : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler {

	[SerializeField] private Image icon;
	private WOASpell spell;
	private SpellBookPanel spellBookPanel;
	private Text spellDescription;
	private GameObject dragItem;

	void Start () {
		if (icon == null)
			icon = GetComponentInChildren<Image> ();
		spellBookPanel = transform.parent.parent.GetComponent<SpellBookPanel> ();
		spellDescription = spellBookPanel.spellDescription;
		dragItem = spellBookPanel.dragItem;
	}
	
	public void onClickSpellSlot() {
		spell.use (GameSystem.gameSystem.player);
	}

	public void setSpell(WOASpell newSpell) {
		spell = newSpell;
		icon.sprite = spell.icon;
		gameObject.transform.localScale = Vector3.one;
	}

	public WOASpell getSpell() {
		return spell;
	}

	public void OnPointerEnter (PointerEventData eventData) {
		if (spellDescription != null) {
			spellDescription.text = spell == null ? "" : spell.itemName;
		}
	}

	public void OnPointerExit (PointerEventData eventData) {
		if (spellDescription != null) {
			spellDescription.text = "";
		}
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

}
