using System;
using UnityEngine;

[Serializable]
public class WOALight : WOAItem
{
	public float lightRange;
	public Color lightColor;
	public float lightIntensity;
	public WOAEffect statsEffect;

	public WOALight ()
	{
	}

	public WOALight (WOALight other) :base(other)
	{
		lightRange = other.lightRange;
		lightColor = other.lightColor;
		lightIntensity = other.lightIntensity;
		statsEffect = other.statsEffect;
	}

	public override bool use (GameObject actor)
	{
		BaseCharacter ch = actor.GetComponent<BaseCharacter> ();
		if (ch != null)
			ch.equip (this);
		return true;
	}
}

