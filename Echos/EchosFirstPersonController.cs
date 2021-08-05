using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Random = UnityEngine.Random;

namespace global
{
	[RequireComponent (typeof(CharacterController))]
	[RequireComponent (typeof(AudioSource))]
	public class EchosFirstPersonController : MonoBehaviour
	{
		[SerializeField] private bool m_IsWalking;
		[SerializeField] private float m_WalkSpeed;
		[SerializeField] private float m_RunSpeed;
		[SerializeField] [Range (0f, 1f)] private float m_RunstepLenghten;
		[SerializeField] private float m_JumpSpeed;
		[SerializeField] private float m_TurnSpeed;
		[SerializeField] private float m_AscendingForce = 0.01f;
		[SerializeField] private float m_MaxFlightHeight = 200;
		[SerializeField] private float m_TouchRadius;
		[SerializeField] private bool useHorizontalAxisForStrafe = true;
		[SerializeField] [Range (0.01f, 1f)] private float m_CrouchHeight = 0.3f;
		[SerializeField] private GameObject m_UITouchedObject;
		[SerializeField] private float m_StickToGroundForce;
		[SerializeField] private float m_GravityMultiplier;
		[SerializeField] private EchosMouseLook m_MouseLook;
		[SerializeField] private bool m_UseHeadBob;
		[SerializeField] private float m_StepInterval;
		[SerializeField] private AudioClip[] m_FootstepSounds;
		// an array of footstep sounds that will be randomly selected from.
		[SerializeField] private AudioClip m_JumpSound;
		// the sound played when character leaves the ground.
		[SerializeField] private AudioClip m_LandSound;
		// the sound played when character touches back on ground.
		[SerializeField] private GameObject m_UILocation;
		private Camera m_Camera;
		private bool m_Jump;
		private bool m_Fly;
		private float m_YRotation;
		private Vector2 m_Input;
		private Vector3 m_MoveDir = Vector3.zero;
		private CharacterController m_CharacterController;
		private BaseCharacter basePlayer;
		private CollisionFlags m_CollisionFlags;
		private bool m_PreviouslyGrounded;
		private Vector3 m_OriginalCameraPosition;
		private float originalHeight;
		private float m_StepCycle;
		private float m_NextStep;
		private bool m_Jumping;
		private bool m_Flying;
		private bool m_Crouching;
		private float ascending;
		private AudioSource m_AudioSource;

		// Use this for initialization
		private void Start ()
		{
			m_CharacterController = GetComponent<CharacterController> ();
			basePlayer = BaseCharacter.getCharacterFromGameObject (gameObject);
			m_Camera = Camera.main;
			m_OriginalCameraPosition = m_Camera.transform.localPosition;
			m_StepCycle = 0f;
			m_NextStep = m_StepCycle / 2f;
			m_Jumping = false;
			m_Flying = false;
			m_Crouching = false;
			ascending = 0;
			m_AudioSource = GetComponent<AudioSource> ();
			originalHeight = m_CharacterController.height;
		}


		// Update is called once per frame
		private void Update ()
		{
			m_MouseLook.LookRotation (transform, m_Camera.transform);
			// the jump state needs to read here to make sure it is not missed
			if (!m_Jump) {
				m_Jump = Input.GetButtonDown ("Jump");
			}
			if (!m_Fly) {
				m_Fly = Input.GetKeyDown (KeyCode.F);
			}
			if (Input.GetKey (KeyCode.Y)) {
				m_CharacterController.height = originalHeight * m_CrouchHeight;
				m_Crouching = true;
			} else {
				m_CharacterController.height = originalHeight;
				m_Crouching = false;
			}
			if (!m_PreviouslyGrounded && m_CharacterController.isGrounded) {
				PlayLandingSound ();
				m_MoveDir.y = 0f;
				m_Jumping = false;
				m_Flying = false;
			}
			if (!m_CharacterController.isGrounded && !m_Jumping && m_PreviouslyGrounded) {
				m_MoveDir.y = 0f;
			}

			m_PreviouslyGrounded = m_CharacterController.isGrounded;

			if (Input.GetButtonUp ("Fire1") && !EventSystem.current.IsPointerOverGameObject()) {
				Ray ray = m_Camera.ScreenPointToRay (Input.mousePosition);
				RaycastHit hit;
				if (Physics.Raycast (ray, out hit, m_TouchRadius, Physics.AllLayers, QueryTriggerInteraction.Collide)) {
					GameObject target = hit.collider.gameObject;
					if (target.GetComponent<NPCBehaviour> () != null) {
						basePlayer.target = target;
					} else {
						m_UITouchedObject.GetComponent<Text> ().text = string.Format ("{0} ({1:0.##}m)", target.name, hit.distance);
						Interactable interactable = target.GetComponent<Interactable> ();
						if (interactable != null)
							interactable.use (basePlayer);
						else
							basePlayer.target = null;
					}
				}
			}
		}


		private void PlayLandingSound ()
		{
			m_AudioSource.clip = m_LandSound;
			m_AudioSource.Play ();
			m_NextStep = m_StepCycle + .5f;
		}


