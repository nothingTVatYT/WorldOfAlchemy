using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class Architect : MonoBehaviour {

	[Serializable]
	public struct GridPoint {
		public Vector3 measured;
		public float accuracy;
		public bool valid;
		public bool marked;
	}

	public struct PossiblePlace {
		public int gridX;
		public int gridZ;
		public float maxHeightDifference;
		public bool nearBorder;
		public int missingPlaces;
		public float fitness;
		public bool marked;
		public override string ToString ()
		{
			return string.Format ("grid {0}/{1}, max diff={2}, missing={3}, fitness={4}", gridX, gridZ, maxHeightDifference, missingPlaces, fitness);
		}
		public bool Overlaps(PossiblePlace other, int sideLength) {
			if ((Mathf.Abs (gridX - other.gridX) < sideLength) && (Mathf.Abs (gridZ - other.gridZ) < sideLength))
				return true;
			return false;
		}
	}

	public class FitnessComparer : IComparer<PossiblePlace> {
		#region IComparer implementation
		public int Compare (PossiblePlace x, PossiblePlace y)
		{
			return (x.fitness > y.fitness) ? -1 : ((x.fitness < y.fitness) ? 1 : 0);
		}
		#endregion
	}

	FitnessComparer fitnessComparer = new FitnessComparer();

	readonly GridPoint NONE = new GridPoint ();

	public Transform startPosition;
	public float radius = 20f;
	public GameObject poleObject;
	public GameObject markerObject;
	public bool recording;
	public int heightUpdateRadius = 1;
	public int minSideLength = 6;
	public int generations = 10;
	public Material mapMaterial;

	IMotivationController motivationController;
	GridPoint[][] grid;

	int validGridPoints = 0;
	int possibleGridPoints;
	List<PossiblePlace> possiblePlaces = new List<PossiblePlace>();
	Queue<Vector3> placesToVisit = new Queue<Vector3>();
	PossiblePlace currentBestPlace;
	Color InvalidPointColor = Color.blue;
	List<GameObject> placedObjects = new List<GameObject> ();

	void Start () {
		int gridPoints = Mathf.CeilToInt (radius * 2);
		grid = new GridPoint[gridPoints][];
		possibleGridPoints = Mathf.FloorToInt(radius * radius * Mathf.PI);
		motivationController = GetComponent<IMotivationController> ();
		if (startPosition != null) {
			WOAMotivation m = new WOAMotivation (WOAMotivation.ActionType.WalkTo, startPosition);
			motivationController.injectMotivation (m);
			recording = true;
		}
		markerObject.transform.localScale = new Vector3 (minSideLength, 1, minSideLength);
		StartCoroutine (Explore ());
	}

	void Update () {
		if (recording) {
			UpdateGridPoints (transform.position);
		}
	}

	void UpdateGridPoint(Vector3 pos) {
		int gridX, gridZ;
		if (IsOnGrid(pos, out gridX, out gridZ)) {
			StoreHeight (pos, gridX, gridZ);
		}
	}

	void UpdateGridPoints(Vector3 pos) {
		UpdateGridPoint (pos);
		int radiusSquared = heightUpdateRadius * heightUpdateRadius;
		for (int x = -heightUpdateRadius; x <= heightUpdateRadius; x++) {
			for (int z = -heightUpdateRadius; z <= heightUpdateRadius; z++) {
				if ((x * x + z * z) <= radiusSquared)
					UpdateGridPoint(NavMeshPoint (pos + new Vector3 (x, 0, z), 10, -1));
			}
		}
	}

	float HeightAt(Vector3 pos) {
		int gridX, gridZ;
		if (IsOnGrid(pos, out gridX, out gridZ)) {
			GridPoint point = CurrentGridPoint (gridX, gridZ);
			if (point.valid)
				return point.measured.y;
		}
		return float.NaN;
	}

	bool IsOnGrid(Vector3 pos, out int gridX, out int gridZ) {
		float distX = pos.x - startPosition.position.x;
		float distZ = pos.z - startPosition.position.z;
		float distXZ = Mathf.Sqrt (distX * distX + distZ * distZ);
		if (distXZ > radius) {
			gridX = 0;
			gridZ = 0;
			return false;
		}
		gridX = Mathf.RoundToInt (distX);
		gridZ = Mathf.RoundToInt (distZ);
		return true;
	}

	float AccuracyToGridPoint(Vector3 pos) {
		float distX = pos.x - startPosition.position.x;
		float distZ = pos.z - startPosition.position.z;
		float distXZ = Mathf.Sqrt (distX * distX + distZ * distZ);
		if (distXZ > radius) {
			return 0;
		}
		float offsetX = distX - Mathf.RoundToInt (distX);
		float offsetZ = distZ - Mathf.RoundToInt (distZ);
		return Mathf.Sqrt (offsetX * offsetX + offsetZ * offsetZ);
	}

	Vector3 PositionForGridPoint(int gridX, int gridZ) {
		GridPoint p = CurrentGridPoint (gridX, gridZ);
		if (p.valid) {
			return new Vector3 (startPosition.position.x + gridX, p.measured.y, startPosition.position.z + gridZ);
		}
		return startPosition.position + new Vector3 (gridX, 0, gridZ);
	}

	GridPoint CurrentGridPoint(int gridX, int gridZ) {
		int indexX = gridX + grid.Length / 2;
		int indexZ = gridZ + grid.Length / 2;
		if (indexX < 0 || indexZ < 0 || indexX >= grid.Length || indexZ >= grid.Length) {
			//Debug.LogError (string.Format ("Illegal index ({0}/{1}) for grid {2}/{3}", indexX, indexZ, gridX, gridZ));
			return NONE;
		}
		if (grid [indexZ] == null)
			return NONE;
		if (grid [indexZ].Length <= indexX)
			return NONE;
		return grid [indexZ] [indexX];
	}

	void StoreHeight(Vector3 position, int gridX, int gridZ) {
		int indexX = gridX + grid.Length / 2;
		int indexZ = gridZ + grid.Length / 2;
		if (indexX < 0 || indexZ < 0 || indexX >= grid.Length || indexZ >= grid.Length) {
			//Debug.LogError (string.Format ("Illegal index ({0}/{1}) for grid {2}/{3}", indexX, indexZ, gridX, gridZ));
			return;
		}
		if (grid [indexZ] == null)
			grid[indexZ] = new GridPoint[grid.Length];
		GridPoint point = grid [indexZ] [indexX];
		if (!point.valid) {
			validGridPoints++;
			//Debug.Log (string.Format("valid grid points: {0}/{1} ({2})", validGridPoints, possibleGridPoints, (float)validGridPoints/possibleGridPoints));
			point.accuracy = AccuracyToGridPoint (position);
			point.valid = true;
			point.measured = position;
			grid [indexZ] [indexX] = point;
			UpdateTexture ();
		} else {
			float thisAccuracy = AccuracyToGridPoint (position);
			if (thisAccuracy > point.accuracy) {
				point.measured = position;
				point.accuracy = thisAccuracy;
				grid [indexZ] [indexX] = point;
			}
		}
	}

	void UpdateTexture() {
		if (mapMaterial != null) {
			Texture2D mapTexture = mapMaterial.mainTexture as Texture2D;
			if (mapTexture == null) {
				mapTexture = new Texture2D(grid.Length, grid.Length);
				mapMaterial.mainTexture = mapTexture;
			}
			Color[] colors = new Color[mapTexture.width * mapTexture.height];
			for (int i = 0; i < colors.Length; i++)
				colors [i] = InvalidPointColor;
			float minHeight = float.MaxValue;
			float maxHeight = float.MinValue;
			for(int gridZ = 0; gridZ < grid.Length; gridZ++) {
				if (grid [gridZ] != null) {
					for (int gridX = 0; gridX < grid.Length; gridX++) {
						GridPoint point = grid [gridZ] [gridX];
						if (point.valid) {
							minHeight = Mathf.Min (minHeight, point.measured.y);
							maxHeight = Mathf.Max (maxHeight, point.measured.y);
						}
					}
				}
			}
			for(int gridZ = 0; gridZ < grid.Length; gridZ++) {
				if (grid [gridZ] != null) {
					for (int gridX = 0; gridX < grid.Length; gridX++) {
						GridPoint point = grid [gridZ] [gridX];
						if (point.valid) {
							float grey = (point.measured.y - minHeight) / (maxHeight - minHeight);
							if (point.marked)
								grey = 1f - grey;
							colors [gridZ * grid.Length + gridX] = new Color (grey, grey, grey);
						}
					}
				}
			}
			mapTexture.SetPixels (colors);
			mapTexture.Apply ();
		}
	}

	Vector3 RandomNavSphere (Vector3 origin, float distance, int layermask) {
		for (int i = 0; i < 20; i++) {
			Vector3 randomDirection;
			if (Random.value < 0.3f) {
				randomDirection = Random.onUnitSphere * distance;
			} else {
				float phi = Random.value * Mathf.PI * 2;
				randomDirection = new Vector3 (Mathf.Cos (phi) * distance, 0, Mathf.Sin (phi) * distance);
			}
			randomDirection += origin;
			NavMeshHit navHit;
			if (NavMesh.SamplePosition (randomDirection, out navHit, distance, layermask))
				return navHit.position;
		}
		return origin;
	}

	Vector3 NavMeshPoint(Vector3 v, float distance, int layermask) {
		NavMeshHit navHit;
		if (NavMesh.SamplePosition (v, out navHit, distance, layermask))
			return navHit.position;
		return v;
	}

	bool IsValid(int gridX, int gridZ) {
		return gridX > 0 && gridZ > 0 && gridX < grid.Length && gridZ < grid.Length;
	}

	bool IsAlreadyOccupied(int gridX, int gridZ) {
		GridPoint point = CurrentGridPoint (gridX, gridZ);
		return point.valid && point.marked;
	}

	void MarkOccupied(int gridX, int gridZ, int extendX, int extendZ) {
		for (int x = -extendX; x <= extendX; x++) {
			for (int z = -extendZ; z <= extendZ; z++) {
				GridPoint point = CurrentGridPoint (gridX + x, gridZ + z);
				if (point.valid) {
					int indexX = gridX + x + grid.Length / 2;
					int indexZ = gridZ + z + grid.Length / 2;
					point.marked = true;
					grid [indexZ] [indexX] = point;
				}		
			}
		}
	}

	void ProcessPlaces() {
		// calculate fitness
		for(int i = 0; i < possiblePlaces.Count; i++) {
			PossiblePlace place = possiblePlaces[i];
			GridPoint midPoint = CurrentGridPoint (place.gridX, place.gridZ);
			float height = midPoint.measured.y;
			float maxHeightDiff = 0;
			int missingHeightDetails = 0;
			for (int x = -minSideLength / 2; x <= minSideLength / 2; x++) {
				for (int z = -minSideLength / 2; z <= minSideLength / 2; z++) {
					if (IsValid (place.gridX + x, place.gridZ + z)) {
						GridPoint p = CurrentGridPoint (place.gridX + x, place.gridZ + z);
						if (p.valid) {
							float h = p.measured.y;
							maxHeightDiff = Mathf.Max (maxHeightDiff, Mathf.Abs (height - h));
						} else {
							missingHeightDetails++;
							if (placesToVisit.Count < 200) {
								Vector3 v = NavMeshPoint (midPoint.measured + new Vector3 (x, 0f, z), 10, -1);
								int gx, gz;
								if (IsOnGrid (v, out gx, out gz))
									placesToVisit.Enqueue (v);
								else
									place.nearBorder = true;
							}
						}
					} else
						place.nearBorder = true;
				}
			}
			place.maxHeightDifference = maxHeightDiff;
			place.missingPlaces = missingHeightDetails;
			place.fitness = 20 - maxHeightDiff - (place.nearBorder ? 20 : 0) - 0.05f * place.missingPlaces - 0.001f * Mathf.Abs(place.gridX) * Mathf.Abs(place.gridZ);
			possiblePlaces [i] = place;
		}
		possiblePlaces.Sort (fitnessComparer);
		if (possiblePlaces.Count == generations) {
			List<PossiblePlace> newList = new List<PossiblePlace> ();
			newList.AddRange (possiblePlaces.GetRange (0, possiblePlaces.Count / 2));
			int newItems = generations - newList.Count;
			for (int i = 0; i < newItems; i++) {
				PossiblePlace pl = newList [i];
				pl.gridX += Random.Range (-1, 1);
				pl.gridZ += Random.Range (-1, 1);
				pl.marked = false;
				newList.Add (pl);
			}
			possiblePlaces = newList;
		}
	}

	void AddToPossiblePlaces(PossiblePlace pl) {
		foreach(PossiblePlace p in possiblePlaces) {
			if (p.gridX == pl.gridX && p.gridZ == pl.gridZ)
				return;
		}
		possiblePlaces.Add (pl);
	}

	public void RecordObject(WOAMotivation m) {
		placedObjects.Add (m.droppedObject);
	}

	public void RemoveObject(WOAMotivation m) {
		if (m.destination != null && m.destination.gameObject != null)
			Destroy (m.destination.gameObject);
	}

	bool FindUnknown(out int gridX, out int gridZ) {
		gridX = Mathf.RoundToInt(Random.Range (-radius, radius));
		gridZ = Mathf.RoundToInt(Random.Range (-radius, radius));
		int direction = Random.value < 0.5f ? -1 : 1;
		while (IsValid(gridX, gridZ) && CurrentGridPoint(gridX, gridZ).valid) {
			if (direction > 0) {
				gridX++;
				if (gridX >= radius) {
					gridX = Mathf.RoundToInt (-radius);
					gridZ++;
				}
			} else {
				gridX--;
				if (gridX <= -radius) {
					gridX = Mathf.RoundToInt(radius);
					gridZ--;
				}
			}
		}
		return true;
	}

	bool nearPlacedObjects(Vector3 v) {
		bool tooNear = false;
		foreach (GameObject g in placedObjects) {
			if (Vector3.Distance(v, g.transform.position) < 1) {
				tooNear = true;
				break;
			}
		}
		return tooNear;
	}

	IEnumerator Explore() {
		while (motivationController.isBusy)
			yield return null;
		float exploreRate = 0;
		Debug.Log ("Start to explore");
		bool exploring = true;
		int goodPlaces = 0;
		int noNewPlaceRequests = 0;
		do {
			exploreRate = (float)validGridPoints / possibleGridPoints;
			if (!motivationController.isBusy) {
				Vector3 v = startPosition.position;
				if (placesToVisit.Count > 0 && Random.value < 0.3f) {
					while (placesToVisit.Count > 0) {
						v = placesToVisit.Dequeue();
						v = NavMeshPoint(v, 10, -1);
						float h = HeightAt (v);
						if (float.IsNaN(h)) break;
					}
					Debug.Log("visit a place required for evaluation: " + v + ", places to visit = " + placesToVisit.Count + ", possible places = " + possiblePlaces.Count);
				} else {
					float exploreChance = 1f - exploreRate * exploreRate;
					if (possiblePlaces.Count == 0 || Random.value <= exploreChance) {
						// get a random spot
						v = RandomNavSphere (startPosition.position, radius - heightUpdateRadius, -1);
					} else {
						// visit current best place
						if (possiblePlaces.Count > 0) {
							PossiblePlace pl = possiblePlaces[0];
							v = PositionForGridPoint(pl.gridX, pl.gridZ);
							if (pl.missingPlaces == 0 && !nearPlacedObjects(v)) {
								currentBestPlace.gridX = pl.gridX;
								currentBestPlace.gridZ = pl.gridZ;
								v = PositionForGridPoint(pl.gridX, pl.gridZ);
								WOAMotivation m = new WOAMotivation(WOAMotivation.ActionType.DropObject, v);
								m.objectPrefab = poleObject;
								m.onComplete = RecordObject;
								motivationController.injectMotivation (m);
								Debug.Log("a good place: " + pl);
								goodPlaces++;
								MarkOccupied(pl.gridX, pl.gridZ, minSideLength/2, minSideLength/2);
							}
							if (goodPlaces >= 3 && exploreRate >= 0.9f)
								exploring = false;
						} else {
							v = transform.position;
							Debug.Log("Idle. places to visit=" + placesToVisit.Count + ", possible places=" + possiblePlaces.Count + ", current best place=" + possiblePlaces[0]);
						}
					}
				}
				float height = HeightAt (v);
				if (float.IsNaN (height)) {
					WOAMotivation m = new WOAMotivation (WOAMotivation.ActionType.WalkTo, v);
					motivationController.injectMotivation (m);
					noNewPlaceRequests = 0;
				} else {
					noNewPlaceRequests++;
					if (noNewPlaceRequests >= 100) {
						int x,z;
						FindUnknown(out x, out z);
						v = PositionForGridPoint(x, z);
						WOAMotivation m = new WOAMotivation (WOAMotivation.ActionType.WalkTo, v);
						motivationController.injectMotivation (m);
					}
				}
			}
			if (possiblePlaces.Count < generations) {
				if (Random.value < 0.05f) {
					int gridX, gridZ;
					if (IsOnGrid(transform.position, out gridX, out gridZ)) {
						PossiblePlace pl = new PossiblePlace();
						pl.gridX = gridX;
						pl.gridZ = gridZ;
						AddToPossiblePlaces(pl);
					}
				}
			}
			ProcessPlaces();
			yield return null;
		} while (exploring);

		while (motivationController.isBusy) {
			ProcessPlaces ();
			yield return null;
		}

		// clear bars
		foreach (GameObject go in placedObjects) {
			// Destroy (go);
			WOAMotivation m = new WOAMotivation (WOAMotivation.ActionType.PickUp, go.transform);
			m.kneelDown = false;
			m.onComplete = RemoveObject;
			motivationController.injectMotivation (m);
		}
		while (motivationController.isBusy) {
			ProcessPlaces ();
			yield return null;
		}

		placedObjects.Clear();

		// mark the n best places
		List<PossiblePlace> marked = new List<PossiblePlace> ();
		for (int i = 0; i < possiblePlaces.Count; i++) {
			bool ignore = false;
			PossiblePlace pl = possiblePlaces [i];
			for (int j = 0; j < marked.Count; j++) {
				if (marked [j].Overlaps (pl, minSideLength)) {
					ignore = true;
					break;
				}
			}
			if (!ignore) {
				marked.Add (pl);
				Vector3 v = PositionForGridPoint (pl.gridX, pl.gridZ);
				WOAMotivation m = new WOAMotivation (WOAMotivation.ActionType.DropObject, v);
				m.objectPrefab = markerObject;
				m.onComplete = RecordObject;
				motivationController.injectMotivation (m);
				if (marked.Count > 4)
					break;
			}
		}

		while (motivationController.isBusy)
			yield return null;
		Debug.Log ("End of Explore action reached.");
	}
}
