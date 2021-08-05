using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HorseFeetIK : MonoBehaviour {

	public Animator anim;
	public float checkOffset = 0.7f;
	public float checkHeight = 1.4f;
	public float handOffset = 0.1f;
	public Transform leftHand;
	public Transform rightHand;
	public Transform leftFoot;
	public Transform rightFoot;
	public Transform leftHandTarget;
	public Transform rightHandTarget;
	public Transform leftFootTarget;
	public Transform rightFootTarget;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void LateUpdate () {
		RaycastHit hit;
		float leftHandOnGround = anim.GetFloat ("leftHandOnGround");
		if (Physics.Raycast (leftHand.position + Vector3.up * checkOffset, Vector3.down, out hit, checkHeight)) {
			leftHandTarget.position = Vector3.Lerp(leftHand.position, hit.point + Vector3.up * handOffset, leftHandOnGround);
		} else
			leftHandTarget.position = leftHand.position;
		float rightHandOnGround = anim.GetFloat ("rightHandOnGround");
		if (Physics.Raycast (rightHand.position + Vector3.up * checkOffset, Vector3.down, out hit, checkHeight)) {
			rightHandTarget.position = Vector3.Lerp(rightHand.position, hit.point + Vector3.up * handOffset, rightHandOnGround);
		} else
			rightHandTarget.position = rightHand.position;
		float leftFootOnGround = anim.GetFloat ("leftFootOnGround");
		if (Physics.Raycast (leftFoot.position + Vector3.up * checkOffset, Vector3.down, out hit, checkHeight)) {
			leftFootTarget.position = Vector3.Lerp(leftFoot.position, hit.point + Vector3.up * handOffset, leftFootOnGround);
		} else
			leftFootTarget.position = leftFoot.position;
		float rightFootOnGround = anim.GetFloat ("rightFootOnGround");
		if (Physics.Raycast (rightFoot.position + Vector3.up * checkOffset, Vector3.down, out hit, checkHeight)) {
			rightFootTarget.position = Vector3.Lerp(rightFoot.position, hit.point + Vector3.up * handOffset, rightFootOnGround);
		} else
			rightFootTarget.position = rightFoot.position;
	}

	public void OnDrawGizmos() {
		Gizmos.DrawWireSphere (leftHandTarget.position, 0.05f);
		Gizmos.DrawWireSphere (rightHandTarget.position, 0.05f);
		Gizmos.DrawWireSphere (leftFootTarget.position, 0.05f);
		Gizmos.DrawWireSphere (rightFootTarget.position, 0.05f);
	}
}
