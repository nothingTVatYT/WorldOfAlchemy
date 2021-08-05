using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectUI : MonoBehaviour {

	[SerializeField] private GameObject player;
	[SerializeField] private GameObject effectSlots;
	[SerializeField] private GameObject effectSlot;
	private List<GameObject> objectsToDestroy = new List<GameObject> ();

	void Start () {
		if (player == null)
			player = GameSystem.gameSystem.player;
		BaseCharacter baseCharacter = BaseCharacter.getCharacterFromGameObject (player);
		baseCharacter.onEffectChangedCallback += effectsChanged;
	}
	
	void Update () {
		if (objectsToDestroy.Count > 0) {
			foreach (GameObject obj in objectsToDestroy)
				Destroy (obj);
			objectsToDestroy.Clear ();
		}
	}

	void effectsChanged(WOAEffect removedEffect, WOAEffect newEffect) {
		if (removedEffect != null) {
			for (int i = 0; i < effectSlots.transform.childCount; i++) {
				EffectSlotHandler slot = effectSlots.transform.GetChild (i).GetComponent<EffectSlotHandler> ();
				if (slot.effect == removedEffect)
					objectsToDestroy.Add (slot.gameObject);
			}
		}
		if (newEffect != null) {
			GameObject newSlotUI = Instantiate (effectSlot);
			EffectSlotHandler newSlot = newSlotUI.GetComponent<EffectSlotHandler> ();
			newSlot.effect = newEffect;
			newSlotUI.transform.SetParent (effectSlots.transform);
		}
	}
}
