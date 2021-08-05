using UnityEngine;

[CreateAssetMenu (fileName = "New Base Character Defaults", menuName = "Character/Base Character Defaults")]
public class BaseCharacterDefaults : ScriptableObject
{
	public int maxHP;
	public int maxInventoryItems;
	public WOAMoney money;
	public WOAWeaponDefaults[] weapons;
	public WOALightDefaults[] lights;
	public WOASpellDefaults[] spells;
	public WOAKeyDefaults[] keys;
	public WOAItem[] otherItems;
}

