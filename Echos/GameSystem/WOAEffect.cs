using System;
using UnityEngine;

[Serializable]
public class WOAEffect
{
	[Serializable]
	public struct StatsBonusMalus {
		public float value;
		public bool inPercent;
	}

	public Sprite icon;
	public string effectName;
	public float started;
	public float duration;
	public bool makesInvisible;
	public StatsBonusMalus hpBonus;
	public string templateName;
	public string templateClass;

	public float state { get { return 1f - (Time.time - started) / duration; } }
	public bool expired { get { return duration > 0 && Time.time > (started + duration); } }

	public WOAEffect() {}

	public WOAEffect(WOAEffect other) {
		icon = other.icon;
		effectName = other.effectName;
		makesInvisible = other.makesInvisible;
		hpBonus = other.hpBonus;
		templateName = other.templateName;
		templateClass = other.templateClass;
	}
}

