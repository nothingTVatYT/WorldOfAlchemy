using UnityEngine;

public class ChestOpener : MonoBehaviour {

	public enum State { Open, Closed, Moving}
	public Vector3 rotationClosed;
	public Vector3 rotationOpened;
	public float openSpeed;
	public State initialState = State.Closed;
	public ChestOpener blockedByDoor;
	public AudioClip doorOpenSound;
	public AudioClip doorCloseSound;
	public string keyPattern = "";
	private State state;
	private State requestedState;
	private int direction;
	private Quaternion closed;
	private Quaternion opened;
	private float timeStartMoving;
	private bool soundPlayed;
	private AudioSource audioSource;
	private string key = "";

	// Use this for initialization
	void Start () {
		opened = Quaternion.Euler (rotationOpened);
		closed = Quaternion.Euler (rotationClosed);
		transform.localRotation = initialState == State.Closed ? closed : opened;
		state = initialState;
		audioSource = GetComponent<AudioSource> ();
		soundPlayed = false;
		key = keyPattern;
		if (!keyPattern.Equals("")) {
			key = keyPattern.Replace ("%d", "" + transform.GetSiblingIndex()).Replace("%p", "" + transform.parent.GetSiblingIndex()).Replace("%g", "" + transform.parent.parent.GetSiblingIndex());
		}
	}
	
	void Update () {
		if (state == State.Moving) {
			float factor = (Time.time - timeStartMoving) / openSpeed;
			if (factor >= 1f) {
				factor = 1f;
				state = requestedState;
			}
			if (!soundPlayed && audioSource != null && doorOpenSound != null && doorCloseSound != null) {
				audioSource.PlayOneShot (requestedState == State.Open ? doorOpenSound : doorCloseSound);
				soundPlayed = true;
			}
			transform.localRotation = Quaternion.Lerp (closed, opened, requestedState == State.Closed ? (1f-factor) : factor);
		}
	}

	public bool isOpen() {
		return state == State.Open;
	}

	public void OnMouseDown() {
		if (GameSystem.gameSystem.playerCanReach (transform)) {
			if (state != State.Moving) {
				if (blockedByDoor != null && !blockedByDoor.isOpen ())
					return;
				if (key != "" && state == State.Closed) {
					if (!GameSystem.gameSystem.player.GetComponent<BasePlayer>().canUnlock (key)) {
						GameConsole.gameConsole.println (GameConsole.InfoLevel.General, "You need a key to unlock this door. (" + key + ")");
						return;
					}
				}
				requestedState = (state == State.Closed) ? State.Open : State.Closed;
				state = State.Moving;
				timeStartMoving = Time.time;
				soundPlayed = false;
			}
		}
	}

	public void OnTriggerEnter(Collider other) {
		IMotivationController nc = other.gameObject.GetComponent<IMotivationController> ();
		if (nc != null && state == State.Closed) {
			requestedState = State.Open;
			state = State.Moving;
			timeStartMoving = Time.time;
			soundPlayed = false;
		}
	}
}
