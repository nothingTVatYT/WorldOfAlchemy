using System;
using UnityEngine;
using UnityEngine.AI;
using ECM.Controllers;
using ECM.Common;
using ECM.Components;

[RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider), typeof(GroundDetection))]
[RequireComponent(typeof(CharacterMovement), typeof(NavMeshAgent))]
public class WOAPlayerController : BaseAgentController
{
	Transform followTarget;
	[Header("Normal Movement")]
	public float walkSpeed = 1.5f;
	public float runSpeed = 5f;
	public float crouchSpeed = 0.5f;
	public float animationWalkSpeed = 1.35f;
	public float animationRunSpeed = 4.95f;
	public float animationCrouchSpeed = 0.36f;
	public float crouchColliderHeight = 0.9f;
	public Vector3 crouchColliderCenter = new Vector3(0, 0.45f, 0);
	[Range(-10f, 10f)]
	public float platformRotationFactor = 1f;
	public GameObject crossHair;
	bool walk = true;
	bool mouseTurning;
	bool crouch;
	float turning;
	float followSpeed = 1.5f;
	float followTargetSet;
	CapsuleCollider capsuleCollider;
	float capsuleColliderInitialHeight;
	Vector3 capsuleColliderInitialCenter;
	PlayerCamera playerCamera;
	public bool isFollowing { get { return followTarget != null; }}

	public override void Awake() {
		base.Awake ();
		capsuleCollider = GetComponent<CapsuleCollider> ();
		capsuleColliderInitialHeight = capsuleCollider.height;
		capsuleColliderInitialCenter = capsuleCollider.center;
		playerCamera = GetComponentInChildren<PlayerCamera> ();
	}

	protected override void HandleInput()
	{
		if (followTarget != null) {
			if (Time.time - followTargetSet > 1) {
				agent.SetDestination (followTarget.position);
				followTargetSet = Time.time;
			}
			float dist = Vector3.Distance (followTarget.position, transform.position);
			if (dist > 6 && followSpeed < runSpeed)
				followSpeed += 0.1f;
			else if (dist < 5 && followSpeed > animationWalkSpeed / 2)
				followSpeed -= 0.1f;

			if (Input.GetKeyDown(KeyCode.U)) {
				Follow (null);
			}
		} else {
            //moveDirection = transform.TransformDirection(new Vector3
            //{
            //	x = Input.GetAxisRaw("Horizontal"),
            //	y = 0.0f,
            //	// as long as there is no backward animation
            //	z = Mathf.Clamp(Input.GetAxisRaw("Vertical"), 0, float.MaxValue)
            //	});

		    
		    //
		    // NEW CODE

			// Cheat until there is support for stairs made of steps
			if (Input.GetKeyDown(KeyCode.P)) {
				RaycastHit hit;
				if (!Physics.SphereCast(transform.position + new Vector3(0, 0.9f, 0), 0.6f, transform.forward, out hit, 0.6f)) {
					Rigidbody b = GetComponentInChildren<Rigidbody> ();
					Vector3 pos = b.transform.position + transform.forward * 0.1f;
					pos.y += 0.3f;
					b.transform.position = pos;
				}
			}

			if (Input.GetKeyDown (KeyCode.Y)) {
				if (!crouch) {
					crouch = true;
					capsuleCollider.height = crouchColliderHeight;
					capsuleCollider.center = crouchColliderCenter;
				}
			} else if (Input.GetKeyUp (KeyCode.Y)) {
				if (crouch) {
					crouch = false;
					capsuleCollider.height = capsuleColliderInitialHeight;
					capsuleCollider.center = capsuleColliderInitialCenter;
				}
			}

            moveDirection = new Vector3
		    {
				x = mouseTurning ? Input.GetAxisRaw("Horizontal") : 0.0f,
		        y = 0.0f,
				z = Input.GetAxisRaw("Vertical")
		    };

		    // Tansform move direction to be relative to character's current orientation

		    moveDirection = transform.TransformDirection(moveDirection);
            
            // Rotate the character inplace, we modify its rotation directly using the horizontal axis,
            // however you can use here a mouse horizontal movement, aling with camera view direction etc.

            // Worth note that for this to work, we need to override the Update method, because by default,
            // ECM rotates the character towards the given desired direction in the Update method
            // using the supplied RotateTowardsMoveDirection function

			if (!mouseTurning) {
				turning = Input.GetAxisRaw ("Horizontal") * angularSpeed * Time.deltaTime;
			} else {
				turning = Input.GetAxis ("Mouse X") * angularSpeed * Time.deltaTime;
			}
            movement.rotation *= Quaternion.Euler(0f, turning, 0f);

            // END OF NEW CODE

            jump = Input.GetButton("Jump");
			walk = !Input.GetButton("Fire3");

			if (Input.GetKeyDown (KeyCode.Tab)) {
				ToggleMouseTurning ();
			}
		}
	}

