using UnityEngine;

public class ObjectSpawner : MonoBehaviour {

	[SerializeField] GameObject objectToSpawn;
	[SerializeField] float radiusMin;
	[SerializeField] int numberOfObjects;
	[SerializeField] float additionalGroundOffset = 0.05f;
	float yOffset = 0.2f;

	void Start () {
		if (objectToSpawn != null && numberOfObjects > 0) {
			Collider itemCollider = objectToSpawn.GetComponent<Collider> ();
			float effectiveYOffset = 0;
			if (itemCollider != null) {
				effectiveYOffset = itemCollider.bounds.extents.y;
			}
			Vector3 groundOffset = new Vector3 (0, effectiveYOffset + additionalGroundOffset, 0);
			for (int i = 0; i < numberOfObjects; i++) {
				GameObject objInstance = Instantiate (objectToSpawn, transform);
				float angle = Random.Range (0, Mathf.PI * 2);
				Vector3 origin = transform.position + new Vector3 (Mathf.Cos (angle) * radiusMin, yOffset, Mathf.Sin (angle) * radiusMin);
				RaycastHit hit;
				if (Physics.Raycast(origin, Vector3.down, out hit)) {
					objInstance.transform.position = hit.point + groundOffset;
					objInstance.transform.rotation = Quaternion.FromToRotation (Vector3.up, hit.normal) * objInstance.transform.rotation;
				} else {
					objInstance.transform.position = origin;
				}
			}
		}
	}
}
