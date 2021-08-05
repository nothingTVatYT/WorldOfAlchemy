using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Minimap : MonoBehaviour {

	public GameObject player;
	public bool rotateWithPlayer = false;
	private Vector3 pos;
	private Camera miniCamera;

	void Start() {
		miniCamera = GetComponent<Camera> ();
	}

	// Update is called once per frame
	void LateUpdate () {
		pos = transform.position;
		pos.x = player.transform.position.x;
		pos.z = player.transform.position.z;
		transform.position = pos;
		if (rotateWithPlayer) {
			transform.rotation = Quaternion.Euler (90f, player.transform.rotation.eulerAngles.y, 0f);
		}
	}

	public void onScaleChange(float val) {
		miniCamera.orthographicSize = val;
	}
}
