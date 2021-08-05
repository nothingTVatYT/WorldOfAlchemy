using UnityEngine;

[CreateAssetMenu(fileName="New Spell", menuName="Spells and Effects/Spell")]
public class WOASpellDefaults : WOAItemDefaults
{
	public WOASpell values;
	public WOAEffectDefaults effect;

	#region implemented abstract members of WOAItemDefaults

	public override WOAItem CreateInstance ()
	{
		values.templateName = name;
		values.templateClass = GetType().AssemblyQualifiedName;
		WOASpell newSpell = new WOASpell (values);
		if (effect != null)
			newSpell.statsEffect = effect.CreateInstance();
		return newSpell;
	}

	#endregion
}

