using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class BaseCharacterData
{
	static readonly char[] PIPE = { '|' };
	public string templateName;
	// for BaseCharacter
	public string name;
	public int currentHP;
	public int effectiveMaxHP;
	public float timeDied;
	public bool isInvisible;
	public string money = "";
	public List<string> effects = new List<string>();
	public List<string> inventory = new List<string>();
	public List<string> equippedItems = new List<string>();
	public List<string> spells = new List<string>();
	public List<string> skills = new List<string> ();
	// for BasePlayer
	public List<string> commands = new List<string>();
	public List<WOAQuestState> questStates = new List<WOAQuestState>();

	public BaseCharacterData() {
	}

	public BaseCharacterData(BaseCharacterVolatile v) {
		this.templateName = v.templateName;
		this.name = v.name;
		this.currentHP = v.currentHP;
		this.effectiveMaxHP = v.effectiveMaxHP;
		this.timeDied = v.timeDied;
		this.isInvisible = v.isInvisible;
		money = Serialize (v.money);
		foreach (WOAEffect e in v.effects)
			effects.Add(Serialize(e));
		foreach (WOAItem e in v.inventory)
			inventory.Add(Serialize(e));
		foreach (WOAItem e in v.equippedItems)
			equippedItems.Add(Serialize(e));
		foreach (WOASpell e in v.spells)
			spells.Add(Serialize(e));
		foreach (WOASkill e in v.skills)
			skills.Add(Serialize(e));
		foreach (WOAItem e in v.commands)
			commands.Add(Serialize(e));
		if (v.questStates != null)
			questStates = new List<WOAQuestState> (v.questStates);
		else
			questStates = new List<WOAQuestState> ();
	}

	public void Fill(BaseCharacterVolatile v) {
		v.templateName = templateName;
		v.name = name;
		v.currentHP = currentHP;
		v.effectiveMaxHP = effectiveMaxHP;
		v.timeDied = timeDied;
		v.isInvisible = isInvisible;
		v.money.Set(DeserializeMoney(money));
		v.effects.Clear ();
		foreach (string s in effects) {
			v.effects.Add(DeserializeEffect(s));
		}
		v.inventory.Clear ();
		foreach(string s in inventory) {
			v.inventory.Add (DeserializeItem (s));
		}
		v.equippedItems.Clear ();
		foreach(string s in equippedItems) {
			v.equippedItems.Add (DeserializeItem (s));
		}
		v.spells.Clear ();
		foreach(string s in spells) {
			v.spells.Add (DeserializeItem (s) as WOASpell);
		}
		v.skills.Clear ();
		if (skills != null) {
			foreach (string s in skills) {
				v.skills.Add (DeserializeSkill (s));
			}
		} else {
			v.skills = new List<WOASkill> ();
		}
		v.commands.Clear ();
		foreach (string s in commands) {
			v.commands.Add( DeserializeItem (s));
		}
		if (questStates != null)
			v.questStates = new List<WOAQuestState> (questStates);
		else
			v.questStates = new List<WOAQuestState> ();
	}

	public override string ToString ()
	{
		return string.Format ("[BaseCharacterData templateName={0}, name={1}, currentHP={2}, effectiveMaxHP={3}, timeDied={4}, isInvisible={5}, effects={6}, inventory={7}, equippedItems={8}, spells={9}, commands={10}]",
			templateName, name, currentHP, effectiveMaxHP, timeDied, isInvisible, String.Join(",", effects.ToArray()),
			String.Join(",", inventory.ToArray()), String.Join(",", equippedItems.ToArray()), String.Join(",", spells.ToArray()), String.Join(",", commands.ToArray()));
	}

	public static string Serialize(WOAEffect e) {
		if (e == null)
			return "";
		return e.templateClass + '|' + e.templateName;
	}

	public static string Serialize(WOASkill e) {
		if (e == null)
			return "";
		return e.templateClass + '|' + e.templateName + '|' + e.currentLevel;
	}

	public static string Serialize(WOAItem e) {
		if (e == null)
			return "";
		int count = 1;
		WOASubstance substance = e as WOASubstance;
		if (substance != null)
			count = substance.currentStackSize;
		return e.templateClass + '|' + e.templateName + "|" + count;
	}

	public static string Serialize(WOAMoney m) {
		if (m == null)
			return "";
		return m.gold + "|" + m.silver + "|" + m.copper;
	}

	public static WOAEffect DeserializeEffect(string s) {
		if (s == "")
			return null;
		int pos = s.IndexOf ('|');
		string className = s.Substring (0, pos);
		string name = s.Substring (pos + 1);
		UnityEngine.Object obj = GlobalGameState.Resolve (className, name);
		if (obj is WOAEffectDefaults)
			return ((WOAEffectDefaults)obj).CreateInstance ();
		return null;
	}

	public static WOAItem DeserializeItem(string s) {
		if (s == "" || s == "||1")
			return null;
		string[] part = s.Split(PIPE, 3);
		//Debug.Log ("Splitting " + s + " gave " + string.Join (", ", part));
		string className = part [0];
		string name = part [1];
		string countString = part.Length > 2 ? part [2] : "1";
		int count = int.Parse (countString);
		UnityEngine.Object obj = GlobalGameState.Resolve (className, name);
		WOAItemDefaults itemDefaults = obj as WOAItemDefaults;
		if (itemDefaults != null) {
			WOAItem item = itemDefaults.CreateInstance ();
			WOASubstance substance = item as WOASubstance;
			if (substance != null)
				substance.currentStackSize = count;
			return item;
		}
		return null;
	}

	public static WOASkill DeserializeSkill(string s) {
		if (s == "")
			return null;
		string[] part = s.Split (PIPE, 3);
		string className = part [0];
		string name = part [1];
		string levelString = part.Length > 2 ? part [2] : "0";
		float level = float.Parse (levelString);
		UnityEngine.Object obj = GlobalGameState.Resolve (className, name);
		WOASkillDefaults skillDefaults = obj as WOASkillDefaults;
		if (skillDefaults != null) {
			WOASkill skill = skillDefaults.CreateInstance ();
			skill.currentLevel = level;
			return skill;
		}
		return null;
	}

	public static WOAMoney DeserializeMoney(string s) {
		if (string.IsNullOrEmpty (s))
			return new WOAMoney ();
		string[] part = s.Split (PIPE, 3);
		return new WOAMoney (int.Parse (part [0]), int.Parse (part [1]), int.Parse (part [2]));
	}
}

