using System;
using UnityEngine;

[Serializable]
public class WOAItem
{
	public string itemName;
	public Sprite icon;
	public float interactionRange;
	public int level;
	[HideInInspector]
	public string templateName;
	[HideInInspector]
	public string templateClass;

	public WOAItem() {
	}

	public WOAItem(WOAItem other) {
		itemName = other.itemName;
		icon = other.icon;
		interactionRange = other.interactionRange;
		level = other.level;
		templateName = other.templateName;
		templateClass = other.templateClass;
	}

	public virtual bool use(GameObject actor) {
		Debug.Log ("item " + itemName + " used by " + actor);
		return false;
	}
}
