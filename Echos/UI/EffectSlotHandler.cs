using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class EffectSlotHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {

	[SerializeField] GameObject bubbleHelp;
	[SerializeField] Text bubbleHelpText;
	[SerializeField] Image state;
	[SerializeField] GameObject imageIcon;
	public WOAEffect effect;

	// Use this for initialization
	void Start () {
		imageIcon.SetActive (effect != null);
		if (effect != null)
			imageIcon.GetComponent<Image> ().sprite = effect.icon;
	}
	
	void LateUpdate () {
		if (effect != null)
			state.fillAmount = effect.state;
	}

	public void OnPointerEnter (PointerEventData eventData) {
		if (bubbleHelp != null && effect != null) {
			bubbleHelpText.text = effect.effectName;
			bubbleHelp.SetActive(true);
		}
	}

	public void OnPointerExit (PointerEventData eventData) {
		if (bubbleHelp != null) {
			bubbleHelp.SetActive(false);
		}
	}

}
