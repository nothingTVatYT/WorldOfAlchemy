using System;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;

[Serializable]
public class BaseCharacter : MonoBehaviour
{
	public delegate void OnEffectChanged (WOAEffect removedEffect, WOAEffect newEffect);

	public OnEffectChanged onEffectChangedCallback;

	public delegate void OnItemEquipChanged (WOAItem item, bool equipped);

	public OnItemEquipChanged onItemEquipChangedCallback;

	public delegate void OnItemChanged ();

	public OnItemChanged onItemChangedCallback;

	public delegate void OnSpellsChanged (WOASpell newSpell);

	public OnSpellsChanged onSpellsChangedCallback;

	public delegate void OnSkillsChanged (WOASkill changedSkill);

	public OnSkillsChanged onSkillsChangedCallback;

	public delegate void OnTargetChanged ();

	public OnTargetChanged onTargetChangedCallback;

	public new string name;

	public int effectiveMaxHP { get { return volatileData.effectiveMaxHP; } set { volatileData.effectiveMaxHP = value; } }

	public int currentHP { get { return volatileData.currentHP; } set { volatileData.currentHP = value; } }

	public float timeDied { get { return volatileData.timeDied; } set { volatileData.timeDied = value; } }

	public int inventoryCount { get { return volatileData.inventory.Count; } }

	public int spellsCount { get { return volatileData.spells.Count; } }

	public string characterName { get { return name; } }

	public WOAMoney money { get { return volatileData.money; } }

	public BaseCharacterDefaults defaults;
	public BaseCharacterVolatile volatileData = new BaseCharacterVolatile ();

	private GameObject targetObject;

	public GameObject target {
		get { return targetObject; }
		set {
			targetObject = value;
			if (onTargetChangedCallback != null)
				onTargetChangedCallback.Invoke ();
		}
	}

	public virtual void FollowTarget (bool follow)
	{
		NPCMotivationController npc = gameObject.GetComponent<NPCMotivationController> ();
		if (npc != null) {
			if (follow)
				npc.injectMotivation (new WOAMotivation (WOAMotivation.ActionType.FollowTarget));
			else
				npc.StopFollowTarget ();
		}
	}

	public bool isDead { get { return currentHP <= 0; } }

	public bool isInvisible { get { return volatileData.isInvisible; } set { volatileData.isInvisible = value; } }

	public void Start ()
	{
		if (defaults != null) {
			volatileData.currentHP = defaults.maxHP;
			if (defaults is BaseNPCDefaults)
				InitializeFromDefaults ();
			else
				InitializePlayer ();
		}
	}

	public void Update ()
	{
		expireEffects ();
	}

	public virtual void InitializePlayer ()
	{
	}

	public void InitializeFromDefaults ()
	{
		if (defaults != null) {
			volatileData.name = name;
			volatileData.currentHP = defaults.maxHP;
			volatileData.effectiveMaxHP = defaults.maxHP;
			volatileData.inventory.Clear ();
			foreach (WOAKeyDefaults k in defaults.keys) {
				volatileData.inventory.Add (k.CreateInstance ());
			}
			foreach (WOALightDefaults k in defaults.lights) {
				volatileData.inventory.Add (k.CreateInstance ());
			}
			foreach (WOAWeaponDefaults k in defaults.weapons) {
				volatileData.inventory.Add (k.CreateInstance ());
			}
			volatileData.money.Add (defaults.money);
			updateEffectiveValues ();
		} else
			Debug.LogError ("Character cannot be initialized: defaults is null");
	}

	public WOAItem inventory (int index)
	{
		return volatileData.inventory [index];
	}

	public WOASpell Spell (int index)
	{
		return volatileData.spells [index];
	}

	public WOASpell Spell(string id) {
		foreach (WOASpell spell in volatileData.spells) {
			if (id == spell.id)
				return spell;
		}
		return null;
	}

	public void addSpell (WOASpell newSpell)
	{
		foreach (WOASpell mySpell in volatileData.spells)
			if (mySpell.itemName.Equals (newSpell.itemName)) {
				Debug.Log (newSpell + " is already in your spellbook");
				return;
			}
		volatileData.spells.Add (newSpell);
		if (onSpellsChangedCallback != null)
			onSpellsChangedCallback.Invoke (newSpell);
	}

	public void updateEffectiveValues ()
	{
		bool visible = true;
		float newEffectiveMaxHP = defaults.maxHP;
		if (effectiveMaxHP == 0)
			effectiveMaxHP = defaults.maxHP;
		foreach (WOAEffect effect in volatileData.effects) {
			if (effect.makesInvisible)
				visible = false;
			if (Mathf.Abs (effect.hpBonus.value) > 0) {
				if (effect.hpBonus.inPercent) {
					newEffectiveMaxHP += effect.hpBonus.value / 100f * defaults.maxHP;
				} else {
					newEffectiveMaxHP += effect.hpBonus.value;
				}
			}
		}
		if (newEffectiveMaxHP > effectiveMaxHP) {
			// increase current hp so that it's at the same relative value
			currentHP = Mathf.RoundToInt ((float)currentHP / effectiveMaxHP * newEffectiveMaxHP);
		}
		effectiveMaxHP = Mathf.RoundToInt (newEffectiveMaxHP);
		if (currentHP > effectiveMaxHP)
			currentHP = effectiveMaxHP;
		isInvisible = visible;
	}

