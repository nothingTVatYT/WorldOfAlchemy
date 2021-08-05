using System;
using UnityEngine;

[Serializable]
public class WOASubstance : WOAItem
{
	public int maxStackSize = 1;
	public int currentStackSize = 1;
	public string substanceId;
	public string neededSkill;
	[Range(1f, 5f)]
	public float gainLevelOverlap = 2;
	[Range(0.001f, 1f)]
	public float gainFactor = 0.05f;

	public WOASubstance ()
	{
	}

	public WOASubstance (WOASubstance other) : base (other)
	{
		maxStackSize = other.maxStackSize;
		currentStackSize = other.currentStackSize;
		substanceId = other.substanceId;
		neededSkill = other.neededSkill;
		gainLevelOverlap = other.gainLevelOverlap;
		gainFactor = other.gainFactor;
	}
}

