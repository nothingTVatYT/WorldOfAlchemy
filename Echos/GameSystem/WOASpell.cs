using System;
using UnityEngine;

[Serializable]
public class WOASpell : WOAItemWithCooldown
{
	public enum SpellTarget { Self, Enemy }
	public string id;
	[SerializeField] SpellTarget spellTarget;
	[SerializeField] public float spellPrepareDuration;
	[SerializeField] public float spellEffectDuration;
	[SerializeField] public GameObject actorEffect;
	public Vector3 actorEffectRotation;
	[SerializeField] GameObject projectile;
	[SerializeField] public GameObject targetEffect;
	[SerializeField] public WOAEffect statsEffect;
	[SerializeField] public float immediateDamage;
	[SerializeField] public NamedLocation teleportLocation;

	public WOASpell(WOASpell other) :base(other) {
		id = other.id;
		spellTarget = other.spellTarget;
		spellPrepareDuration = other.spellPrepareDuration;
		spellEffectDuration = other.spellEffectDuration;
		actorEffect = other.actorEffect;
		actorEffectRotation = other.actorEffectRotation;
		projectile = other.projectile;
		targetEffect = other.targetEffect;
		statsEffect = new WOAEffect (other.statsEffect);
		immediateDamage = other.immediateDamage;
		teleportLocation = other.teleportLocation;
	}

	public override bool use (GameObject actor)
	{
		BaseCharacter ch = actor.GetComponent<BaseCharacter> ();
		bool actorIsPlayer = ch != null && ch is BasePlayer;
		if (coolingProgress < 1) {
			if (actorIsPlayer)
				GameConsole.gameConsole.println (GameConsole.InfoLevel.General, itemName + " is not ready yet.");
			return false;
		}
		if (spellTarget == SpellTarget.Enemy && ch.target == null) {
			if (actorIsPlayer)
				GameConsole.gameConsole.println (GameConsole.InfoLevel.General, "You need a target to attack.");
			return false;
		}
		ch.RunSpell (this);
		return true;
	}
}


