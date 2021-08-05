using UnityEngine;

public class PathRecorder : MonoBehaviour {

	public RecordedLocations recorder;
	public float recordInterval = 5;
	public bool record;
	Vector3 lastRecorded;

	void Update() {
		if (record) {
			float dist = Vector3.Distance (transform.position, lastRecorded);
			if (dist > recordInterval) {
				recorder.addLocation (transform);
				lastRecorded = transform.position;
			}
		}
	}

	void OnValidate() {
		if (recordInterval <= 1)
			recordInterval = 1;
	}
}
