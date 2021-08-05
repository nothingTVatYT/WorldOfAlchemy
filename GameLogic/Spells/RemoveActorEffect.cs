using System.Collections;
using UnityEngine;

public class RemoveActorEffect : MonoBehaviour, ISpellEffect {

	WOASpell thisSpell;

	public void setSpell (WOASpell spell) {
		thisSpell = spell;
	}

	public bool attach (GameObject target) {
		transform.position = target.transform.position;
		StartCoroutine (RemoveObject (gameObject, thisSpell.spellPrepareDuration));
		return true;
	}

	IEnumerator RemoveObject(GameObject obj, float delayed) {
		yield return new WaitForSeconds (delayed);
		Destroy (obj);
	}
}