		private void FixedUpdate ()
		{
			float speed;
			GetInput (out speed);

			Vector3 desiredMove;
			if (useHorizontalAxisForStrafe) {
				desiredMove = transform.forward * m_Input.y + transform.right * m_Input.x;
			} else {
				float m_horizontalRotation = transform.localRotation.eulerAngles.y;
				// turn character on horizontal control (the 180 just keeps the m_TurnSpeed in a reasonable range)
				m_horizontalRotation += 180f * m_Input.x * Time.fixedDeltaTime * m_TurnSpeed;
				transform.localRotation = Quaternion.AngleAxis (m_horizontalRotation, transform.up);

				desiredMove = transform.forward * m_Input.y;
			}

			// get a normal for the surface that is being touched to move along it
			RaycastHit hitInfo;
			Physics.SphereCast (transform.position, m_CharacterController.radius, Vector3.down, out hitInfo,
				m_CharacterController.height / 2f, ~0, QueryTriggerInteraction.Ignore);
			desiredMove = Vector3.ProjectOnPlane (desiredMove, hitInfo.normal).normalized;

			m_MoveDir.x = desiredMove.x * speed;
			m_MoveDir.z = desiredMove.z * speed;


			if (m_CharacterController.isGrounded) {
				m_MoveDir.y = -m_StickToGroundForce;

				if (m_Jump) {
					m_MoveDir.y = m_JumpSpeed;
					PlayJumpSound ();
					m_Jump = false;
					m_Jumping = true;
				}
				if (m_Fly) {
					m_MoveDir.y = m_JumpSpeed + m_CharacterController.velocity.magnitude * m_AscendingForce;
					m_Fly = false;
					m_Flying = true;
				}
			} else {
				if (m_Fly && m_Flying) {
					ascending += m_AscendingForce / 2;
					m_Fly = false;
				}
				m_MoveDir += Physics.gravity * m_GravityMultiplier * Time.fixedDeltaTime;
				if (m_Flying) {
					if (m_Fly) {
						ascending += 1;
						m_Fly = false;
					}
					Vector3 velocity = m_CharacterController.velocity;
					velocity.y = 0;
					velocity = Vector3.Normalize (velocity);
					float lookDir = m_Camera.transform.localEulerAngles.x;
					if (lookDir > 90)
						lookDir -= 360;
					float ascendingForce = (ascending + m_AscendingForce) * velocity.magnitude - lookDir / 90f * m_AscendingForce;
					m_MoveDir.y += ascendingForce * Time.fixedDeltaTime;
					if (ascending > 0)
						ascending -= 1;
					if (transform.position.y >= m_MaxFlightHeight) {
						m_MoveDir.y = -m_StickToGroundForce;
						//ascending = 0;
					}
				}
			}
			m_CollisionFlags = m_CharacterController.Move (m_MoveDir * Time.fixedDeltaTime);

			ProgressStepCycle (speed);
			UpdateCameraPosition (speed);

			m_MouseLook.UpdateCursorLock ();
			if (m_UILocation != null) {
				m_UILocation.GetComponent<Text> ().text = string.Format ("{0}", transform.position);
			}
		}


		private void PlayJumpSound ()
		{
			m_AudioSource.clip = m_JumpSound;
			m_AudioSource.Play ();
		}


		private void ProgressStepCycle (float speed)
		{
			if (m_CharacterController.velocity.sqrMagnitude > 0 && (m_Input.x != 0 || m_Input.y != 0)) {
				m_StepCycle += (m_CharacterController.velocity.magnitude + (speed * (m_IsWalking ? 1f : m_RunstepLenghten))) *
				Time.fixedDeltaTime;
			}

			if (!(m_StepCycle > m_NextStep)) {
				return;
			}

			m_NextStep = m_StepCycle + m_StepInterval;

			PlayFootStepAudio ();
		}


		private void PlayFootStepAudio ()
		{
			if (!m_CharacterController.isGrounded) {
				return;
			}
			// pick & play a random footstep sound from the array,
			// excluding sound at index 0
			int n = Random.Range (1, m_FootstepSounds.Length);
			m_AudioSource.clip = m_FootstepSounds [n];
			m_AudioSource.PlayOneShot (m_AudioSource.clip);
			// move picked sound to index 0 so it's not picked next time
			m_FootstepSounds [n] = m_FootstepSounds [0];
			m_FootstepSounds [0] = m_AudioSource.clip;
		}


		private void UpdateCameraPosition (float speed)
		{
			Vector3 newCameraPosition;
			if (!m_UseHeadBob) {
				return;
			}
			newCameraPosition = m_Camera.transform.localPosition;
			if (m_Crouching)
				newCameraPosition.y -= (1f - m_CrouchHeight) / 4;
			m_Camera.transform.localPosition = newCameraPosition;
		}


		private void GetInput (out float speed)
		{
			// Read input
			float horizontal = Input.GetAxis ("Horizontal");
			float vertical = Input.GetAxis ("Vertical");

			bool waswalking = m_IsWalking;

			#if !MOBILE_INPUT
			// On standalone builds, walk/run speed is modified by a key press.
			// keep track of whether or not the character is walking or running
			m_IsWalking = !Input.GetKey (KeyCode.LeftShift);
			#endif
			if (!m_IsWalking && m_Crouching)
				m_IsWalking = true;
			// set the desired speed to be walking or running
			speed = m_Flying ? m_RunSpeed : (m_IsWalking ? (m_Crouching ? m_WalkSpeed / 2 : m_WalkSpeed) : m_RunSpeed);
			m_Input = new Vector2 (horizontal, vertical);

			// normalize input if it exceeds 1 in combined length:
			if (m_Input.sqrMagnitude > 1) {
				m_Input.Normalize ();
			}
		}


		private void OnControllerColliderHit (ControllerColliderHit hit)
		{
			Rigidbody body = hit.collider.attachedRigidbody;
			//dont move the rigidbody if the character is on top of it
			if (m_CollisionFlags == CollisionFlags.Below) {
				return;
			}

			if (body == null || body.isKinematic) {
				return;
			}
			body.AddForceAtPosition (m_CharacterController.velocity * 0.1f, hit.point, ForceMode.Impulse);
		}
	}
}
