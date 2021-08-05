using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class fpsRig : MonoBehaviour
{

	[SerializeField] private GameObject targetPanel;
	[SerializeField] private GameObject weapon1;
	[SerializeField] private GameObject map;
	[SerializeField] private GameObject mapMarker;
	[SerializeField] private GameObject portableLight;
	private Animator animator;
	private bool liftedHands;
	private bool isAttacking;
	private bool liftedMap;
	private BasePlayer basePlayer;

	// Use this for initialization
	void Start ()
	{
		animator = GetComponent<Animator> ();
		if (weapon1 != null)
			weapon1.SetActive (false);
		if (map != null)
			map.SetActive (false);
		if (mapMarker != null)
			mapMarker.SetActive (false);
		basePlayer = GameSystem.gameSystem.player.GetComponent<BasePlayer> ();
		basePlayer.onItemEquipChangedCallback += equipped;
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (Input.GetButtonDown ("Map") && !liftedHands) {
			if (!liftedMap) {
				weapon1.SetActive (false);
				map.SetActive (true);
				mapMarker.SetActive (true);
				animator.SetBool ("liftmap", true);
				liftedMap = true;
			} else {
				animator.SetBool ("liftmap", false);
				liftedMap = false;
				Invoke ("hideMap", 2);
			}
		}
		if (Input.GetButtonDown("LiftHands") && !liftedMap) {
			if (liftedHands) {
				liftedHands = false;
				animator.SetBool ("lift", false);
			} else {
				hideMap ();
				weapon1.SetActive (true);
				liftHands ();
			}
		}
		/*
		if (GameSystem.gameSystem.playerIsAttacking()) {
			if (!liftedHands) {
				Weapon weapon = basePlayer.getEquippedWeapon ();
				if (weapon != null && weapon.type == Weapon.WeaponType.Knife)
					weapon1.SetActive (true);
				liftHands ();
			}
			if (isAttacking && basePlayer.targetCharacter != null && basePlayer.targetCharacter.isDead) {
				isAttacking = false;
			} else if (!isAttacking) {
				animator.SetBool ("isAttacking", true);
				isAttacking = true;
			}
		} else {
			if (!GameSystem.gameSystem.hasTarget () || GameSystem.gameSystem.currentTargetIsDead()) {
				animator.SetBool ("isAttacking", false);
				isAttacking = false;
			}
		} */
	}

	void equipped(WOAItem item, bool isEquipped) {
		Debug.Log ((isEquipped ? "" : "un") + "equip " + item.itemName);
		if (item is WOALight) {
			if (portableLight != null) {
				WOALight lightItem = (WOALight)item;
				Light pl = portableLight.GetComponent<Light> ();
				pl.color = lightItem.lightColor;
				pl.range = lightItem.lightRange;
				pl.intensity = 0; //lightItem.lightIntensity;
				StartCoroutine(Fade(pl, lightItem.lightIntensity, 1.5f));
				portableLight.SetActive (isEquipped);
			}
		}
	}

	void FixedUpdate ()
	{
	}

	void hideMap() {
		map.SetActive(false);
		mapMarker.SetActive (false);
	}

	void liftHands ()
	{
		liftedHands = true;
		animator.SetBool ("lift", true);
	}

	IEnumerator Fade(Light pl, float intensity, float delayTime)
	{
		float increment = (intensity - pl.intensity) / (delayTime * 10);
		while (pl.intensity < intensity) {
			pl.intensity += increment;
			yield return new WaitForSeconds(0.1f);
		}
	}

}
