using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class FeetCheck : MonoBehaviour {

	[SerializeField] private Transform leftFoot;
	[SerializeField] private Transform rightFoot;
	[SerializeField] private KeyCode recordKey = KeyCode.R;
	private List<Vector3> offsetsLeft = new List<Vector3> ();
	private List<Vector3> offsetsRight = new List<Vector3> ();
	private List<float> timesLeft = new List<float> ();
	private List<float> timesRight = new List<float> ();
	private Vector3 startLocationLeft;
	private Vector3 startLocationRight;
	private bool recordLeftFoot = false;
	private bool recordRightFoot = false;
	private bool recordingLeft = false;
	private bool recordingRight = false;
	private int index = 0;

	void Update () {
		if (Input.GetKeyDown (recordKey)) {
			recordingLeft = true;
			recordingRight = true;
		}
	}

	void FixedUpdate() {
		if (recordLeftFoot) {
			offsetsLeft.Add (leftFoot.position);
			timesLeft.Add(Time.fixedTime);
		}
		if (recordRightFoot) {
			offsetsRight.Add (rightFoot.position);
			timesRight.Add(Time.fixedTime);
		}
	}

	public void startRecordingLeftFoot() {
		if (recordingLeft) {
			recordLeftFoot = true;
			startLocationLeft = transform.position;
		}
	}

	public void stopRecordingLeftFoot() {
		if (recordingLeft && recordLeftFoot) {
			recordLeftFoot = false;
			recordingLeft = false;
			saveRecording (leftFoot.gameObject.name + "-left-" + index++, offsetsLeft, timesLeft, startLocationLeft);
			offsetsLeft.Clear ();
			timesLeft.Clear ();
		}
	}

	public void startRecordingRightFoot() {
		if (recordingRight) {
			recordRightFoot = true;
			startLocationRight = transform.position;
		}
	}

	public void stopRecordingRightFoot() {
		if (recordingRight && recordRightFoot) {
			recordRightFoot = false;
			recordingRight = false;
			saveRecording (rightFoot.gameObject.name + "-right-" + index++, offsetsRight, timesRight, startLocationRight);
			offsetsRight.Clear ();
			timesRight.Clear ();
		}
	}

	private void saveRecording(string name, List<Vector3> vectors, List<float> times, Vector3 startLocation) {
		if (vectors.Count > 0) {
			RecordedFeetOffsets recording = ScriptableObject.CreateInstance<RecordedFeetOffsets> ();
			Vector3 average = Vector3.zero;
			Vector3 walked = transform.position - startLocation;
			float timePassed = times [times.Count - 1] - times[0];
			recording.offsets = new Vector3[vectors.Count];
			recording.timestamps = new float[times.Count];
			Vector3 first = transform.InverseTransformPoint(vectors[0]);
			for (int i = 0; i < vectors.Count; i++) {
				recording.offsets [i] = first - transform.InverseTransformPoint (vectors [i]);
				recording.timestamps [i] = times [i] - times [0];
				average += recording.offsets [i];
			}
			recording.characterMoved = walked.magnitude;
			recording.animationMoved = recording.characterMoved + recording.offsets [recording.offsets.Length - 1].z;
			recording.animationSpeedMultiplier = recording.characterMoved / recording.animationMoved;
			recording.average = average / vectors.Count;
			recording.characterSpeed = recording.characterMoved / timePassed;
			AssetDatabase.CreateAsset (recording, "Assets/" + name + ".asset");
			Debug.Log ("recorded offsets saved to " + name);
		}
	}

	void OnDestroy() {
		AssetDatabase.SaveAssets();
	}
}