	public void resetCooldown ()
	{
		foreach (WOAItem item in volatileData.inventory)
			if (item is WOAItemWithCooldown)
				((WOAItemWithCooldown)item).resetCooldown ();
		foreach (WOAItem item in volatileData.spells)
			if (item is WOAItemWithCooldown)
				((WOAItemWithCooldown)item).resetCooldown ();
	}

	public bool hasLightEquipped ()
	{
		foreach (WOAItem item in volatileData.equippedItems)
			if (item is WOALight)
				return true;
		return false;
	}

	public bool canUnlock (string KeyCode)
	{
		foreach (WOAItem item in volatileData.inventory) {
			WOAKey key = item as WOAKey;
			if (key != null && key.canUnlock (KeyCode))
				return true;
		}
		return false;
	}

	public virtual bool pickup (WOAItem item)
	{
		if (!CanPickup (item))
			return false;
		bool couldPickup = false;
		// WOASubstance is stackable
		if (item is WOASubstance) {
			Harvest (item as WOASubstance);
			foreach (WOAItem it in volatileData.inventory) {
				WOASubstance substance = it as WOASubstance;
				if (substance != null && substance.currentStackSize < substance.maxStackSize) {
					substance.currentStackSize++;
					couldPickup = true;
				}
			}
		}
		if (!couldPickup && volatileData.inventory.Count < defaults.maxInventoryItems) {
			volatileData.inventory.Add (item);
			couldPickup = true;
		}
		if (couldPickup) {
			if (onItemChangedCallback != null)
				onItemChangedCallback.Invoke ();
			return true;
		}
		return false;
	}

	public bool Harvest(WOASubstance substance) {
		if (substance == null)
			return false;
		if (substance.neededSkill != "") {
			WOASkill characterSkill = Skill (substance.neededSkill);
			if (substance.level > 0 && (characterSkill == null || characterSkill.currentLevel < substance.level))
				return false;
			if (characterSkill == null)
				characterSkill = AddSkill (substance.neededSkill);
			AdaptSkill (characterSkill, substance.level, substance.gainFactor, substance.gainLevelOverlap);
		}
		return true;
	}

	public WOASkill AddSkill(string skillId) {
		WOASkill newSkill = SkillRegistry.registry.SkillTemplate (skillId).CreateInstance ();
		volatileData.skills.Add (newSkill);
		return newSkill;
	}

	public void AddAndIncreaseSkill(string skillId, int sourceLevel, float gainFactor) {
		WOASkill skill = Skill (skillId);
		if (skill == null)
			skill = AddSkill (skillId);
		AdaptSkill (skill, sourceLevel, gainFactor, 1);
	}

	public void AdaptSkill(WOASkill skill, int sourceLevel, float gainFactor, float gainLevelOverlap) {
		// =B2+SQRT(MAX(0,(C2+$Q$1)-B2))*$Q$2
		float gain = Mathf.Sqrt(Mathf.Clamp01(sourceLevel + gainLevelOverlap - skill.currentLevel)) * gainFactor;
		int intLevelBefore = Mathf.FloorToInt (skill.currentLevel);
		skill.currentLevel += gain;
		int intLevelCurrent = Mathf.FloorToInt (skill.currentLevel);
		AnnounceSkillGain (skill, gain, intLevelCurrent - intLevelBefore);
		if (onSkillsChangedCallback != null)
			onSkillsChangedCallback.Invoke (skill);
	}

	public WOASkill Skill(string skillId) {
		foreach (WOASkill skill in volatileData.skills) {
			if (skill.id.Equals(skillId)) {
				return skill;
			}
		}
		return null;
	}

	public WOASkill[] Skills() {
		return volatileData.skills.ToArray ();
	}

	public float SkillLevel(string skillId) {
		WOASkill skill = Skill (skillId);
		return skill == null ? 0f : skill.currentLevel;
	}

	public virtual void AnnounceSkillGain(WOASkill skill, float gain, int gainedIntLevel) {
	}

	public virtual bool CanPickup(WOAItem item) {
		WOASubstance substance = item as WOASubstance;
		if (substance != null && substance.level > 0 && substance.neededSkill != "" && SkillLevel (substance.neededSkill) < substance.level)
			return false;
		return true;
	}

	public int ItemCount(string substanceId) {
		int count = 0;
		foreach(WOAItem item in volatileData.inventory) {
			WOASubstance substance = item as WOASubstance;
			if (substance != null && substance.substanceId.Equals (substanceId))
				count += substance.currentStackSize;
		}
		return count;
	}

