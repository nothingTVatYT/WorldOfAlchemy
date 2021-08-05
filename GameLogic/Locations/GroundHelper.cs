using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class GroundHelper : MonoBehaviour {

	public float checkGroundOffset = 100;
	public bool realTimeUpdate = false;
	public float extendX = 1.5f;
	public float extendZ = 1.5f;
	Vector3 previousPosition;
	float rotationY;
	BoxCollider boundingCollider;
	public GameObject hitObject;
	public float maxDistanceFromGround;
	public bool isPlacedOnGround;
	Vector3 castHit;
	Vector3 castSize;
	List<Vector3> savedHeights = new List<Vector3>();
	TerrainData savedForTerrainData;
	Terrain savedForTerrain;

	// Use this for initialization
	void Start () {
		previousPosition = Vector3.zero;
		rotationY = -20000;
		boundingCollider = GetComponent<BoxCollider> ();
		if (boundingCollider == null)
			boundingCollider = AddBoundingBoxCollider ();
	}

	// Update is called once per frame
	void Update () {
		if (realTimeUpdate) {
			TouchGround ();
		}
	}

	public void FlattenTerrain() {
		if (savedHeights.Count > 0)
			RestoreHeights ();
		if (hitObject != null) {
			Terrain terrain = hitObject.GetComponent<Terrain> ();
			if (terrain != null) {
				TerrainData terrainData = terrain.terrainData;
				Vector2[] tc = new Vector2[4];
				float commonY = -boundingCollider.size.y / 2;
				Vector3 worldCorner0 = transform.TransformPoint (boundingCollider.center + new Vector3 (-boundingCollider.size.x / 2, commonY, -boundingCollider.size.z / 2));
				tc[0] = CalculateTerrainCoordinates (worldCorner0, terrainData);
				Vector3 worldCorner1 = transform.TransformPoint (boundingCollider.center + new Vector3 (boundingCollider.size.x / 2, commonY, -boundingCollider.size.z / 2));
				tc[1] = CalculateTerrainCoordinates (worldCorner1, terrainData);
				Vector3 worldCorner2 = transform.TransformPoint (boundingCollider.center + new Vector3 (boundingCollider.size.x / 2, commonY, boundingCollider.size.z / 2));
				tc[2] = CalculateTerrainCoordinates (worldCorner2, terrainData);
				Vector3 worldCorner3 = transform.TransformPoint (boundingCollider.center + new Vector3 (-boundingCollider.size.x / 2, commonY, boundingCollider.size.z / 2));
				tc[3] = CalculateTerrainCoordinates (worldCorner3, terrainData);
				PolynomUtils.NoOverlap (tc);
				List<Vector2> inside = PolynomUtils.PointsInside (tc);
				float[,] heights = new float[1,1];
				heights [0,0] = worldCorner0.y / terrainData.size.y;
				foreach (Vector2 v in inside) {
					float[,] relativeHeights = terrainData.GetHeights ((int)v.x, (int)v.y, 1, 1);
					savedHeights.Add (new Vector3 (v.x, relativeHeights[0,0], v.y));
					terrainData.SetHeightsDelayLOD ((int)v.x, (int)v.y, heights);
				}
				savedForTerrainData = terrainData;
				savedForTerrain = terrain;
				terrain.ApplyDelayedHeightmapModification ();
			}
		}
	}

	public void RestoreHeights() {
		float[,] heights = new float[1,1];
		foreach (Vector3 data in savedHeights) {
			heights [0, 0] = data.y;
			savedForTerrainData.SetHeightsDelayLOD ((int)data.x, (int)data.z, heights);
		}
		savedForTerrain.ApplyDelayedHeightmapModification ();
		savedHeights.Clear ();
	}

	void CalculateTerrainCoordinates(Vector3 worldVector, TerrainData terrainData, out int x, out int y) {
		Vector3 terrainLocal = hitObject.transform.InverseTransformPoint (worldVector);
		x = Mathf.RoundToInt (terrainLocal.x / terrainData.size.x * terrainData.heightmapResolution);
		y = Mathf.RoundToInt (terrainLocal.z / terrainData.size.z * terrainData.heightmapResolution);
	}

	Vector2 CalculateTerrainCoordinates(Vector3 worldVector, TerrainData terrainData) {
		Vector2 result;
		Vector3 terrainLocal = hitObject.transform.InverseTransformPoint (worldVector);
		result.x = Mathf.RoundToInt (terrainLocal.x / terrainData.size.x * terrainData.heightmapResolution);
		result.y = Mathf.RoundToInt (terrainLocal.z / terrainData.size.z * terrainData.heightmapResolution);
		return result;
	}

	BoxCollider AddBoundingBoxCollider ()
	{
		Quaternion originalRotation = transform.rotation;
		transform.rotation = Quaternion.identity;
		Bounds bounds = new Bounds(transform.position, Vector3.zero);
		foreach (Renderer r in GetComponentsInChildren<MeshRenderer>(true)) {
			bounds.Encapsulate (r.bounds);
		}
		foreach (Collider c in GetComponentsInChildren<Collider>()) {
			c.enabled = false;
		}
		BoxCollider newCollider = gameObject.AddComponent<BoxCollider> ();
		newCollider.center = transform.InverseTransformPoint(bounds.center);
		newCollider.size = bounds.size + new Vector3(2 * extendX, 0, 2 * extendZ);

		transform.rotation = originalRotation;
		return newCollider;
	}

	public void IncludeInBoundingCheck(bool what) {
		if (boundingCollider == null)
			boundingCollider = AddBoundingBoxCollider ();
		boundingCollider.enabled = what;
	}

	public void TouchGround() {
		if (Vector3.Distance (previousPosition, transform.position) > 0.1f || Mathf.Abs(transform.rotation.eulerAngles.y - rotationY) > 0.1f) {
			RaycastHit hit;
			if (boundingCollider == null)
				boundingCollider = AddBoundingBoxCollider ();
			IncludeInBoundingCheck (false);
			castHit = boundingCollider.center + transform.position + Vector3.up * checkGroundOffset;
			castSize = boundingCollider.size / 2;

			if (Physics.BoxCast (castHit, castSize, Vector3.down, out hit, transform.rotation)) {
				hitObject = hit.collider.gameObject;
				Vector3 pos = transform.position;
				pos.y = hit.point.y + boundingCollider.size.y / 2 - boundingCollider.center.y;
				transform.position = pos;
				float maxDistance = 0;
				Vector3 corner = boundingCollider.center + new Vector3 (boundingCollider.size.x / 2, -boundingCollider.size.y / 2, boundingCollider.size.z / 2);
				maxDistance = DistanceToGround (transform.TransformPoint (corner));
				corner = boundingCollider.center + new Vector3 (-boundingCollider.size.x / 2, -boundingCollider.size.y / 2, boundingCollider.size.z / 2);
				maxDistance = Mathf.Max (maxDistance, DistanceToGround (transform.TransformPoint (corner)));
				corner = boundingCollider.center + new Vector3 (-boundingCollider.size.x / 2, -boundingCollider.size.y / 2, -boundingCollider.size.z / 2);
				maxDistance = Mathf.Max (maxDistance, DistanceToGround (transform.TransformPoint (corner)));
				corner = boundingCollider.center + new Vector3 (boundingCollider.size.x / 2, -boundingCollider.size.y / 2, -boundingCollider.size.z / 2);
				maxDistance = Mathf.Max (maxDistance, DistanceToGround (transform.TransformPoint (corner)));
				maxDistanceFromGround = maxDistance;
				isPlacedOnGround = hitObject.GetComponent<Terrain> () != null;
			} else {
				hitObject = null;
				isPlacedOnGround = false;
			}
			IncludeInBoundingCheck (true);
			previousPosition = transform.position;
			rotationY = transform.rotation.eulerAngles.y;
		}
	}

	float DistanceToGround(Vector3 v) {
		RaycastHit hit;
		if (Physics.Raycast(v, Vector3.down, out hit)) {
			return hit.distance;
		}
		return 0;
	}
}
