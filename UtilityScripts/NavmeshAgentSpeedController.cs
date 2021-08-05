using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NavmeshAgentSpeedController : MonoBehaviour {

	public SpringJoint joint;
	[Range(0,1)]
	public float influence = 0.1f;
	public float minForceToAct = 2000;
	public float maxExpectedForce = 9000;
	private NavMeshAgent agent;

	void Start () {
		agent = GetComponent<NavMeshAgent> ();
	}
	
	void Update () {
		if (joint != null) {
			Vector3 velocity = agent.velocity;
			float jointForce = joint.currentForce.magnitude;
			if (jointForce > minForceToAct) {
				float manipulator = Mathf.Clamp01 (jointForce * influence / (maxExpectedForce - minForceToAct));
				velocity *= (1 - manipulator * Time.deltaTime);
				agent.velocity = velocity;
			}
		}
	}
}
