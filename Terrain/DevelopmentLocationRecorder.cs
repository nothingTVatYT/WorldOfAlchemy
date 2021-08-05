using System;
using UnityEngine;

public class DevelopmentLocationRecorder : MonoBehaviour
{
	public RecordedLocations recorded;
	public RecordedLocation lastLocation;

	void Update() {
		if (Input.GetKeyDown(KeyCode.Plus)) {
			Debug.Log ("record this location");
			recorded.addLocation (gameObject.transform);
		}
		if (lastLocation != null) {
			lastLocation.position = gameObject.transform.position;
			lastLocation.rotation = gameObject.transform.rotation.eulerAngles;
		}
	}

	public void teleportToLastPosition() {
		if (lastLocation != null) {
			gameObject.transform.position = lastLocation.position;
			gameObject.transform.rotation = Quaternion.Euler (lastLocation.rotation);
		}
	}
}

