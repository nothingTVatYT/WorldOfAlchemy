using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[ExecuteInEditMode]
public class BridgeTile : MonoBehaviour {

	public float xOffset = 0.75f;
	public GameObject bridgeTile;
	public LineRenderer lineRenderer1;
	public LineRenderer lineRenderer2;
	private Vector3 myAnchor;
	private Vector3 connectedAnchor;
	private Joint joint;
	private Vector3[] positions1 = new Vector3[2];
	private Vector3[] positions2 = new Vector3[2];
	private Rigidbody connectedTo;

	void Start () {
		joint = bridgeTile.GetComponent<Joint> ();
		myAnchor = joint.anchor;
		connectedTo = joint.connectedBody;
		if (connectedTo == null) {
			connectedAnchor = joint.connectedAnchor;
		}
	}

	// Update is called once per frame
	void Update () {
		positions1 [0] = bridgeTile.transform.TransformPoint(myAnchor + Vector3.right * xOffset);
		positions2 [0] = bridgeTile.transform.TransformPoint(myAnchor - Vector3.right * xOffset);
		if (connectedTo != null) {
			positions1 [1] = connectedTo.transform.TransformPoint (joint.connectedAnchor + Vector3.right * xOffset);
			positions2 [1] = connectedTo.transform.TransformPoint (joint.connectedAnchor - Vector3.right * xOffset);
		} else {
			positions1 [1] = connectedAnchor + Vector3.right * xOffset;
			positions2 [1] = connectedAnchor - Vector3.right * xOffset;
		}
		lineRenderer1.positionCount = 2;
		lineRenderer1.SetPositions (positions1);
		lineRenderer2.positionCount = 2;
		lineRenderer2.SetPositions (positions2);
	}
}
