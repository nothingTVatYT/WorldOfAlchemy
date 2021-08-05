using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WagonController : MonoBehaviour {

	public Transform drawbar;
	public Transform front;
	[Range(0,1)]
	public float drawbarUp = 0f;
	[Range(-1,1)]
	public float frontRotation = 0f;
	private float drawbarMinX = -11.9f;
	private float drawbarMaxX = 45;
	private float frontMaxAngle = 38f;
	private float frontRotYOffset = 180;
	private Vector3 drawbarRot;
	private Vector3 frontRot;

	// Use this for initialization
	void Start () {
		drawbarRot = drawbar.localRotation.eulerAngles;
		frontRot = front.localRotation.eulerAngles;
		//Debug.Log ("local drawbar rotation: " + drawbarRot);
		//Debug.Log ("local front rotation: " + frontRot);
	}
	
	// Update is called once per frame
	void Update () {
		drawbarRot.x = drawbarMinX + drawbarUp * (drawbarMaxX - drawbarMinX);
		drawbar.localRotation = Quaternion.Euler (drawbarRot);
		frontRot.y = frontRotation * frontMaxAngle + frontRotYOffset;
		front.localRotation = Quaternion.Euler (frontRot);
	}
}
