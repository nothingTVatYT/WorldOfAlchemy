using System;
using UnityEngine;

[CreateAssetMenu (fileName = "New (Single) Location", menuName = "Terrain/Recorded Location (Single)")]
public class RecordedLocation : ScriptableObject
{
	public Vector3 position;
	public Vector3 rotation;
}
