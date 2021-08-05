using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SpeedLabelController : MonoBehaviour {

	public GameObject target;
	public bool forceUseBody = false;
	private NavMeshAgent agent;
	private Rigidbody body;
	private TextMesh textMesh;
	private float maxSpeed = 0;

	// Use this for initialization
	void Start () {
		textMesh = GetComponent<TextMesh> ();
		agent = target.GetComponent<NavMeshAgent> ();
		body = target.GetComponent<Rigidbody> ();
	}
	
	// Update is called once per frame
	void Update () {
		float speed = 0;
		if (agent != null && !forceUseBody) {
			speed = agent.velocity.magnitude;
		} else if(body != null) {
			speed = body.velocity.magnitude;
		}
		if (maxSpeed < speed)
			maxSpeed = speed;
		textMesh.text = string.Format ("{0:F}\n({1:F})", speed, maxSpeed);
	}
}
