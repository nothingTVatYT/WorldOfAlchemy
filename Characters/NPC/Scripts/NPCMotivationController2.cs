using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using ECM.Controllers;
using System.Collections;
using ECM.Components;
using ECM.Common;

[RequireComponent(typeof(BaseCharacter), typeof(NavMeshAgent), typeof(CharacterMovement))]
[RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider), typeof(GroundDetection))]
public class NPCMotivationController2 : BaseAgentController, IMotivationController {

	public enum HandSelection { None, LeftHand, RightHand};
	Stack<WOAMotivation> motivationStack = new Stack<WOAMotivation> ();
	Animator anim;
	BaseCharacter baseCharacter;
	Vector3 currentDestination;
	WOAMotivation actionInProgress;
	[SerializeField] HandSelection basketHand;
	[SerializeField] GameObject basketObject;
	[SerializeField] float targetXZMin = 0.5f;
	[SerializeField] float targetYMin = 2f;
	[SerializeField] List<WOAMotivation> initialMotivations = new List<WOAMotivation>();
	[SerializeField] bool logProgress = false;
	[SerializeField] bool resetTarget = false;
	Vector3 destination;
	GameObject basketInstance;
	bool hasBasket = false;
	bool stopFollowTarget = false;
	float kneelSpeed = 1.5f;
	float initialSpeed;
	NPCDialog npcDialog;
	int lockCounter;

	public bool isBusy { get { return motivationStack.Count > 0; }}

	void Start () {
		anim = GetComponent<Animator> ();
		if (anim == null)
			anim = GetComponentInChildren<Animator> ();
		baseCharacter = GetComponent<BaseCharacter> ();
		npcDialog = GetComponent<NPCDialog> ();
		for (int i = initialMotivations.Count - 1; i >=0; i--)
			motivationStack.Push (initialMotivations[i]);
		initialSpeed = agent.speed;
		lockCounter = 0;
	}

	public void injectMotivation(WOAMotivation m) {
		// just in case we get called twice
		if (m == null || motivationStack.Count > 0 && motivationStack.Peek ().Equals (m)) {
			if (logProgress)
				Debug.Log ("Ignoring motivation injection of " + m);
			return;
		}
		motivationStack.Push (m);
		if (logProgress)
			Debug.Log (m + " was injected to " + gameObject.name);
	}

	public void StopFollowTarget() {
		stopFollowTarget = true;
	}

	protected override void SetMoveDirection()
	{
		// If agent is not moving, return

		moveDirection = Vector3.zero;

		if (!agent.hasPath)
			return;

		// If destination not reached,
		// feed agent's desired velocity (lateral only) as the character move direction

		if (agent.remainingDistance > stoppingDistance)
			moveDirection = agent.desiredVelocity.onlyXZ();
	}