	void ToggleMouseTurning() {
		if (mouseTurning) {
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
			mouseTurning = false;
		} else {
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
			mouseTurning = true;
		}
		if (playerCamera != null)
			playerCamera.mouseTurning = mouseTurning;
		if (crossHair != null)
			crossHair.SetActive(mouseTurning);
	}

	float GetTargetSpeed()
	{
		if (followTarget != null)
			return followSpeed;
		if (crouch)
			return crouchSpeed;
		return walk ? walkSpeed : runSpeed;
	}

	/// <summary>
	/// Overrides 'BaseCharacterController' CalcDesiredVelocity method to handle different speeds,
	/// eg: running, walking, etc.
	/// </summary>

	protected override Vector3 CalcDesiredVelocity()
	{
		if (followTarget != null) {
			SetMoveDirection();

			var desiredVelocity = base.CalcDesiredVelocity();
			return autoBraking ? desiredVelocity * brakingRatio : desiredVelocity;
		}
		speed = GetTargetSpeed();

		return base.CalcDesiredVelocity();
	}

	protected override void SetMoveDirection()
	{
		// If agent is not moving, return
		if (followTarget != null) {
			moveDirection = Vector3.zero;

			if (!agent.hasPath)
				return;

			// If destination not reached,
			// feed agent's desired velocity (lateral only) as the character move direction

			if (agent.remainingDistance > stoppingDistance) {
				moveDirection = agent.desiredVelocity.onlyXZ ();
				speed = GetTargetSpeed ();
			} else {
				// If destination is reached,
				// reset stop agent and clear its path

				//agent.ResetPath ();

				// wait a bit
				speed = 0;
			}
		}
	}

	public void Follow(Transform target) {
		followTarget = target;
	}

	protected override void Animate()
	{
		// If no animator, return

		if (animator == null)
			return;

		// Compute move vector in local space

		var move = transform.InverseTransformDirection(moveDirection);

		// Update the animator parameters

		float forwardSpeed = movement.forwardSpeed;

		var forwardAmount = animator.applyRootMotion
			? move.z * brakingRatio * (walk ? 0.5f : 1f)
			: (forwardSpeed < 0 
				? -Mathf.InverseLerp(0.0f, animationWalkSpeed, -forwardSpeed)
				: Mathf.InverseLerp(0.0f, crouch ? animationCrouchSpeed : animationRunSpeed, forwardSpeed));

		float animationSpeed;
		float turn = turning;
		//Debug.Log ("forwardspeed=" + forwardSpeed);
		if (forwardAmount <= 0.1f || animator.applyRootMotion || !movement.isGrounded || Mathf.Abs(turn) > 0.1f) {
			animationSpeed = 1;
		} else if (crouch) {
			animationSpeed = forwardSpeed / animationCrouchSpeed;
		} else if (forwardAmount < 0.5f) {
			animationSpeed = forwardSpeed / animationWalkSpeed;
		} else {
			animationSpeed = forwardSpeed / animationRunSpeed;
		}
		animator.SetFloat ("AnimationSpeed", animationSpeed);

		animator.SetFloat("Forward", forwardAmount, 0.1f, Time.deltaTime);
		animator.SetFloat("Turn", turn, 0.1f, Time.deltaTime);

		animator.SetBool("OnGround", movement.isGrounded);
		animator.SetBool("Crouch", crouch);

		if (!movement.isGrounded)
			animator.SetFloat("Jump", movement.velocity.y, 0.1f, Time.deltaTime);

		var runCycle = Mathf.Repeat(animator.GetCurrentAnimatorStateInfo(0).normalizedTime + 0.2f, 1.0f);
		var jumpLeg = (runCycle < 0.5f ? 1.0f : -1.0f) * forwardAmount;

		if (movement.isGrounded)
			animator.SetFloat("JumpLeg", jumpLeg);
	}

    // NEW CODE

    public override void Update()
    {
        // Handle input

        HandleInput();
        
        // Rotate towards movement direction (only in agent controlled mode)
		if (followTarget != null)
        	RotateTowardsMoveDirection();


		Collider groundCollider = movement.groundCollider;
		if (groundCollider != null && groundCollider.attachedRigidbody != null) {
			float rotY = groundCollider.attachedRigidbody.angularVelocity.y;
			movement.rotation *= Quaternion.Euler (0f, rotY * platformRotationFactor, 0f);
		}

        // Perform character animation

        Animate();
    }

    // END NEW CODE
}

