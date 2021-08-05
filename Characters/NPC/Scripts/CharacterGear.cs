using System;
using System.Collections;
using UnityEngine;

public class CharacterGear : MonoBehaviour {

	[Serializable]
	public struct MapSettings {
		public GameObject map;
		public Transform mapHandle;
		public Transform loweredLocation;
		public Transform raisedLocation;
		public float mapAnimSpeed;
	}
	BaseCharacter baseCharacter;
	public GameObject portableLight;
	Animator anim;
	public Vector3 lightOffset = new Vector3 (-0.23f, 1.3f, 0.25f);
	public Vector3 lightRotation = new Vector3 (0, 1, 0);
	public float lightIKWeight = 0.8f;
	public MapSettings mapSettings;
	bool isLightEquipped;
	bool isMapUp;

	void Start () {
		baseCharacter = transform.parent.GetComponent<BaseCharacter> ();
		baseCharacter.onItemEquipChangedCallback += equipped;
		anim = GetComponent<Animator> ();
		isMapUp = false;
		mapSettings.map.SetActive (false);
	}
	
	void Update () {
		if (Input.GetKeyDown (KeyCode.M)) {
			StartCoroutine (MoveMap (!isMapUp));
		}
	}

	void equipped(WOAItem item, bool isEquipped) {
		Debug.Log ((isEquipped ? "" : "un") + "equip " + item.itemName);
		if (item is WOALight) {
			if (portableLight != null) {
				WOALight lightItem = (WOALight)item;
				isLightEquipped = isEquipped;
				Light pl = portableLight.GetComponentInChildren<Light> ();
				if (pl != null) {
					pl.color = lightItem.lightColor;
					pl.range = lightItem.lightRange;
					pl.intensity = 0; //lightItem.lightIntensity;
					StartCoroutine (Fade (pl, lightItem.lightIntensity, 1.5f));
				}
				portableLight.SetActive (isEquipped);
			}
		}
	}

	public void OnAnimatorIK(int layerIndex) {
		if (isLightEquipped) {
			anim.SetIKPosition (AvatarIKGoal.LeftHand, transform.TransformPoint (lightOffset));
			anim.SetIKPositionWeight (AvatarIKGoal.LeftHand, lightIKWeight);
			anim.SetIKRotation (AvatarIKGoal.LeftHand, Quaternion.Euler(transform.TransformDirection(lightRotation)) * transform.rotation);
			anim.SetIKRotationWeight (AvatarIKGoal.LeftHand, lightIKWeight);
		}
		else if (isMapUp) {
			anim.SetIKPosition (AvatarIKGoal.LeftHand, mapSettings.mapHandle.position);
			anim.SetIKPositionWeight (AvatarIKGoal.LeftHand, 1f);
			anim.SetIKRotation (AvatarIKGoal.LeftHand, mapSettings.mapHandle.rotation);
			anim.SetIKRotationWeight (AvatarIKGoal.LeftHand, 1f);
			anim.SetLookAtPosition (mapSettings.map.transform.position);
			anim.SetLookAtWeight (1);
		}
	}

	IEnumerator Fade(Light pl, float intensity, float delayTime)
	{
		float increment = (intensity - pl.intensity) / (delayTime * 10);
		while (pl.intensity < intensity) {
			pl.intensity += increment;
			yield return new WaitForSeconds(0.1f);
		}
	}

	IEnumerator MoveMap(bool raise) {
		float t;
		float moved = 0;
		isMapUp = true;
		if (raise) {
			mapSettings.map.SetActive (true);
			mapSettings.map.transform.localPosition = mapSettings.loweredLocation.localPosition;
			mapSettings.map.transform.localRotation = mapSettings.loweredLocation.localRotation;
		}
		while (moved < 1f) {
			yield return new WaitForSeconds (0.02f);
			moved += mapSettings.mapAnimSpeed * 0.05f;
			t = raise ? moved : 1f - moved;
			mapSettings.map.transform.localPosition = Vector3.Slerp (mapSettings.loweredLocation.localPosition, mapSettings.raisedLocation.localPosition, t);
			mapSettings.map.transform.localRotation = Quaternion.Slerp (mapSettings.loweredLocation.localRotation, mapSettings.raisedLocation.localRotation, t);
		}
		if (!raise) {
			mapSettings.map.SetActive (false);
			isMapUp = false;
		}
	}
}