	protected override void HandleInput() {

		if (motivationStack.Count > 0) {
			WOAMotivation currentAction = motivationStack.Peek ();

			switch (currentAction.actionType) {
			case WOAMotivation.ActionType.WalkTo:
				destination = currentAction.destinationVector;
				// already set?
				if (currentAction == actionInProgress) {
					if (resetTarget) {
						if (!agent.destination.Equals(destination))
							agent.SetDestination (destination);
						resetTarget = false;
					}
					// are we there?
					if (arrivedAtDestination(actionInProgress)) {
						EndCurrentMotivation ();
						actionInProgress = null;
					} else {
						if (movement.velocity.magnitude < 0.01f && !agent.pathPending) {
							lockCounter++;

							if (lockCounter > 100) {
								Debug.Log (string.Format (gameObject.name + ": agent seems to be stuck at {0} (remaining={1}, destination={2}, has path={3}), should walk to {4}, ground collider={5}",
									transform.position, agent.remainingDistance, agent.destination, agent.hasPath, destination, movement.groundCollider));
								resetTarget = true;
								lockCounter = 0;
							}
						} else
							lockCounter = 0;
					}
				} else {
					agent.speed = initialSpeed * currentAction.speedMultiplier;
					agent.SetDestination (destination);
					agent.isStopped = false;
					//stoppingDistance = targetXZMin;
					StartMotivation (currentAction);
				}
				break;
			case WOAMotivation.ActionType.FollowTarget:
				if (baseCharacter.target != null)
					destination = baseCharacter.target.transform.position;
				else
					destination = transform.position;
				// already set?
				if (currentAction.Equals (actionInProgress)) {
					// are we there?
					if (baseCharacter.target == null || stopFollowTarget) {
						EndCurrentMotivation ();
						actionInProgress = null;
					}
				} else {
					stopFollowTarget = false;
					agent.speed = initialSpeed * currentAction.speedMultiplier;
					StartMotivation (currentAction);
				}
				agent.SetDestination (destination);
				break;
			case WOAMotivation.ActionType.WalkAlong:
				//destination = currentAction.destination.position;
				// already set?
				if (currentAction.Equals (actionInProgress)) {
					// are we there?
					if (arrivedAtDestination(actionInProgress)) {
						Transform next = currentAction.nextDestination ();
						if (next == null) {
							EndCurrentMotivation ();
							agent.speed = initialSpeed;
							actionInProgress = null;
						} else {
							destination = next.position;
							agent.SetDestination (destination);
						}
					}
				} else {
					Transform next = currentAction.nextDestination ();
					if (next != null) {
						destination = next.position;
						agent.SetDestination (destination);
						StartMotivation(currentAction);
						agent.speed = initialSpeed * currentAction.speedMultiplier;
						//stoppingDistance = targetXZMin;
					} else {
						EndCurrentMotivation ();
						actionInProgress = null;
					}
				}
				break;
			case WOAMotivation.ActionType.PickUp:
				if (!currentAction.Equals(actionInProgress)) {
					if (currentAction.destination == null) {
						EndCurrentMotivation ();
						if (logProgress)
							Debug.Log (gameObject.name + " cancelled (b/c no target) " + currentAction);
						actionInProgress = null;
					} else {
						if (currentAction.hasDestination) {
							destination = currentAction.destinationVector;
							agent.speed = initialSpeed * currentAction.speedMultiplier;
							//stoppingDistance = targetXZMin;
							agent.SetDestination (currentAction.destination.position);
							agent.isStopped = false;
						} else {
							destination = transform.position;
						}
						StartMotivation (currentAction);
						StartCoroutine (PickupProcess (actionInProgress));
					}
				}
				break;
			case WOAMotivation.ActionType.GetBasket:
				if (currentAction.Equals(actionInProgress)) {
					if (!currentAction.hasDestination || arrivedAtDestination(actionInProgress)) {
						if (basketObject != null && basketHand != HandSelection.None) {
							if (!hasBasket) {
								GameObject basket = Instantiate (basketObject);
								basket.transform.parent = basketHand == HandSelection.LeftHand 
									? anim.GetBoneTransform (HumanBodyBones.LeftHand) 
									: anim.GetBoneTransform (HumanBodyBones.RightHand);
								basket.transform.localPosition = Vector3.zero;
								basket.transform.localRotation = Quaternion.identity;
								basketInstance = basket;
							}
						}
						hasBasket = true;
						anim.SetBool ("isCarrying", true);
						EndCurrentMotivation ();
						actionInProgress = null;
					}
				} else {
					agent.speed = initialSpeed * currentAction.speedMultiplier;
					if (currentAction.hasDestination) {
						destination = currentAction.destinationVector;
						agent.SetDestination (destination);
						//stoppingDistance = targetXZMin;
					}
					StartMotivation(currentAction);
				}
				break;
			case WOAMotivation.ActionType.DropBasket:
				if (currentAction.Equals(actionInProgress)) {
					if (!currentAction.hasDestination || arrivedAtDestination(actionInProgress)) {
						if (hasBasket && basketInstance != null) {
							basketInstance.transform.parent = null;
							if (currentAction.destroyOnDrop) {
								Destroy (basketInstance);
							} else {
								Rigidbody body = basketInstance.GetComponent<Rigidbody> ();
								if (body != null)
									body.isKinematic = false;
							}
							basketInstance = null;
						}
						hasBasket = false;
						anim.SetBool ("isCarrying", false);
						EndCurrentMotivation ();
						actionInProgress = null;
					}
				} else {
					agent.speed = initialSpeed * currentAction.speedMultiplier;
					if (currentAction.hasDestination) {
						destination = currentAction.destinationVector;
						agent.SetDestination (destination);
						//stoppingDistance = targetXZMin;
					}
					StartMotivation(currentAction);
				}
				break;
			case WOAMotivation.ActionType.DropObject:
				if (currentAction.Equals(actionInProgress)) {
					if (!currentAction.hasDestination || arrivedAtDestination(actionInProgress)) {
						if (currentAction.objectPrefab != null) {
							GameObject go = Instantiate (currentAction.objectPrefab);
							go.transform.position = destination;
							if (currentAction.destination != null)
								go.transform.rotation = currentAction.destination.rotation;
							currentAction.droppedObject = go;
						}
						EndCurrentMotivation ();
						actionInProgress = null;
					}
				} else {
					agent.speed = initialSpeed * currentAction.speedMultiplier;
					if (currentAction.hasDestination) {
						destination = currentAction.destinationVector;
						agent.SetDestination (destination);
						//stoppingDistance = targetXZMin;
					}
					StartMotivation(currentAction);
				}
				break;
			case WOAMotivation.ActionType.TalkTo:
				if (currentAction.Equals (actionInProgress)) {
					if (npcDialog == null || !npcDialog.inProgress || baseCharacter.target == null) {
						EndCurrentMotivation ();
						actionInProgress = null;
					}
				} else {
					agent.SetDestination (transform.position);
					//stoppingDistance = targetXZMin;
					StartMotivation(currentAction);
					baseCharacter.target = GameSystem.gameSystem.player;
					GetComponent<Rigidbody> ().rotation = Quaternion.LookRotation (baseCharacter.target.transform.position - transform.position, Vector3.up);
					if (npcDialog != null) {
						npcDialog.InitializeState (GameSystem.gameSystem.player.GetComponent<BasePlayer> ());
					}
				}
				break;
			}
		}
	}

