using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Teleport : MonoBehaviour
{
	public GameObject UILocation;
	private GameObject[] locations;
	private GameObject player;
	private KeyCode[] keyCodes = new KeyCode[] {KeyCode.Alpha0, KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4,
		KeyCode.Alpha5, KeyCode.Alpha6, KeyCode.Alpha7, KeyCode.Alpha8, KeyCode.Alpha9};
	private bool readNumber = false;

	// Use this for initialization
	void Start ()
	{
		locations = GameObject.FindGameObjectsWithTag ("StartLocation");
		player = GameObject.FindGameObjectWithTag ("Player");
		Debug.Log (string.Format("Found start locations: {0}", locations.Length));
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (Input.GetKeyUp (KeyCode.T))
			readNumber = true;
		if (readNumber) {
			for (int i = 0; i < keyCodes.Length; i++) { 
				if (Input.GetKeyUp (keyCodes[i])) {
					gotoLocation (i);
					readNumber = false;
					break;
				}
			}
		}
	}

	void gotoLocation (int n)
	{
		if (n < locations.Length) {
			player.transform.position = locations [n].transform.position;
			if (UILocation != null)
				UILocation.GetComponent<Text> ().text = locations [n].name;
		}
	}
}
