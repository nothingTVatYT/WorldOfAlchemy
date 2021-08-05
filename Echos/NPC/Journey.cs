using System;
using UnityEngine;

[Serializable]
public class Journey
{
	private Transform[] waypoints;
	private int currentIndex;
	public float delay1;
	public float delay2;

	public Transform getCurrentWaypoint ()
	{
		if (currentIndex < waypoints.Length)
			return waypoints [currentIndex];
		return null;
	}

	public Transform getNextWaypoint ()
	{
		currentIndex++;
		return getCurrentWaypoint();
	}

	public Journey (Transform[] locations, float delay1, float delay2)
	{
		waypoints = locations;
		currentIndex = 0;
	}

}

