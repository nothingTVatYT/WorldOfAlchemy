using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

//[RequireComponent(typeof(LinkedCharacter))]
public class NPCBehaviour : MonoBehaviour {

	public const int State_Idle = 0;
	public const int State_WalkBack = 1;
	public const int State_Attack = 2;
	public const int State_Wait = 3;
	public const int State_Patrol = 4;
	public const int State_Dead = 5;

	[SerializeField] private float walkSpeed = 3;
	[SerializeField] private float combatRange = 2;
	[SerializeField] private float rotationSpeed = 5;

	private Animator animator;
	private NavMeshAgent agent;
	private CharacterController cc;
	private Vector3 initialPosition;
	private Vector3 moveDirection;
	private Quaternion initialRotation;
	private AudioSource audioSource;
	private bool isAlreadyDead;
	int state = State_Idle;
	float stateSet;
	float duration;
	int nextState;
	private Journey journey;
	private BaseCharacter npc;
	[SerializeField] private float respawnTime = 30;
	[SerializeField] private Transform respawnLocation;
	[SerializeField] private Light portableLight;
	[SerializeField] private AudioClip dyingClip;
	[SerializeField] private float timeBeforeDyingSound = 1.8f;

	public virtual void Start() {
		npc = gameObject.GetComponent<BaseCharacter> ();
		animator = GetComponent<Animator> ();
		initialPosition = transform.position;
		initialRotation = transform.rotation;
		cc = GetComponent<CharacterController> ();
		moveDirection = new Vector3 ();
		npc.currentHP = npc.effectiveMaxHP;
		agent = GetComponent<NavMeshAgent> ();
		if (agent != null && !agent.isActiveAndEnabled)
			agent = null;
		audioSource = GetComponent<AudioSource> ();
		isAlreadyDead = false;
	}

	public float getRelativeHealth() {
		return (float)npc.currentHP / npc.effectiveMaxHP;
	}

	public void setState(int state) {
		this.state = state;
		stateSet = Time.time;
	}

	public int currentState() {
		return state;
	}

	public void setStateDelayed(int state, float seconds) {
		duration = seconds;
		setState (State_Wait);
		nextState = state;
	}

	public void patrolOnceAndWait(Journey journey) {
		this.journey = journey;
		setState (State_Patrol);
	}

	public virtual void Update () {
		if (npc == null || npc.isDead)
			state = State_Dead;
		switch (state) {
		case State_Dead:
			if (!isAlreadyDead) {
				stopWalking ();
				animator.SetBool ("isAttacking", false);
				animator.SetBool ("isIdle", false);
				animator.SetBool ("isWalking", false);
				animator.SetBool ("isDead", true);
				audioSource.Stop ();
				if (dyingClip != null)
					StartCoroutine(playOneShotDelayed(dyingClip, timeBeforeDyingSound));
				if (portableLight != null)
					portableLight.enabled = false;
				isAlreadyDead = true;
			}
			if (Time.time - npc.timeDied > respawnTime) {
				if (respawnLocation != null) {
					cc.transform.position = respawnLocation.position;
					setStateDelayed (State_WalkBack, 1);
				} else {
					cc.transform.position = initialPosition;
					cc.transform.rotation = initialRotation;
					setStateDelayed (State_Idle, 1);
				}
				npc.currentHP = npc.effectiveMaxHP;
				cc.enabled = true;
				//if (agent != null)
				//	agent.enabled = true;
				audioSource.Play ();
				isAlreadyDead = false;
				animator.SetBool ("isDead", false);
				if (portableLight != null)
					portableLight.enabled = true;
			}
			break;
		case State_Idle:
			animator.SetBool ("isIdle", true);
			animator.SetBool ("isWalking", false);
			animator.SetBool ("isAttacking", false);
			break;
		case State_Wait:
			animator.SetBool ("isIdle", true);
			animator.SetBool ("isWalking", false);
			animator.SetBool ("isAttacking", false);
			if (Time.time - stateSet > duration) {
				state = nextState;
			}
			break;
		case State_WalkBack:
			animator.SetBool ("isIdle", false);
			if (Vector3.Distance (initialPosition, transform.position) > 1) {
				if (!walkToAndTurn (initialPosition, initialRotation)) {
					stopWalking ();
					animator.SetBool ("isWalking", false);
					state = State_Idle;
				}
			} else {
				state = State_Idle;
				stopWalking ();
				animator.SetBool ("isWalking", false);
			}
			break;
		case State_Patrol:
			Vector3 v = journey.getCurrentWaypoint().position - transform.position;
			animator.SetBool ("isIdle", false);
			if (v.magnitude > 1) {
				if (Vector3.Distance(agent.destination, transform.position) < 1)
					walkTo(journey.getCurrentWaypoint().position);
			} else {
				if (journey.getNextWaypoint() == null) {
					setStateDelayed (State_WalkBack, journey.delay2);
				} else
					setStateDelayed (State_Patrol, journey.delay1);
			}
			break;
		case State_Attack:
			if (npc.target != null) {
				Vector3 direction = npc.target.transform.position - transform.position;
				direction.y = 0;
				transform.rotation = Quaternion.Slerp (transform.rotation, Quaternion.LookRotation (direction), rotationSpeed * Time.deltaTime);
				animator.SetBool ("isIdle", false);
				float distance = Vector3.Distance (npc.target.transform.position, transform.position);
				if (distance > combatRange) {
					walkTo (npc.target.transform.position);
					animator.SetBool ("isAttacking", false);
				} else {
					animator.SetBool ("isAttacking", true);
					animator.SetBool ("isWalking", false);
					stopWalking ();
					BaseCharacter bc = BaseCharacter.getCharacterFromGameObject (npc.target);
					if (bc != null)
						bc.getsAttackedBy (npc);
				}
			}
			break;
		}
	}

	private void stopWalking() {
		if (agent != null)
			agent.SetDestination (transform.position);
	}

	private bool walkTo(Vector3 destination) {
		Debug.Log ("walking to " + destination);
		if (Vector3.Distance (destination, transform.position) > 1) {
			if (agent != null) {
				agent.SetDestination (destination);
			} else {
				Vector3 directionBack = destination - transform.position;
				directionBack.y = 0;
				float angleBack = Vector3.Angle (directionBack, transform.forward);
				if (angleBack > 2) {
					transform.rotation = Quaternion.Slerp (transform.rotation, Quaternion.LookRotation (directionBack), rotationSpeed * Time.deltaTime);
				}
				moveDirection = transform.TransformDirection (Vector3.forward) * walkSpeed;
				cc.SimpleMove (moveDirection);
			}
			animator.SetBool ("isWalking", true);
			return true;
		}
		return false;
	}

	private bool walkToAndTurn(Vector3 destination, Quaternion rotation) {
		Debug.Log ("walking to " + destination + " and turn");
		if (!walkTo (destination)) {
			float angleRotateBack = Quaternion.Angle (transform.rotation, rotation);
			if (angleRotateBack > 1) {
				transform.rotation = Quaternion.Slerp (transform.rotation, rotation, rotationSpeed * Time.deltaTime);
			} else {
				animator.SetBool ("isWalking", false);
				return false;
			}
		}
		return true;
	}

	IEnumerator playOneShotDelayed(AudioClip clip, float delay) {
		yield return new WaitForSeconds (delay);
		audioSource.PlayOneShot (clip);
	}
}
