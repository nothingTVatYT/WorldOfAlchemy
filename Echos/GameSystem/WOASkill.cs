using System;
using UnityEngine;

[Serializable]
public class WOASkill
{
	public string name;
	public string description;
	public string id;
	[HideInInspector]
	public float currentLevel;
	[HideInInspector]
	public string templateClass;
	[HideInInspector]
	public string templateName;

	public WOASkill() {}
	public WOASkill(WOASkill other) {
		name = other.name;
		description = other.description;
		id = other.id;
		currentLevel = other.currentLevel;
	}
}

