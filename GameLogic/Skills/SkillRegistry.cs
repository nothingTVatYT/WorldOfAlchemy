using System.Collections.Generic;
using UnityEngine;

public class SkillRegistry : MonoBehaviour {

	public static SkillRegistry registry;

	public List<WOASkillDefaults> skillTemplates = new List<WOASkillDefaults>();

	public void Awake() {
		registry = this;
	}

	public WOASkillDefaults SkillTemplate(string skillId) {
		foreach (WOASkillDefaults sd in skillTemplates) {
			if (sd.values.id.Equals (skillId))
				return sd;
		}
		return null;
	}
}
