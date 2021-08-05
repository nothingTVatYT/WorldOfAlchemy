using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerCamera : MonoBehaviour
{

	const string playerCameraDistance = "playercamera.distance";

	[Range (0, 10)]
	public float distance = 1.5f;
	[Range (0, 1)]
	public float forwardOffset = 0.15f;
	[Range (0, 1)]
	public float minHeight = 0.9f;
	[Range (0, 1)]
	public float minDistanceFromWall = 0.1f;
	public float scrollSpeed = 50;
	public float cameraRotationSpeed = 60;
	public float snapDistance = 0.3f;
	public float fpsDistance = 0.16f;
	GameObject player;
	CapsuleCollider playerCollider;
	Vector3 focusPosition;
	Vector3 initialLocalPosition;
	Vector3 localPositionBeforeFPS;
	Vector3 localRotationBeforeFPS;
	float effectiveDistance;
	Ray lineOfSight = new Ray();
	Vector3 direction;
	bool weLockedCursor;
	bool wasRotating;
	bool fpsMode;
	public bool mouseTurning;
	float totalRotateX;
	float totalRotateY;
	float previousPlayerColliderHeight;

	void Start ()
	{
		player = transform.parent.gameObject;
		playerCollider = player.GetComponent<CapsuleCollider> ();
		previousPlayerColliderHeight = playerCollider.height;
		initialLocalPosition = transform.localPosition;
		if (PlayerPrefs.HasKey (playerCameraDistance))
			distance = PlayerPrefs.GetFloat (playerCameraDistance);
		weLockedCursor = false;
		fpsMode = distance <= snapDistance;
		if (fpsMode) {
			// if we start in FPS mode there are no values before FPS mode, so make them up
			localPositionBeforeFPS = new Vector3 (0, minHeight * playerCollider.height, -0.5f);
			localRotationBeforeFPS = Vector3.zero;
		}
	}

	void Update ()
	{
		if (!EventSystem.current.IsPointerOverGameObject()) {
			float scrolled = Input.GetAxis ("Mouse ScrollWheel");
			distance += scrolled * scrollSpeed * Time.deltaTime;
			distance = Mathf.Clamp (distance, 0.14f, 10f);

			if (!fpsMode) {
				if (Input.GetMouseButton (2) || Input.GetKey (KeyCode.LeftControl)) {
					ConsumeCursor (true);
					float rotateX = Input.GetAxis ("Mouse X");
					float rotateY = Input.GetAxis ("Mouse Y");
					totalRotateX += rotateX;
					totalRotateY += rotateY;
					transform.RotateAround (focusPosition, Vector3.right, -rotateY * cameraRotationSpeed * Time.deltaTime);
					transform.RotateAround (focusPosition, Vector3.up, rotateX * cameraRotationSpeed * Time.deltaTime);
					transform.LookAt (focusPosition);
					wasRotating = true;
				} else {
					if (weLockedCursor)
						ConsumeCursor (false);
					if (wasRotating) {
						// snap back to default position if button/key was pressed and released without moving the mouse
						if (Mathf.Abs (totalRotateX) < Mathf.Epsilon && Mathf.Abs (totalRotateY) < Mathf.Epsilon) {
							transform.localPosition = initialLocalPosition;
						}
						totalRotateX = 0;
						totalRotateY = 0;
						wasRotating = false;
					}
				}
			} else {
				if (Input.GetMouseButton (2) || Input.GetKey (KeyCode.LeftControl) || mouseTurning) {
					ConsumeCursor (true);
					float rotateY = Input.GetAxis ("Mouse Y");
					totalRotateY += rotateY * cameraRotationSpeed * Time.deltaTime;
					Vector3 localCamRot = transform.localRotation.eulerAngles;
					localCamRot.x -= rotateY * cameraRotationSpeed * Time.deltaTime;
					if (localCamRot.x > 180)
						localCamRot.x -= 360;
					localCamRot.x = Mathf.Clamp (localCamRot.x, -90f, 90f);
					transform.localRotation = Quaternion.Euler (localCamRot);
					wasRotating = true;
				} else {
					if (weLockedCursor)
						ConsumeCursor (false);
					if (wasRotating) {
						// snap back to default position if button/key was pressed and released without moving the mouse
						if (Mathf.Abs (totalRotateX) < Mathf.Epsilon && Mathf.Abs (totalRotateY) < Mathf.Epsilon) {
							transform.localRotation = Quaternion.identity;
						}
						totalRotateX = 0;
						totalRotateY = 0;
						wasRotating = false;
					}
				}

			}
		}
	}

	void OnDestroy ()
	{
		PlayerPrefs.SetFloat (playerCameraDistance, distance);
	}

	void LateUpdate ()
	{
		effectiveDistance = distance;
		lineOfSight.origin = player.transform.TransformPoint (0, minHeight * playerCollider.height, 0);
		if (!fpsMode) {
			direction = transform.position - lineOfSight.origin;
			lineOfSight.direction = direction;
			RaycastHit hitInfo;
			if (Physics.Raycast (lineOfSight, out hitInfo, distance, Physics.AllLayers, QueryTriggerInteraction.Ignore)) {
				effectiveDistance = hitInfo.distance - minDistanceFromWall;
			}
			transform.position = lineOfSight.origin + direction / direction.magnitude * effectiveDistance;
		}
		if (effectiveDistance <= snapDistance) {
			if (!fpsMode) {
				// save the current local position and rotation and set the camera to defaults
				localPositionBeforeFPS = transform.localPosition;
				localRotationBeforeFPS = transform.localRotation.eulerAngles;
				transform.position = lineOfSight.origin + player.transform.forward * fpsDistance;
				transform.localRotation = Quaternion.identity;
				fpsMode = true;
			}
		} else {
			if (fpsMode) {
				// restore the state before we entered FPS mode
				transform.localPosition = localPositionBeforeFPS;
				transform.localRotation = Quaternion.Euler (localRotationBeforeFPS);
				fpsMode = false;
			}
		}
		// adapt the camera height when crouching
		if (fpsMode && Mathf.Abs(playerCollider.height - previousPlayerColliderHeight) > Mathf.Epsilon) {
			transform.position = lineOfSight.origin + player.transform.forward * fpsDistance;
			previousPlayerColliderHeight = playerCollider.height;
		}
		focusPosition = player.transform.TransformPoint (new Vector3 (0, minHeight * playerCollider.height, forwardOffset));
	}

	void ConsumeCursor (bool lockIt)
	{
		if (lockIt && Cursor.visible) {
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
			weLockedCursor = true;
		} else if (!lockIt && weLockedCursor) {
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
			weLockedCursor = false;
		}
	}
}
