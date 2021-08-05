using UnityEngine;

public class PathRecordTrigger : MonoBehaviour {

	public enum RecordAction { Start, Stop }
	public RecordAction recordAction;

	void OnTriggerEnter (Collider other) {
		PathRecorder recorder = other.gameObject.GetComponent<PathRecorder> ();
		if (recorder != null) {
			recorder.record = recordAction == RecordAction.Start;
		}
	}
}
