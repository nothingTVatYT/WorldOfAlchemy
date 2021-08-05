using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForceTest : MonoBehaviour {

	public float thrust;
	public Vector3 direction;
	public float maxSpeed;
	public bool limitSpeed = false;
	public ForceMode forceMode;

	private Rigidbody body;

	// Use this for initialization
	void Start () {
		body = GetComponent<Rigidbody> ();
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		float currentSpeed = body.velocity.magnitude;
		if (!limitSpeed || currentSpeed < maxSpeed)
			body.AddForce (direction.normalized * thrust, forceMode);
	}
}
