using ECM.Common;
using ECM.Controllers;
using ECM.Helpers;
using UnityEngine;

namespace ECM.Examples
{
    /// <summary>
    /// 
    /// Example Character Controller
    /// 
    /// This example shows how to extend the 'BaseCharacterController' adding support for different
    /// character speeds (eg: walking, running, etc), plus how to handle custom input extending the
    /// HandleInput method and make the movement relative to camera view direction.
    /// 
    /// </summary>

    public sealed class CustomCharacterController : BaseCharacterController, IPlayerController
    {
        #region EDITOR EXPOSED FIELDS

        [Header("CUSTOM CONTROLLER")]
        //[Tooltip("The character's follow camera.")]
        //public Transform playerCamera;

        [Tooltip("The character's walk speed.")]
        [SerializeField]
        private float _walkSpeed = 2.5f;

        [Tooltip("The character's run speed.")]
        [SerializeField]
        private float _runSpeed = 5.0f;

        [SerializeField]
        private float _turnSpeed = 60;
		[Range(-10f, 10f)]
		public float platformRotationFactor = 2f;
		public GameObject crossHair;
		public float crouchColliderHeight = 0.9f;
		public Vector3 crouchColliderCenter = new Vector3(0, 0.45f, 0);

		bool mouseTurning;
		CapsuleCollider capsuleCollider;
		float capsuleColliderInitialHeight;
		Vector3 capsuleColliderInitialCenter;
		PlayerCamera playerCamera;
        Transform followTarget;

        #endregion

        #region PROPERTIES

        /// <summary>
        /// The character's walk speed.
        /// </summary>

        public float walkSpeed
        {
            get { return _walkSpeed; }
            set { _walkSpeed = Mathf.Max(0.0f, value); }
        }

        /// <summary>
        /// The character's run speed.
        /// </summary>

        public float runSpeed
        {
            get { return _runSpeed; }
            set { _runSpeed = Mathf.Max(0.0f, value); }
        }

        /// <summary>
        /// Walk input command.
        /// </summary>

        public bool walk { get; private set; }
        public bool crouch { get; private set; }
        public bool strafe { get; private set; }
        public bool backward { get; private set; }
        public bool sitting { get; private set; }
        public bool pushing { get; private set; }

        bool IPlayerController.isFollowing
        {
            get
            {
                return followTarget != null;
            }
        }

        #endregion

        #region METHODS

        public override void Awake() {
			base.Awake ();
			capsuleCollider = GetComponent<CapsuleCollider> ();
			capsuleColliderInitialHeight = capsuleCollider.height;
			capsuleColliderInitialCenter = capsuleCollider.center;
			playerCamera = GetComponentInChildren<PlayerCamera> ();
		}

        /// <summary>
        /// Get target speed based on character state (eg: running, walking, etc).
        /// </summary>

        private float GetTargetSpeed()
        {
            angularSpeed = walk ? _turnSpeed : (_turnSpeed * 2);
            return walk ? walkSpeed : runSpeed;
        }

        /// <summary>
        /// Overrides 'BaseCharacterController' CalcDesiredVelocity method to handle different speeds,
        /// eg: running, walking, etc.
        /// </summary>

        protected override Vector3 CalcDesiredVelocity()
        {
            if (animator == null || rootMotionController == null)
                return Vector3.zero;

            // Set 'BaseCharacterController' speed property based on this character state

            speed = GetTargetSpeed();

            // Return desired velocity vector

            return base.CalcDesiredVelocity();
        }

        /// <summary>
        /// Overrides 'BaseCharacterController' Animate method.
        /// 
        /// This shows how to handle your characters' animation states using the Animate method.
        /// The use of this method is optional, for example you can use a separate script to manage your
        /// animations completely separate of movement controller.
        /// 
        /// </summary>

