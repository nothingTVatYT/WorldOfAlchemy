using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NPCMotivationController : MonoBehaviour, IMotivationController {

	private Stack<WOAMotivation> motivationStack = new Stack<WOAMotivation> ();
	private NavMeshAgent agent;
	private Animator anim;
	BaseCharacter baseCharacter;
	private Vector3 currentDestination;
	private WOAMotivation actionInProgress;
	[SerializeField] private Transform carryingHand;
	[SerializeField] private GameObject basketObject;
	[SerializeField] private float destinationThreshold = 2f;
	[SerializeField] private List<WOAMotivation> initialMotivations = new List<WOAMotivation>();
	[SerializeField] private bool logProgress = false;
	private Vector3 destination;
	private GameObject basketInstance;
	private bool oneShotTriggered = false;
	private bool oneShotBegan = false;
	private bool oneShotDone = false;
	private bool hasBasket = false;
	bool stopFollowTarget = false;
	private float kneelFactor = 0;
	private float kneelSpeed = 1.5f;
	private float initialSpeed;
	float lastActionStart;
	NPCDialog npcDialog;

	public bool isBusy { get { return motivationStack.Count > 0; }}

	void Start () {
		agent = GetComponent<NavMeshAgent> ();
		anim = GetComponent<Animator> ();
		baseCharacter = GetComponent<BaseCharacter> ();
		npcDialog = GetComponent<NPCDialog> ();
		for (int i = initialMotivations.Count - 1; i >=0; i--)
			motivationStack.Push (initialMotivations[i]);
		initialSpeed = agent.speed;
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

	void Update () {
		if (motivationStack.Count > 0) {
			WOAMotivation currentAction = motivationStack.Peek ();
			switch (currentAction.actionType) {
			case WOAMotivation.ActionType.WalkTo:
				destination = currentAction.destination.position;
				// already set?
				if (currentAction.Equals (actionInProgress)) {
					// are we there?
					if (arrivedAtDestination()) {
						motivationStack.Pop ();
						if (logProgress)
							Debug.Log(gameObject.name + " finished " + actionInProgress);
						actionInProgress = null;
					}
				} else {
					agent.speed = initialSpeed * currentAction.speedMultiplier;
					agent.SetDestination (destination);
					actionInProgress = currentAction;
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
						motivationStack.Pop ();
						if (logProgress)
							Debug.Log(gameObject.name + " finished " + actionInProgress);
						actionInProgress = null;
					}
				} else {
					stopFollowTarget = false;
					agent.speed = initialSpeed * currentAction.speedMultiplier;
					actionInProgress = currentAction;
				}
				agent.SetDestination (destination);
				break;
			case WOAMotivation.ActionType.WalkAlong:
				//destination = currentAction.destination.position;
				// already set?
				if (currentAction.Equals (actionInProgress)) {
					// are we there?
					if (arrivedAtDestination()) {
						Transform next = currentAction.nextDestination ();
						if (next == null) {
							motivationStack.Pop ();
							if (logProgress)
								Debug.Log(gameObject.name + " finished " + actionInProgress);
							agent.speed = initialSpeed;
							actionInProgress = null;
						} else
							agent.SetDestination (next.position);
					}
				} else {
					Transform next = currentAction.nextDestination ();
					if (next != null) {
						agent.SetDestination (next.position);
						actionInProgress = currentAction;
						agent.speed = initialSpeed * currentAction.speedMultiplier;
					} else {
						motivationStack.Pop ();
						if (logProgress)
							Debug.Log(gameObject.name + " cancelled (not a track) " + currentAction);
						actionInProgress = null;
					}
				}
				break;
			case WOAMotivation.ActionType.PickUp:
				// already set?
				if (currentAction.Equals (actionInProgress)) {
					//Debug.Log ("remaining distance: " + agent.remainingDistance);
					// are we there?
					if (arrivedAtDestination()) {
						agent.SetDestination (transform.position);
						agent.isStopped = true;
						if (anim.GetCurrentAnimatorStateInfo(0).IsTag("oneShot") && !oneShotBegan && !oneShotDone) {
							oneShotBegan = true;
						}
						if (!anim.GetCurrentAnimatorStateInfo(0).IsTag("oneShot") && oneShotBegan && !oneShotDone) {
							if (!currentAction.pickupControlledByAnimation)
								tryUse (currentAction.destination.gameObject);
							oneShotDone = true;
						}
						if (currentAction.kneelDown && kneelFactor < 1f && !oneShotDone) {
							kneelFactor += Time.deltaTime * kneelSpeed;
							anim.SetFloat ("kneel", kneelFactor);
						} else {
							if (!oneShotBegan && !oneShotTriggered) {
								anim.SetTrigger ("isPickingUp");
								oneShotTriggered = true;
							}
							if (oneShotDone) {
								if (currentAction.kneelDown && kneelFactor > 0f) {
									kneelFactor -= Time.deltaTime * kneelSpeed;
									anim.SetFloat ("kneel", kneelFactor);
								} else {
									motivationStack.Pop ();
									if (logProgress)
										Debug.Log (gameObject.name + " finished " + currentAction);
									actionInProgress = null;
									agent.isStopped = false;
								}
							}
						}
					}
				} else {
					if (currentAction.destination == null) {
						motivationStack.Pop ();
						if (logProgress)
							Debug.Log (gameObject.name + " cancelled (b/c no target) " + currentAction);
						actionInProgress = null;
					} else {
						agent.speed = initialSpeed * currentAction.speedMultiplier;
						agent.SetDestination (currentAction.destination.position);
						actionInProgress = currentAction;
						oneShotTriggered = false;
						oneShotBegan = false;
						oneShotDone = false;
						kneelFactor = 0;
					}
				}
				break;
			case WOAMotivation.ActionType.GetBasket:
				if (currentAction.Equals(actionInProgress)) {
					if (currentAction.destination == null || arrivedAtDestination()) {
						if (basketObject != null) {
							if (!hasBasket) {
								GameObject basket = Instantiate (basketObject);
								basket.transform.parent = carryingHand;
								basket.transform.localPosition = Vector3.zero;
								basket.transform.localRotation = Quaternion.identity;
								basketInstance = basket;
							}
						}
						hasBasket = true;
						anim.SetBool ("isCarrying", true);
						motivationStack.Pop ();
						if (logProgress)
							Debug.Log (gameObject.name + " finished " + currentAction);
						actionInProgress = null;
					}
				} else {
					agent.speed = initialSpeed * currentAction.speedMultiplier;
					if (currentAction.destination != null)
						agent.SetDestination (currentAction.destination.position);
					actionInProgress = currentAction;
				}
				break;
			case WOAMotivation.ActionType.DropBasket:
				if (currentAction.Equals(actionInProgress)) {
					if (currentAction.destination == null || arrivedAtDestination()) {
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
						motivationStack.Pop ();
						if (logProgress)
							Debug.Log (gameObject.name + " finished " + currentAction);
						actionInProgress = null;
					}
				} else {
					agent.speed = initialSpeed * currentAction.speedMultiplier;
					if (currentAction.destination != null)
						agent.SetDestination (currentAction.destination.position);
					actionInProgress = currentAction;
				}
				break;
			case WOAMotivation.ActionType.TalkTo:
				if (currentAction.Equals (actionInProgress)) {
					if ((Time.time - lastActionStart) > 3 && (npcDialog == null || !npcDialog.inProgress || baseCharacter.target == null)) {
						motivationStack.Pop ();
						if (logProgress)
							Debug.Log (gameObject.name + " finished " + currentAction);
						actionInProgress = null;
					}
				} else {
					agent.SetDestination (transform.position);
					actionInProgress = currentAction;
					baseCharacter.target = GameSystem.gameSystem.player;
					GetComponent<Rigidbody> ().rotation = Quaternion.LookRotation (baseCharacter.target.transform.position - transform.position, Vector3.up);
					lastActionStart = Time.time;
					if (npcDialog != null && !npcDialog.inProgress) {
						Debug.Log ("start talking");
						npcDialog.InitializeState (GameSystem.gameSystem.player.GetComponent<BasePlayer> ());
					} else
						Debug.LogWarning ("npcDialog is busy");
				}
				break;
			}
		}
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

	private void tryUse(GameObject go) {
		ItemHandler itemHandler = go.GetComponent<ItemHandler> ();
		if (itemHandler != null) {
			itemHandler.use (BaseCharacter.getCharacterFromGameObject (gameObject));
		}
	}

	private bool arrivedAtDestination() {
		return agent.isOnNavMesh && !agent.pathPending && agent.remainingDistance < destinationThreshold;
	}
}
