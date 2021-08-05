using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EyeOfTheBeholderController : MonoBehaviour, ISpellEffect {

	private GameObject target;
	private int state = 0;
	[SerializeField] private GameObject eyeMesh;
	[SerializeField] private float hiddenVerticalRotation = 100;
	[SerializeField] private float summonTime = 1f;
	[SerializeField] bool attachToCamera = true;
	private float timeSummoned;
	private float timeFadeStart;
	private float timeUnsummoned;
	private float effectDuration;
	private float originalAlpha;

	void Start () {
		originalAlpha = eyeMesh.GetComponent<MeshRenderer> ().material.GetColor ("_Color").a;
	}
	
	void LateUpdate () {
		if (target != null) {
			float lasted = Time.time - timeSummoned;
			switch (state) {
			case 0:
				float factor = (Time.time - timeSummoned) / summonTime;
				if (factor >= 1) {
					state = 1;
				} else {
					transform.localRotation = Quaternion.Euler (hiddenVerticalRotation * (1 - factor), 0, 0);
				}
				break;
			case 1:
				if (lasted > effectDuration) {
					state = 2;
					timeFadeStart = Time.time;
				}
				// just follow - done by parenting
				break;
			case 2:
				float fadeFactor = (Time.time - timeFadeStart) / summonTime;
				if (fadeFactor >= 1) {
					timeUnsummoned = Time.time;
					state = 3;
				}
				Color lensColor = eyeMesh.GetComponent<MeshRenderer> ().material.GetColor ("_Color");
				lensColor.a = Mathf.Lerp(originalAlpha, 1, fadeFactor);
				eyeMesh.GetComponent<MeshRenderer> ().material.SetColor ("_Color", lensColor);
				break;
			case 3:
				float factor2 = (Time.time - timeUnsummoned) / summonTime;
				if (factor2 >= 1) {
					Color c = eyeMesh.GetComponent<MeshRenderer> ().material.GetColor ("_Color");
					c.a = originalAlpha;
					eyeMesh.GetComponent<MeshRenderer> ().material.SetColor ("_Color", c);
					Destroy (gameObject);
				} else {
					transform.localRotation = Quaternion.Euler (hiddenVerticalRotation * factor2, 0, 0);
				}
				break;
			}
		}
	}

	public bool attach(GameObject t) {
		target = t;
		timeSummoned = Time.time;
		if (t != null) {
			if (attachToCamera)
				transform.SetParent (Camera.main.transform);
			else
				transform.SetParent (t.transform);
			transform.localPosition = Vector3.zero;
		}
		return true;
	}

	public void setSpell(WOASpell spell) {
		effectDuration = spell.spellEffectDuration;
	}
}