        protected override void Animate()
        {
            // If no animator, return

            if (animator == null)
                return;

            // Compute move vector in local space

            var move = transform.InverseTransformDirection(moveDirection);

            // Update the animator parameters

            var forwardAmount = animator.applyRootMotion
                ? move.z * (walk ? 0.5f : 1f)
                : Mathf.InverseLerp(0.0f, runSpeed, movement.forwardSpeed);

            backward = forwardAmount < -0.1f;

            float turn = 0f;
            if (Mathf.Abs(move.x) > 0.1f)
                turn = Mathf.Sign(move.x) * (walk ? 0.5f : 1f);

            animator.SetFloat("Forward", forwardAmount, 0.1f, Time.deltaTime);
            //animator.SetFloat("Turn", Mathf.Atan2(move.x, move.z) / 1.6f, 0.1f, Time.deltaTime);
            animator.SetFloat("Turn", turn, 0.1f, Time.deltaTime);

            animator.SetBool("OnGround", movement.isGrounded);
            animator.SetBool("Crouch", crouch);
            animator.SetBool("Strafe", strafe);
            animator.SetBool("Sitting", sitting);
            animator.SetBool("Pushing", pushing);

            if (!movement.isGrounded)
                animator.SetFloat("Jump", movement.velocity.y, 0.1f, Time.deltaTime);
        }

        /// <summary>
        /// Overrides 'BaseCharacterController' HandleInput,
        /// to perform custom controller input.
        /// </summary>

        protected override void HandleInput()
        {
            // Handle your custom input here...

            float strafeX = Input.GetAxisRaw("Strafe");
			float turnX; // = Input.GetAxisRaw("Horizontal");
            float prioritizedX;

			if (!mouseTurning) {
				turnX = Input.GetAxisRaw ("Horizontal"); // * angularSpeed * Time.deltaTime;
			} else {
				turnX = Input.GetAxis ("Mouse X"); // * angularSpeed * Time.deltaTime;
			}

			if (Mathf.Abs(strafeX) > 0.01f) {
                prioritizedX = strafeX;
                strafe = true;
            } else {
                prioritizedX = turnX;
                strafe = false;
            }

            moveDirection = new Vector3
            {
                x = prioritizedX,
                y = 0.0f,
                z = Input.GetAxisRaw("Vertical")
            };

            walk = !Input.GetButton("Fire3");

            jump = Input.GetButton("Jump");

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

			if (Input.GetKeyDown(KeyCode.Hash))
                animator.SetTrigger("AttackMagic1");
            if (Input.GetKeyDown(KeyCode.X))
                sitting = !sitting;
            pushing = Input.GetKey(KeyCode.Alpha0);

            // Transform moveDirection vector to be relative to camera view direction

			moveDirection = transform.TransformDirection(moveDirection);
			//moveDirection = moveDirection.relativeTo(playerCamera);
            if (followTarget != null && moveDirection.magnitude < 1e-3f) {
                moveDirection = followTarget.position - transform.position;
            }

			if (Input.GetKeyDown (KeyCode.Tab)) {
				ToggleMouseTurning ();
			}
            if (Input.GetKeyDown(KeyCode.U))
                Follow(null);
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

        public override void Update() {
            HandleInput();

            // Rotate towards movement direction (input)

            if (!strafe && !backward)
                RotateTowardsMoveDirection();

			Collider groundCollider = movement.groundCollider;
			if (groundCollider != null && groundCollider.attachedRigidbody != null) {
				float rotY = groundCollider.attachedRigidbody.angularVelocity.y;
				movement.rotation *= Quaternion.Euler (0f, rotY * platformRotationFactor, 0f);
			}

            // Perform character animation

            Animate();
        }

        #endregion

        #region MONOBEHAVIOUR

        /// <summary>
        /// Overrides 'BaseCharacterController' OnValidate method,
        /// to perform this class editor exposed fields validation.
        /// </summary>

        public override void OnValidate()
        {
            // Validate 'BaseCharacterController' editor exposed fields

            base.OnValidate();

            // Validate this editor exposed fields

            walkSpeed = _walkSpeed;
            runSpeed = _runSpeed;
        }

        public void Follow(Transform target)
        {
            followTarget = target;
        }

        #endregion
    }
}
