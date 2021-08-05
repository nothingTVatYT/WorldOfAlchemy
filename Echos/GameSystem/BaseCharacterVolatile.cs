using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class BaseCharacterVolatile
{
	// for deserializing
	[HideInInspector]
	public string templateName;
	// for BaseCharacter
	public string name;
	public int currentHP;
	public int effectiveMaxHP;
	public float timeDied;
	public bool isInvisible;
	public WOAMoney money = new WOAMoney();
	public List<WOAEffect> effects = new List<WOAEffect> ();
	public List<WOAItem> inventory = new List<WOAItem> ();
	public List<WOAItem> equippedItems = new List<WOAItem> ();
	public List<WOASpell> spells = new List<WOASpell> ();
	public List<WOASkill> skills = new List<WOASkill> ();
	// for BasePlayer
	public List<WOAItem> commands = new List<WOAItem>(10);
	public List<WOAQuestState> questStates = new List<WOAQuestState>();

	public override string ToString ()
	{
		return string.Format ("[BaseCharacterVolatile name={0}, currentHP={1}, effectiveMaxHP={2}, timeDied={3}, isInvisible={4}, effects={5}, inventory={6}, equippedItems={7}, spells={8}]",
			name, currentHP, effectiveMaxHP, timeDied, isInvisible, effects, inventory, equippedItems, spells);
	}
}

