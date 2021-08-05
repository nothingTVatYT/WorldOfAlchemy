using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatrollingBehaviour : NPCBehaviour {

	[SerializeField] private Transform[] wayPoints;
	[SerializeField] private float pauseAtWayPoints = 0;
	[SerializeField] private float pauseAtLastWayPoint = 6;
	float patrolTime;

	public override void Start ()
	{
		base.Start ();
	}

	// Update is called once per frame
	public override void Update () {
		if (currentState () != State_Dead && currentState() != State_Attack) {
			if (currentState () == State_Idle && wayPoints.Length > 0 && Random.value < 0.006f) {
				if (patrolTime == 0f)
					patrolTime = Time.time + Random.Range (15, 30);
				else if (patrolTime <= Time.time) {
					Journey journey = new Journey (wayPoints, pauseAtWayPoints, pauseAtLastWayPoint);
					patrolOnceAndWait (journey);
					patrolTime = 0;
				}
			}
		}
		base.Update ();
	}
}