	IEnumerator PickupProcess(WOAMotivation currentAction) {
		while (!arrivedAtDestination(currentAction)) {
			yield return null;
		}
		float kneelFactor = 0;
		if (currentAction.kneelDown) {
			Debug.Log ("start kneeling");
			while (kneelFactor < 1) {
				kneelFactor += Time.deltaTime * kneelSpeed;
				anim.SetFloat ("kneel", kneelFactor);
				yield return null;
			}
		}
		anim.SetTrigger ("isPickingUp");
		while (!anim.GetCurrentAnimatorStateInfo (0).IsTag ("oneShot"))
			yield return null;
		if (!currentAction.pickupControlledByAnimation)
			tryUse (currentAction.destination.gameObject);
		while (anim.GetCurrentAnimatorStateInfo (0).IsTag ("oneShot"))
			yield return null;
		if (currentAction.kneelDown) {
			while (kneelFactor > 0) {
				kneelFactor -= Time.deltaTime * kneelSpeed;
				anim.SetFloat ("kneel", kneelFactor);
			}
		}
		EndCurrentMotivation ();
		actionInProgress = null;
	}

	void StartMotivation(WOAMotivation currentAction) {
		actionInProgress = currentAction;
		if (logProgress)
			Debug.Log (gameObject + " started " + currentAction + " speed=" + speed);
	}

	void EndCurrentMotivation() {
		WOAMotivation m = motivationStack.Pop ();
		if (logProgress)
			Debug.Log (gameObject.name + " finished " + m);
		if (m.onComplete != null)
			m.onComplete(m);
	}

	IEnumerator ResetTarget(float delay) {
		yield return new WaitForSeconds (delay);
		resetTarget = true;
	}

	public void EndDialog() {
		//npcDialog.inProgress = false;
		baseCharacter.target = null;
	}

	public void tryUseTarget() {
		WOAMotivation current = motivationStack.Peek ();
		if (current.pickupControlledByAnimation)
			tryUse (current.destination.gameObject);
	}

	void tryUse(GameObject go) {
		ItemHandler itemHandler = go.GetComponent<ItemHandler> ();
		if (itemHandler != null) {
			itemHandler.use (BaseCharacter.getCharacterFromGameObject (gameObject));
		}
	}

	bool arrivedAtDestination(WOAMotivation m) {
		return m.HasReachedTarget(transform, targetYMin, targetXZMin);
	}

	/// <summary>
	/// Callback to draw gizmos that are pickable and always drawn.
	/// </summary>
	void OnDrawGizmosSelected()
	{
		if (actionInProgress != null)
			actionInProgress.OnDrawGizmosSelected();
	}
}
