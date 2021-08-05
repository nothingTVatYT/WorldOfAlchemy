using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellRegistry : MonoBehaviour {

	public static SpellRegistry registry;

	public List<WOASpellDefaults> spellTemplates = new List<WOASpellDefaults>();

	public void Awake() {
		registry = this;
	}

	public WOASpell CreateSpell(string spellId) {
		WOASpellDefaults defaults = SpellTemplate (spellId);
		if (defaults == null) {
			return null;
		}
		WOASpell newSpell = defaults.CreateInstance () as WOASpell;
		return newSpell;
	}

	public WOASpellDefaults SpellTemplate(string spellId) {
		foreach (WOASpellDefaults sd in spellTemplates) {
			if (sd.values.id.Equals (spellId))
				return sd;
		}
		return null;
	}
}
