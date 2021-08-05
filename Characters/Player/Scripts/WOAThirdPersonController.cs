using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;

public class WOAThirdPersonController : MonoBehaviour {

	[System.Serializable]
	public class AdvancedSettings {
		public float checkGroundForwardDistance = 0.15f;
		public float stepUpForce = 0.5f;
		public float groundCheckRadius = 0.95f;
		public bool allowRotationInAir = true;
	}

	[System.Serializable]
	public class PhysicsSettings {
		public float jumpForce = 5;
		public float dragOnGround = 5;
		public float dragInAir = 0;
	}

	[System.Serializable]
	public class CharacterControllerSettings {
		public float jumpSpeed = 5;
		public float stayOnGroundForce = 20;
	}

	[SerializeField] private float walkSpeed = 2;
	[SerializeField] private float runSpeed = 5;
	[SerializeField] private float walkBackwardSpeed = 1;
	[SerializeField] private float acceleration = 4;
	[SerializeField] private float rotationSpeed = 120;
	[SerializeField] private float clickTargetRadius = 100;
	[SerializeField] float followTargetMoved = 1;
	[SerializeField] float followMinDistance = 3;
	[SerializeField] float followRunDistance = 6;
	[SerializeField] float followAcceleration = 1;
	[SerializeField] private bool usePhysics = true;
	[SerializeField] private PhysicsSettings physicsSettings = new PhysicsSettings();
	[SerializeField] private CharacterControllerSettings characterControllerSettings = new CharacterControllerSettings();
	[SerializeField] private AdvancedSettings advancedSettings = new AdvancedSettings();
	private bool isRunning = false;
	private bool jumping = false;
	bool autoFollowTarget = false;
	private bool grounded;
	private float moveForward;
	private float rotate;
	private Rigidbody body;
	private CharacterController cc;
	private CapsuleCollider cCollider;
	private BasePlayer basePlayer;
	private AnimatedHeadLook animatedHeadLook;
	private Animator anim;
	NavMeshAgent agent;
	Vector3 targetDirection;

	void Start () {
		body = GetComponent<Rigidbody> ();
		cc = GetComponent<CharacterController> ();
		cCollider = GetComponent<CapsuleCollider> ();
		anim = GetComponent<Animator> ();
		agent = GetComponent<NavMeshAgent> ();
		agent.updatePosition = false;
		grounded = true;
		moveForward = 0;
		if (body != null && usePhysics) {
			body.constraints = RigidbodyConstraints.FreezeRotation;
		} else
			usePhysics = false;
		basePlayer = BaseCharacter.getCharacterFromGameObject (gameObject) as BasePlayer;
		animatedHeadLook = GetComponent<AnimatedHeadLook> ();
		body.isKinematic = !usePhysics;
		if (!usePhysics) {
			if (body != null)
				body.constraints = (RigidbodyConstraints)((int)RigidbodyConstraints.FreezeRotationX | (int)RigidbodyConstraints.FreezeRotationZ);
		}
	}

	void Update() {
		if (grounded) {
			if (usePhysics)
				body.drag = physicsSettings.dragOnGround;
			float impulsForward = Input.GetAxis ("Vertical");
			isRunning = Input.GetKey (KeyCode.LeftShift);
			if (autoFollowTarget && basePlayer.target != null) {
				float targetMoved;
				targetDirection = basePlayer.target.transform.position - transform.position;
				float followDistance = targetDirection.magnitude;
				if (!agent.pathPending && agent.remainingDistance > agent.stoppingDistance) {
					float currentSpeed = agent.speed;
					if (followDistance < followMinDistance) {
						currentSpeed -= Time.deltaTime * followAcceleration;
					} else if (followDistance > followRunDistance)
						currentSpeed += Time.deltaTime * followAcceleration;
					agent.speed = Mathf.Clamp (currentSpeed, 0, runSpeed);
				}
				targetMoved = Vector3.Distance (agent.destination, basePlayer.target.transform.position);
				if (targetMoved > followTargetMoved) {
					agent.SetDestination (basePlayer.target.transform.position);
					agent.updateRotation = true;
				}
			} else {
				agent.speed = isRunning ? runSpeed : walkSpeed;
			}
			if (Mathf.Abs (impulsForward) > 1e-6f) {
				moveForward = moveForward + impulsForward * Time.deltaTime * acceleration;
			} else {
				moveForward = 0;
			}
			moveForward = Mathf.Clamp (moveForward, -walkBackwardSpeed, isRunning ? runSpeed : walkSpeed);
		}

		if (grounded || advancedSettings.allowRotationInAir) {
			if (autoFollowTarget && basePlayer.target != null && agent == null) {
				float followAngle = Vector3.SignedAngle (transform.forward, targetDirection, transform.up);
				if (Mathf.Abs(followAngle) > 1) {
					rotate = followAngle / 30;
				}
			} else {
				rotate = Input.GetAxis ("Horizontal");
			}
			rotate *= rotationSpeed * Time.deltaTime;
		}

		if (Input.GetButtonUp ("Fire1") && (EventSystem.current == null || !EventSystem.current.IsPointerOverGameObject())) {
			Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
			RaycastHit hit;
			if (Physics.Raycast (ray, out hit, clickTargetRadius, Physics.AllLayers, QueryTriggerInteraction.Collide)) {
				GameObject target = hit.collider.gameObject;
				if (target.GetComponent<BaseCharacter> () != null) {
					basePlayer.target = target;
				} else {
					basePlayer.target = null;
				}
			} else {
				basePlayer.target = null;
			}
		}

		if (Input.GetButtonDown("Jump") && grounded) {
			anim.SetTrigger ("jumping");
			// the force will be triggered by the animation => jump()
		}

		if (basePlayer.target != null) {
			targetDirection = basePlayer.target.transform.position - transform.position;
			if (targetDirection.sqrMagnitude < 1000) {
				Vector3 targetDirectionNorm = targetDirection;
				targetDirectionNorm.Normalize ();
				float dot = Vector3.Dot (targetDirectionNorm, transform.forward);
				if (dot > 0)
					animatedHeadLook.setTargetVector (basePlayer.target.transform.position + AnimatedHeadLook.NPC_LOOK_OFFSET, dot);
				else
					animatedHeadLook.clearTargetVector ();
			}
		}

		if (!usePhysics) {
			if (autoFollowTarget) {
				cc.Move (agent.nextPosition - transform.position);
			} else {
				Vector3 moveVector = transform.forward * moveForward;
				if (jumping) {
					moveVector += Vector3.up * characterControllerSettings.jumpSpeed;
					jumping = false;
				}
				if (!grounded)
					moveVector += characterControllerSettings.stayOnGroundForce * Physics.gravity * Time.deltaTime;
				cc.Move (moveVector * Time.deltaTime);
				transform.Rotate (0, rotate, 0);
				agent.nextPosition = transform.position;
				agent.updateRotation = false;
			}
		}
	}

