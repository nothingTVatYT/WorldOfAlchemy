using System;
using UnityEngine;

[Serializable]
public class WOAMotivation
{
	public enum ActionType
	{
WalkTo,
		PickUp,
		GetBasket,
		DropBasket,
		WalkAlong,
		FollowTarget,
		TalkTo,
		DropObject}

	;

	public ActionType actionType;
	public Transform destination;
	public Vector3 destinationPosition;
	public bool useVector;
	public GameObject objectPrefab;
	public GameObject droppedObject;
	public bool kneelDown;
	public bool pickupControlledByAnimation = true;
	public bool destroyOnDrop = true;
	public float speedMultiplier = 1;
	public Action<WOAMotivation> onComplete;
	int currentIndex;
	Bounds targetBounds;
	bool boundsCalculated;
	bool multiTransformTarget;
	float minBoundsSize = 0.3f;

	// temporary debug
	bool drawGizmos;
	Vector3 gizmo1;
	Vector3 gizmo2;

	public Vector3 destinationVector {
		get {
			if (useVector || destination == null)
				return destinationPosition;
			else
				return destination.position;
		}
	}

	public bool hasDestination {
		get { return destination != null || useVector; }
	}

	public WOAMotivation ()
	{
	}

	public WOAMotivation (ActionType action)
	{
		actionType = action;
	}

	public WOAMotivation (ActionType action, Transform destination)
	{
		actionType = action;
		this.destination = destination;
		UpdateTargetBounds();
	}

	public WOAMotivation (ActionType action, Vector3 v)
	{
		actionType = action;
		destinationPosition = v;
		useVector = true;
		UpdateTargetBounds();
	}

	public WOAMotivation (ActionType action, GameObject obj, Transform destination)
	{
		actionType = action;
		objectPrefab = obj;
		this.destination = destination;
		UpdateTargetBounds();
	}

	public override string ToString ()
	{
		return actionType + " " + (destination != null ? destination.name : destinationPosition.ToString()) + "(speed*" + speedMultiplier + ")";
	}

	public Transform nextDestination ()
	{
		if (destination == null || destination.childCount == 0 || currentIndex >= destination.childCount) {
			UpdateTargetBounds();
			return null;
		}
		Transform destinationToReach = destination.GetChild (currentIndex++);
		UpdateTargetBounds();
		return destinationToReach;
	}

	public bool HasReachedTarget(Transform t, float targetYMin, float targetXZMin) {
		if (!boundsCalculated)
			UpdateTargetBounds();
		drawGizmos = false;
		if (destination == null && !useVector)
			return true;
		drawGizmos = true;
		gizmo1 = t.position;
		Vector3 closestPoint = targetBounds.ClosestPoint(t.position);
		gizmo2 = closestPoint;
		float distanceY = Mathf.Abs(closestPoint.y - t.position.y);
		float dx = closestPoint.x - t.position.x;
		float dz = closestPoint.z - t.position.z;
		float distanceXZ = Mathf.Sqrt (dx * dx + dz * dz);
		return distanceY <= targetYMin && distanceXZ <= targetXZMin;
	}

	void UpdateTargetBounds() {
		multiTransformTarget = actionType == ActionType.WalkAlong;
		Transform destinationToReach = destination;
		if (multiTransformTarget) {
			if (destination != null && currentIndex < destination.childCount)
				destinationToReach = destination.GetChild(currentIndex);
		}
		if (useVector) {
			targetBounds.center = destinationPosition;
			targetBounds.size = new Vector3(minBoundsSize, minBoundsSize, minBoundsSize);
		} else {
			if (destinationToReach != null) {
				Collider c = destinationToReach.GetComponent<Collider>();
				if (c != null) {
					targetBounds = c.bounds;
				} else {
					MeshRenderer r = destinationToReach.GetComponent<MeshRenderer>();
					if (r != null)
						targetBounds = r.bounds;
					else {
						targetBounds.center = destinationToReach.position;
						targetBounds.size = new Vector3(minBoundsSize, minBoundsSize, minBoundsSize);
					}

				}
			}
		}
		boundsCalculated = true;
		//Debug.Log("Updated target bounds for " + this);
	}

	public void OnDrawGizmosSelected()
	{
		if (drawGizmos) {
			Gizmos.color = Color.red;
			Gizmos.DrawLine(gizmo1, gizmo2);
			//Debug.Log("current target length=" + Vector3.Distance(gizmo1, gizmo2));
		}
	}
}

