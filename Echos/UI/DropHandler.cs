using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class DropHandler : MonoBehaviour, IDropHandler {

	public void OnDrop(PointerEventData eventData) {
		Debug.Log ("onDrop called on " + gameObject.name + " with " + eventData.selectedObject + " of " + eventData.selectedObject.transform.parent);
	}
}
