using System.Collections.Generic;
using UnityEngine;

public class LocationRegistry : MonoBehaviour {

	public struct WOAArea {
		public string name;
		public Vector2[] points;
		public int priority;
	}

	[SerializeField] Transform[] locations;
	List<WOAArea> areas = new List<WOAArea>();
	static LocationRegistry registry;

	void Awake() {
		registry = this;
	}

	public Transform Location(string name) {
		foreach (Transform l in locations) {
			if (l.name == name)
				return l;
		}
		return null;
	}

	public string LocationName(Transform t) {
		if (areas != null) {
			Vector2 pos2 = new Vector2(t.position.x, t.position.z);
			List<WOAArea> inAreas = new List<WOAArea>();
			foreach (WOAArea area in areas)
			{
				Debug.Log("Check area " + area.name + " for location " + t.position);
				if (ContainsPoint(area.points, pos2)) {
					Debug.Log("Location is on area " + area.name);
					inAreas.Add(area);
				}
			}
			if (inAreas.Count > 0) {
				inAreas.Sort((a,b) => a.priority.CompareTo(b.priority));
				return inAreas[0].name;
			}
		}
		return "Unbekannt";
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

	public void _RegisterArea(string areaName, Vector2[] points, int priority) {
		WOAArea newArea = new WOAArea();
		newArea.name = areaName;
		newArea.points = points;
		newArea.priority = priority;
		areas.Add(newArea);
		Debug.Log("Registered area " + newArea.name + " with priority " + newArea.priority);
	}

	public static Transform LocationByName(string name) {
		return registry.Location(name);
	}

	public static string LocationByTransform(Transform t) {
		return registry.LocationName(t);
	}

	public static void RegisterArea(string areaName, Vector2[] points, int priority) {
		registry._RegisterArea(areaName, points, priority);
	}
}
