using UnityEngine;

public class AnimatedHeadLook : MonoBehaviour {

	public static Vector3 NPC_LOOK_OFFSET = new Vector3 (0, 1.5f, 0);
	public Transform target;
	public float defaultWeight = 1f;
	public bool targetNearbyCharacter = true;
	Animator anim;
	Vector3 targetVector;
	bool useTargetVector;
	float weight;
	BaseCharacter baseCharacter;

	// Use this for initialization
	void Start () {
		anim = GetComponent<Animator> ();
		weight = defaultWeight;
		baseCharacter = GetComponent<BaseCharacter> ();
		if (baseCharacter == null)
			baseCharacter = transform.parent.gameObject.GetComponent<BaseCharacter> ();
		if (baseCharacter is BasePlayer)
			targetNearbyCharacter = false;
	}

	void Update() {
		target = null;
		if (targetNearbyCharacter && GameSystem.gameSystem.player != null)
			target = GameSystem.gameSystem.player.transform;
		if (baseCharacter != null && baseCharacter.target != null)
			target = baseCharacter.target.transform;
		if (target != null) {
			Vector3 targetDirection = target.position - transform.position;
			if (targetDirection.sqrMagnitude < 1000) {
				Vector3 targetDirectionNorm = targetDirection;
				targetDirectionNorm.Normalize ();
				float dot = Vector3.Dot (targetDirectionNorm, transform.forward);
				if (dot > 0)
					setTargetVector (target.position + AnimatedHeadLook.NPC_LOOK_OFFSET, dot);
				else
					clearTargetVector ();
			}
		} else
			clearTargetVector ();
	}

	public void setTarget(Transform newTarget) {
		target = newTarget;
	}

	public void setTargetVector(Vector3 targetLocation, float weight) {
		useTargetVector = true;
		targetVector = targetLocation;
		this.weight = weight;
	}

	public void clearTargetVector() {
		useTargetVector = false;
		weight = defaultWeight;
	}

	void OnAnimatorIK(int layerIndex) {
		if (useTargetVector) {
			anim.SetLookAtPosition (targetVector);
			anim.SetLookAtWeight(weight);
		}
	}
}
