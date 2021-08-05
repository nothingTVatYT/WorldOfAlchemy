using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraHover : MonoBehaviour {

	[System.Serializable]
	public class FloatRange {
		public float min;
		public float max;
		public FloatRange(float minValue, float maxValue) {
			min = minValue;
			max = maxValue;
		}
		public float average { get { return (max - min) / 2f + min; }}
		public bool outside(float val) {
			return val < min || val > max;
		}
	}

	[SerializeField] private Transform focus;
	[SerializeField] private float distance = 3.5f;
	[SerializeField] private float height = 2f;
	[SerializeField] private float turnSpeed = 20f;

	private Vector3 pos;
	private Camera cam;

	// Use this for initialization
	void Start () {
		pos = focus.position + new Vector3(0f, height, -distance);
		cam = GetComponent<Camera> ();
		transform.position = pos;
	}
	
	// Update is called once per frame
	void Update () {
		transform.RotateAround (focus.position, Vector3.up, turnSpeed * Time.deltaTime);
		cam.transform.LookAt (focus.transform.position);
	}
}
