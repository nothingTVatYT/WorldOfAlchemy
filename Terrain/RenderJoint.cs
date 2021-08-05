using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class RenderJoint : MonoBehaviour {

	private Vector3 myAnchor;
	private Vector3 connectedAnchor;
	private ConfigurableJoint joint;
	public LineRenderer lineRenderer;
	private Vector3[] positions = new Vector3[2];
	private Rigidbody connectedTo;

	void Start () {
		joint = GetComponent<ConfigurableJoint> ();
		myAnchor = joint.anchor;
		connectedTo = joint.connectedBody;
		if (connectedTo == null) {
			connectedAnchor = joint.connectedAnchor;
		} else {
			connectedAnchor = connectedTo.transform.TransformPoint (joint.connectedAnchor);
		}
	}
	
	void LateUpdate () {
		positions [0] = transform.TransformPoint(myAnchor);
		if (connectedTo != null) {
			connectedAnchor = connectedTo.transform.TransformPoint (joint.connectedAnchor);
		}
		positions [1] = connectedAnchor;
		lineRenderer.positionCount = 2;
		lineRenderer.SetPositions (positions);
	}
}
