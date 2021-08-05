using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AreaEntry : MonoBehaviour {

	public string areaName;
	public int priority;

	Vector2[] points2D;

	// Use this for initialization
	void Start () {
		points2D = new Vector2[transform.childCount];

		for (int i = 0; i < transform.childCount; i++)
		{
			Vector3 pos = transform.GetChild(i).position;
			points2D[i] = new Vector2(pos.x, pos.z);
		}
		LocationRegistry.RegisterArea(areaName, points2D, priority);
	}
}
