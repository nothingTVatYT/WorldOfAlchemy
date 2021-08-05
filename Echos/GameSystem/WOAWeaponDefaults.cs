using UnityEngine;

[CreateAssetMenu(fileName="New Weapon", menuName="Items/Weapon")]
public class WOAWeaponDefaults : WOAItemDefaults
{
	public WOAWeapon values = new WOAWeapon ();

	#region implemented abstract members of WOAItemDefaults

	public override WOAItem CreateInstance ()
	{
		values.templateName = name;
		values.templateClass = GetType().AssemblyQualifiedName;
		return new WOAWeapon (values);
	}

	#endregion
}

