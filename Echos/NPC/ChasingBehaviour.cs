using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChasingBehaviour : PatrollingBehaviour {

	[SerializeField] private Transform Eyes;
	[SerializeField] private bool EyesLookToZ = false;
	[SerializeField] private float aggroRange = 10;
	private BasePlayer basePlayer;
	private bool persuing;
	private Transform player;

	// Use this for initialization
	public override void Start () {
		persuing = false;
		player = GameSystem.gameSystem.player.transform;
		basePlayer = player.gameObject.GetComponent<BasePlayer> ();
	}
	
	// Update is called once per frame
	public override void Update () {
		if (currentState () != State_Dead) {
			if (!basePlayer.isInvisible) {
				float distance = Vector3.Distance (player.position, transform.position);
				Vector3 direction = player.position - transform.position;
				direction.y = 0;
				float angle = Vector3.Angle (direction, EyesLookToZ ? Eyes.forward : Eyes.up);
				if (distance <= aggroRange && (persuing || angle <= 60)) {
					persuing = true;
					setState (State_Attack);
				}
				if (currentState () == State_Attack && distance > aggroRange) {
					persuing = false;
					setStateDelayed (State_WalkBack, Random.Range (3, 10));
				}
			}
		}
		base.Update ();
	}
}
