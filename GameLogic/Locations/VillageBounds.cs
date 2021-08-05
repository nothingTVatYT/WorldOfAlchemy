using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

[ExecuteInEditMode]
[RequireComponent (typeof(LineRenderer))]
public class VillageBounds : MonoBehaviour
{

	LineRenderer lineRenderer;
	Vector3[] pos;
	Vector3 midPoint;
	public float maxRadius { get; private set; }
	Vector2[] points2D;
	public bool autoUpdateBounds;
	public bool keepOnTerrain;

	// Use this for initialization
	void Start ()
	{
		lineRenderer = GetComponent<LineRenderer> ();
		lineRenderer.loop = true;
		UpdateBounds ();
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (autoUpdateBounds)
			UpdateBounds ();
	}

	public void UpdateBounds ()
	{
		if (keepOnTerrain) {
			for (int i = 0; i < transform.childCount; i++) {
				Vector3 v = transform.GetChild (i).position;
				RaycastHit hit;
				if (Physics.Raycast (v + Vector3.up * 10, Vector3.down, out hit)) {
					v = hit.point + Vector3.up * 0.1f;
					transform.GetChild (i).position = v;
				}
			}
		}
		lineRenderer.positionCount = transform.childCount;
		pos = new Vector3[transform.childCount];
		midPoint = Vector3.zero;
		for (int i = 0; i < transform.childCount; i++) {
			pos [i] = transform.GetChild (i).position;
			midPoint += pos [i];
		}
		lineRenderer.SetPositions (pos);
		midPoint /= transform.childCount;
		maxRadius = 0;
		points2D = new Vector2[pos.Length];
		for (int i = 0; i < pos.Length; i++) {
			maxRadius = Mathf.Max (maxRadius, Vector3.Distance (midPoint, pos [i]));
			points2D [i] = new Vector2 (pos [i].x, pos [i].z);
		}
	}

	public bool IsInBounds(Vector3 v) {
		Vector2 point2D = new Vector2 (v.x, v.z);
		return ContainsPoint (points2D, point2D);
	}

	public Vector3 RandomSpot ()
	{
		Vector2 midPoint2D = new Vector2 (midPoint.x, midPoint.z);
		Vector2 point2D;
		do {
			float phi = Random.Range (0, Mathf.PI * 2);
			float r = Random.Range (0, maxRadius);
			point2D = midPoint2D + new Vector2 (Mathf.Cos (phi) * r, Mathf.Sin (phi) * r);
		} while (!ContainsPoint (points2D, point2D));
		return new Vector3 (point2D.x, midPoint.y, point2D.y);
	}

	bool ContainsPoint (Vector2[] polyPoints, Vector2 p)
	{
		var j = polyPoints.Length - 1;
		var inside = false;
		for (int i = 0; i < polyPoints.Length; j = i++) {
			if (((polyPoints [i].y <= p.y && p.y < polyPoints [j].y) || (polyPoints [j].y <= p.y && p.y < polyPoints [i].y)) &&
			    (p.x < (polyPoints [j].x - polyPoints [i].x) * (p.y - polyPoints [i].y) / (polyPoints [j].y - polyPoints [i].y) + polyPoints [i].x))
				inside = !inside;
		}
		return inside;
	}
}
