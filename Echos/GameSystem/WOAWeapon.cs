using System;
using UnityEngine;

[Serializable]
public class WOAWeapon : WOAItemWithCooldown
{
	public enum WeaponType
	{
		OneHanded,
		Knife,
		Throwable
	}

	[SerializeField] public float range;
	[SerializeField] public float damage;
	[SerializeField] public float speed;
	[SerializeField] public WeaponType type;
	[SerializeField] public GameObject model;
	[SerializeField] public AudioClip[] throwSound;

	public WOAWeapon ()
	{
	}

	public WOAWeapon (WOAWeapon other) : base (other)
	{
		range = other.range;
		damage = other.damage;
		speed = other.speed;
		type = other.type;
		model = other.model;
		throwSound = other.throwSound;
	}

	public override bool use (GameObject actor)
	{
		BaseCharacter ch = actor.GetComponent<BaseCharacter> ();
		bool actorIsPlayer = ch != null && ch is BasePlayer;
		if (coolingProgress < 1) {
			if (actorIsPlayer)
				GameConsole.gameConsole.println (GameConsole.InfoLevel.General, itemName + " is not ready.");
			return false;
		}
		if (ch.target == null) {
			if (actorIsPlayer)
				GameConsole.gameConsole.println (GameConsole.InfoLevel.General, "You need a target to use " + itemName);
			return false;
		}
		float distance = Vector3.Distance (actor.transform.position, ch.target.transform.position);
		if (distance > range) {
			if (actorIsPlayer)
				GameConsole.gameConsole.println (GameConsole.InfoLevel.General, "Your target is too far away.");
			return false;
		}
		if (type == WeaponType.Throwable && model != null) {
			GameObject bullet = UnityEngine.Object.Instantiate (model);
			bullet.transform.position = actor.transform.position + Vector3.up * 0.3f + Vector3.right * 0.1f;
			IWeaponObject weaponObject = bullet.GetComponent<IWeaponObject> ();
			if (weaponObject != null) {
				weaponObject.attackTarget (actor, ch.target);
				startCooldown ();
				if (actorIsPlayer && throwSound.Length > 0)
					actor.GetComponent<AudioSource> ().PlayOneShot (randomThrowSound ());
			} else {
				Debug.LogError (bullet + " has no IWeaponObject attached");
				UnityEngine.Object.Destroy (bullet);
			}
		} else
			GameSystem.gameSystem.attackWith (this);
		return true;
	}

	public AudioClip randomThrowSound ()
	{
		if (throwSound.Length > 0)
			return throwSound [UnityEngine.Random.Range (0, throwSound.Length)];
		return null;
	}

	public override string ToString ()
	{
		return string.Format ("[WOAWeapon {0} type:{1} icon:{2} cooldown:{3} range:{4} damage:{5} speed:{6}]",
			itemName, type, icon, coolDown, range, damage, speed);
	}
}


