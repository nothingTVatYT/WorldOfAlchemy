using System;
using UnityEngine;
using System.Collections.Generic;

public class PolynomUtils
{
	public static bool LineSegmentsCollide (Vector2 a1, Vector2 a2,
	                                       Vector2 b1, Vector2 b2)
	{
		float denom = (b2.y - b1.y) * (a2.x - a1.x) -
		              (b2.x - b1.x) * (a2.y - a1.y);
		if (Mathf.Abs (denom) < Vector2.kEpsilon)
			return false;

		float ua = ((b2.x - b1.x) * (a1.y - b1.y) -
		           (b2.y - b1.y) * (a1.x - b1.x)) / denom;
		float ub = ((a2.x - a1.x) * (a1.y - b1.y) -
		           (a2.y - a1.y) * (a1.x - b1.x)) / denom;
		return ua >= 0 && ua <= 1 && ub >= 0 && ub <= 1;
	}

	public static bool ContainsPoint (Vector2[] polyPoints, Vector2 p)
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

	public static void NoOverlap(Vector2[] points) {
		if (points.Length != 4)
			throw new Exception ("NoOverlap only supports 4 vertices.");
		if (LineSegmentsCollide(points[0], points[1], points[2], points[3])) {
			Vector2 backup = points [3];
			points [1] = points [3];
			points [3] = backup;
		} else if (LineSegmentsCollide(points[1], points[2], points[3], points[0])) {
			Vector2 backup = points [2];
			points [2] = points [3];
			points [3] = backup;
		}
	}

	public static Vector2[] BoundingBox(Vector2[] points) {
		float xmin = points[0].x;
		float xmax = points[0].x;
		float ymin = points[0].y;
		float ymax = points[0].y;
		for (int i = 1; i < points.Length; i++) {
			xmin = Mathf.Min(xmin, points[i].x);
			xmax = Mathf.Max(xmax, points[i].x);
			ymin = Mathf.Min(ymin, points[i].y);
			ymax = Mathf.Max(ymax, points[i].y);
		}
		return new Vector2[] { new Vector2(xmin, ymin), new Vector2(xmax, ymin), new Vector2(xmax, ymax), new Vector2(xmin, ymax)};
	}

	public static List<Vector2> PointsInside(Vector2[] points) {
		List<Vector2> list = new List<Vector2>();
		Vector2[] bb = BoundingBox(points);
		for (int y = (int)bb[0].y; y < bb[2].y; y++) {
			for (int x = (int)bb[0].x; x < bb[1].x; x++) {
				Vector2 v = new Vector2(x,y);
				if (ContainsPoint(points, v))
					list.Add(v);
			}
		}
		return list;
	}
}

