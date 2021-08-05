using UnityEngine;

public class SpeedMeter : MonoBehaviour {

	Vector3 oldPosition;
	float lastMeasureTime;
	float measureInterval = 1f;

	// Use this for initialization
	void Start () {
		oldPosition = transform.position;
		lastMeasureTime = Time.time;
	}
	
	// Update is called once per frame
	void Update () {
		float elapsed = Time.time - lastMeasureTime;
		if (elapsed >= measureInterval) {
			float walked = Vector3.Distance (transform.position, oldPosition);
			Debug.Log ("speed = " + (walked / elapsed) + " u/sec");
			oldPosition = transform.position;
			lastMeasureTime = Time.time;
		}
	}
}
