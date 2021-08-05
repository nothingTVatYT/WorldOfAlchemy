using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NavmeshPhysicsAgent : MonoBehaviour {

	public float thrust = 1f;
	public ForceMode forceMode;
	public bool agentControlsRotation = true;
	public float angularSpeedDivider = 45;
	public bool useAgentDesiredVelocity = true;
	public bool debug = false;
	private NavMeshAgent agent;
	private Rigidbody body;
	private AudioSource audioSource;
	private bool controlRigidbody;
	private Vector3 currentForce;
	private Vector3 gizmoOffset = Vector3.up * 2;

	// Use this for initialization
	void Start () {
		agent = GetComponent<NavMeshAgent> ();
		body = GetComponent<Rigidbody> ();
		controlRigidbody = agent != null && body != null;
		if (controlRigidbody) {
			agent.updatePosition = false;
			agent.updateRotation = false;
			agent.updateUpAxis = agentControlsRotation;
			body.isKinematic = false;
		}
		audioSource = GetComponent<AudioSource> ();
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		if (controlRigidbody) {
			Vector3 v = Vector3.zero;
			if (useAgentDesiredVelocity) {
				v = agent.pathPending ? Vector3.zero : agent.desiredVelocity;
			} else {
				if(agent.hasPath) {
					foreach (Vector3 corner in agent.path.corners)
						if ((body.position - corner).sqrMagnitude > 2f) {
							v = corner - body.position;
							break;
						}
				}
			}
			v.y = 0;
			float currentSpeed = body.velocity.magnitude;
			float acceleration = 0f;
			if (currentSpeed < agent.speed)
				acceleration = agent.acceleration;
			currentForce = v.normalized * body.mass * acceleration * thrust + Physics.gravity * (forceMode == ForceMode.Force ? body.mass : 1f);
			body.AddForce (currentForce, forceMode);
			if (v.sqrMagnitude > 0) {
				if (!agentControlsRotation) {
					//Vector3 lookVector = new Vector3 (v.x, 0, v.z).normalized;
					body.rotation = Quaternion.Slerp (body.rotation, Quaternion.LookRotation (v), Time.deltaTime * agent.angularSpeed / angularSpeedDivider);
				}
				if (audioSource != null && !audioSource.isPlaying)
					audioSource.Play ();
			} else {
				if (audioSource != null && audioSource.isPlaying)
					audioSource.Stop ();
			}
			agent.nextPosition = body.position;
			agent.velocity = body.velocity;
		}
	}

	void _LateUpdate() {
		if (controlRigidbody) {
			agent.nextPosition = body.position;
			agent.velocity = body.velocity;
		}
	}

	public void OnDrawGizmos () {
		if (debug && controlRigidbody) {
			Gizmos.color = Color.red;
			Gizmos.DrawLine (transform.position + gizmoOffset, transform.position + currentForce + gizmoOffset);
			Gizmos.DrawSphere (agent.destination, 1f);
			Gizmos.color = Color.blue;
			foreach (Vector3 v in agent.path.corners)
				Gizmos.DrawSphere (v, 0.3f);
		}
	}
}
