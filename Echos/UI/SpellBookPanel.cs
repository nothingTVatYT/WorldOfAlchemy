using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpellBookPanel : MonoBehaviour {

	public Text spellDescription;
	public GameObject dragItem;

	// Use this for initialization
	void Start () {
		spellDescription.text = "";
		dragItem.SetActive (false);
	}
}
