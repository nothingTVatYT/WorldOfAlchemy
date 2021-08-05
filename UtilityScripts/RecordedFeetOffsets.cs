using System;
using UnityEngine;

public class RecordedFeetOffsets : ScriptableObject
{
	public Vector3 average;
	public float characterSpeed;
	public float characterMoved;
	public float animationMoved;
	public float animationSpeedMultiplier;
	public Vector3[] offsets;
	public float[] timestamps;
}

