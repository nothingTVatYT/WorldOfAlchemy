using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectorController : MonoBehaviour {

	public GameObject projector;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		projector.GetComponent<Projector> ().fieldOfView = Mathf.Sin (Time.time) * 30f + 35f;
	}
}
