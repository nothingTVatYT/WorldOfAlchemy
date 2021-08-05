using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bridge : MonoBehaviour {

	public GameObject anchor1;
	public GameObject anchor2;
	public LineRenderer lineRenderer;
	public float jointDistance;
	public GameObject stepObject;
	private List<GameObject> steps = new List<GameObject>();
	private Vector3[] positions;

	// Use this for initialization
	void Start () {
		CreateBridge ();
	}
	
	// Update is called once per frame
	void Update () {
		if (lineRenderer != null) {
			int i = 0;
			foreach (GameObject step in steps) {
				positions [i++] = step.transform.position;
			}
			positions [i++] = anchor2.transform.position;
			lineRenderer.positionCount = i;
			lineRenderer.SetPositions (positions);
		}
	}

	public void CreateBridge() {
		float distance = Vector3.Distance (anchor1.transform.position, anchor2.transform.position);
		int intermediatePoints = Mathf.RoundToInt (distance / jointDistance);
		Vector3 verticalOffset = Vector3.zero;

		GameObject startingPoint = new GameObject ("StartingAnchor");
		startingPoint.transform.SetParent (anchor1.transform);
		startingPoint.transform.localPosition = Vector3.zero;
		Rigidbody previousBody = startingPoint.AddComponent<Rigidbody> ();
		SpringJoint startingJoint = startingPoint.AddComponent<SpringJoint> ();
		startingJoint.spring = 100;
		startingJoint.damper = 10;
		steps.Add (startingPoint);

		for (int i = 1; i < intermediatePoints; i++) {
			GameObject step = new GameObject ();
			step.transform.localPosition = Vector3.zero;
			step.name = "Step" + i;
			if (stepObject != null) {
				GameObject stepMesh = Instantiate (stepObject, step.transform);
				stepMesh.transform.localPosition = Vector3.zero;
				stepMesh.transform.localRotation = Quaternion.identity;
			}
			//step.transform.SetParent (transform);
			float slerpFactor = (float)i / intermediatePoints;
			verticalOffset.y = -Mathf.Sin (Mathf.PI * slerpFactor) * 5;
			Vector3 pos = Vector3.Slerp (anchor1.transform.position, anchor2.transform.position, slerpFactor) + verticalOffset;
			step.transform.position = pos;
			Rigidbody body = step.AddComponent<Rigidbody> ();
			body.mass = 0.1f;
			SpringJoint joint = step.AddComponent<SpringJoint> ();
			joint.connectedBody = previousBody;
			joint.enableCollision = true;
			joint.spring = 100;
			joint.damper = 10;
			previousBody = body;
			steps.Add (step);
		}

		GameObject endPoint = new GameObject ("EndAnchor");
		endPoint.transform.SetParent (anchor2.transform);
		endPoint.transform.localPosition = Vector3.zero;
		Rigidbody endPointBody = endPoint.AddComponent<Rigidbody> ();
		endPointBody.isKinematic = true;

		SpringJoint anchorJoint = endPoint.AddComponent<SpringJoint>();
		anchorJoint.spring = 100;
		anchorJoint.damper = 10;
		anchorJoint.connectedBody = previousBody;
		int idx = 0;
		positions = new Vector3[steps.Count+1];
		foreach(GameObject step in steps) {
			positions [idx++] = step.transform.position;
		}
		positions [idx] = anchor2.transform.position;
	}
}