	public virtual int GiveItems(string substanceId, int count, WOAMoney priceTotal) {
		int needed = count;
		List<WOAItem> itemsToRemove = new List<WOAItem> ();
		for (int i = volatileData.inventory.Count; i > 0; i--) {
			WOASubstance substance = volatileData.inventory[i-1] as WOASubstance;
			if (substance != null && substance.substanceId.Equals (substanceId)) {
				if (substance.currentStackSize >= needed) {
					substance.currentStackSize -= needed;
					needed = 0;
					if (substance.currentStackSize == 0)
						itemsToRemove.Add (substance);
				} else {
					needed -= substance.currentStackSize;
					itemsToRemove.Add (substance);
				}
			}
		}
		foreach (WOAItem it in itemsToRemove)
			volatileData.inventory.Remove (it);
		if (priceTotal != null)
			EarnMoney (priceTotal);
		if (onItemChangedCallback != null)
			onItemChangedCallback.Invoke ();
		return count - needed;
	}

	public virtual void EarnMoney(WOAMoney m) {
		volatileData.money.Add (m);
	}

	public void RunSpell(WOASpell spell) {
		StartCoroutine (RunSpellInternal (spell));
	}

	IEnumerator RunSpellInternal(WOASpell spell) {
		GameObject aeObject = null;
		if (spell.actorEffect != null) {
			GameObject actorEffectObject = UnityEngine.Object.Instantiate (spell.actorEffect);
			ISpellEffect se = actorEffectObject.GetComponent<ISpellEffect> ();
			if (se != null) {
				se.setSpell (spell);
				se.attach (gameObject);
			} else {
				actorEffectObject.transform.position = transform.position;
				actorEffectObject.transform.rotation = transform.rotation * Quaternion.Euler(spell.actorEffectRotation);
				aeObject = actorEffectObject;
			}
		}
		spell.startCooldown ();
		yield return new WaitForSeconds (spell.spellPrepareDuration);
		if (aeObject != null)
			Destroy (aeObject);
		if (spell.targetEffect != null) {
			GameObject targetEffectObject = UnityEngine.Object.Instantiate (spell.targetEffect);
			ISpellEffect se = targetEffectObject.GetComponent<ISpellEffect> ();
			if (se != null) {
				se.setSpell (spell);
				se.attach (target);
			} else {
				targetEffectObject.transform.position = target.transform.position;
				targetEffectObject.transform.rotation = target.transform.rotation;
			}
		}
		if (spell.statsEffect != null) {
			spell.statsEffect.duration = spell.spellEffectDuration;
			spell.statsEffect.started = Time.time;
			addEffect (spell.statsEffect);
		}
		if (spell.teleportLocation != null) {
			GameSystem.gameSystem.teleportPlayer (spell.teleportLocation);
		}
	}

	void expireEffects ()
	{
		List<WOAEffect> toBeRemoved = volatileData.effects.FindAll (i => i.expired);
		foreach (WOAEffect effect in toBeRemoved) {
			if (onEffectChangedCallback != null)
				onEffectChangedCallback.Invoke (effect, null);
			volatileData.effects.Remove (effect);
		}
		if (toBeRemoved.Count > 0)
			updateEffectiveValues ();
	}

	public WOAWeapon getEquippedWeapon ()
	{
		foreach (WOAItem item in volatileData.equippedItems) {
			if (item is WOAWeapon)
				return (WOAWeapon)item;
		}
		return null;
	}

	public WOALight getEquippedLight ()
	{
		foreach (WOAItem item in volatileData.equippedItems) {
			if (item is WOALight)
				return (WOALight)item;
		}
		return null;
	}

	public void equip (WOAItem item)
	{
		if (item is WOALight) {
			bool equipped = false;
			WOALight lightItem = (WOALight)item;
			if (volatileData.equippedItems.Contains (item)) {
				volatileData.equippedItems.Remove (item);
			} else {
				volatileData.equippedItems.Add (item);
				equipped = true;
				if (lightItem.statsEffect != null)
					addEffect (lightItem.statsEffect);
			}
			if (!equipped && lightItem.statsEffect != null)
				removeEffect (lightItem.statsEffect);
			if (onItemEquipChangedCallback != null)
				onItemEquipChangedCallback.Invoke (item, equipped);
		}
	}

	public virtual void getsAttackedBy (BaseCharacter other)
	{
	}

	public virtual int causeDamage (BaseCharacter enemy, WOAWeapon item)
	{
		float dmg = item.damage;
		return enemy.takeDamage (dmg);
	}

	public int takeDamage (float damage)
	{
		int decrement = Mathf.RoundToInt (damage);
		currentHP -= decrement;
		if (currentHP <= 0) {
			timeDied = Time.time;
		}
		return decrement;
	}

	public void addEffect (WOAEffect effect)
	{
		if (!volatileData.effects.Contains (effect)) {
			volatileData.effects.Add (effect);
			if (onEffectChangedCallback != null)
				onEffectChangedCallback.Invoke (null, effect);
		}
		updateEffectiveValues ();
	}

	public void removeEffect (WOAEffect effect)
	{
		if (volatileData.effects.Contains (effect)) {
			volatileData.effects.Remove (effect);
			if (onEffectChangedCallback != null)
				onEffectChangedCallback.Invoke (effect, null);
		}
		updateEffectiveValues ();
	}

	public static BaseCharacter getCharacterFromGameObject (GameObject o)
	{
		if (o == null)
			return null;
		return o.GetComponent<BaseCharacter> ();
	}
}