	public void SetFollowTarget(bool what) {
		autoFollowTarget = what;
	}

	private void GroundCheck() {
		if (usePhysics) {
			RaycastHit hitInfo;
			Vector3 castOrigin = transform.position + transform.up * cCollider.height / 2 + transform.forward * advancedSettings.checkGroundForwardDistance;
			cCollider.enabled = false;
			float distance = cCollider.height / 2 - cCollider.radius;
			float castRadius = cCollider.radius * advancedSettings.groundCheckRadius;
			if (Physics.SphereCast (castOrigin, castRadius, -transform.up, out hitInfo, cCollider.height / 2 + 0.1f, Physics.AllLayers, QueryTriggerInteraction.Ignore)) {
				if (hitInfo.distance < distance) {
					body.AddForce (transform.up * advancedSettings.stepUpForce, ForceMode.VelocityChange);
				}
				grounded = true;
			} else {
				grounded = false;
			}
			cCollider.enabled = true;
		} else
			grounded = cc.isGrounded;
	}

	public void jump() {
		if (usePhysics) {
			body.drag = physicsSettings.dragInAir;
			Vector3 jumpDirection = transform.up + transform.forward * 0.5f;
			jumpDirection.Normalize ();
			body.AddForce (jumpDirection * physicsSettings.jumpForce, ForceMode.VelocityChange);
			grounded = false;
		} else {
			jumping = true;
		}
	}

	void FixedUpdate () {
		GroundCheck ();
		if (usePhysics) {
			body.drag = grounded ? physicsSettings.dragOnGround : physicsSettings.dragInAir;
			float currentVelocity = body.velocity.magnitude;
			float maxSpeed = isRunning ? runSpeed : walkSpeed;
			if (currentVelocity < maxSpeed * 0.9f)
				body.velocity += transform.forward * moveForward * (1 - currentVelocity / maxSpeed);
			if (grounded || advancedSettings.allowRotationInAir)
				body.rotation *= Quaternion.Euler (0, rotate, 0);
		/* } else {
			Vector3 moveVector = transform.forward * moveForward;
			if (jumping) {
				moveVector += Vector3.up * characterControllerSettings.jumpSpeed;
				jumping = false;
			}
			if (!grounded)
				moveVector += characterControllerSettings.stayOnGroundForce * Physics.gravity * Time.fixedDeltaTime;
			cc.Move (moveVector * Time.fixedDeltaTime);
			transform.Rotate (0, rotate, 0);
			*/
		}
	}

	void OnDrawGizmosSelected() {
		CapsuleCollider coll = GetComponent<CapsuleCollider> ();
		Vector3 castOrigin = transform.position + transform.up * coll.height / 2 + transform.forward * advancedSettings.checkGroundForwardDistance;
		Vector3 castEnd = castOrigin - transform.up * (coll.height / 2 + advancedSettings.checkGroundForwardDistance);
		Gizmos.color = Color.cyan;
		Gizmos.DrawWireSphere (castOrigin, coll.radius * advancedSettings.groundCheckRadius);
		Gizmos.DrawWireSphere (castEnd, coll.radius * advancedSettings.groundCheckRadius);
	}
}
