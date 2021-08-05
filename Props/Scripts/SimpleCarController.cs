using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SimpleCarController : MonoBehaviour {
	public List<AxleInfo> axleInfos; // the information about each individual axle
	public float maxMotorTorque; // maximum torque the motor can apply to wheel
	public float maxSteeringAngle; // maximum steer angle the wheel can have

	public void FixedUpdate()
	{
		float motor = maxMotorTorque * Input.GetAxis("Vertical");
		float steering = maxSteeringAngle * Input.GetAxis("Horizontal");

		foreach (AxleInfo axleInfo in axleInfos) {
			if (axleInfo.steering) {
				axleInfo.leftWheel.steerAngle = steering;
				axleInfo.rightWheel.steerAngle = steering;
			}
			if (axleInfo.motor) {
				axleInfo.leftWheel.motorTorque = motor;
				axleInfo.rightWheel.motorTorque = motor;
			}
			ApplyTransformToWheelMesh (axleInfo.leftWheel, axleInfo.meshLeft);
			ApplyTransformToWheelMesh (axleInfo.rightWheel, axleInfo.meshRight);
		}
	}

	void ApplyTransformToWheelMesh(WheelCollider collider, Transform meshTransform) {
		Vector3 position;
		Quaternion rotation;
		collider.GetWorldPose(out position, out rotation);
		meshTransform.position = position;
		meshTransform.rotation = rotation;
	}
}

[System.Serializable]
public class AxleInfo {
	public WheelCollider leftWheel;
	public WheelCollider rightWheel;
	public Transform meshLeft;
	public Transform meshRight;
	public bool motor; // is this wheel attached to motor?
	public bool steering; // does this wheel apply steer angle?
}
