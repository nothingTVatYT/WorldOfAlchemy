using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellSpecialEffect : MonoBehaviour {

	private Vector3 rotationVector;
	public float rotationSpeed = 1f;

	// Use this for initialization
	void Start () {
		rotationVector = transform.localRotation.eulerAngles;
	}
	
	// Update is called once per frame
	void Update () {
		rotationVector.z += Time.deltaTime * rotationSpeed * 360f;
		transform.localRotation = Quaternion.Euler (rotationVector);
	}
}
