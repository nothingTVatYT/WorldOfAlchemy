using UnityEngine;

[CreateAssetMenu(fileName="New Light", menuName="Items/Light")]
public class WOALightDefaults : WOAItemDefaults
{
	public WOALight values = new WOALight ();
	public WOAEffectDefaults effect;

	#region implemented abstract members of WOAItemDefaults

	public override WOAItem CreateInstance ()
	{
		values.templateName = name;
		values.templateClass = GetType().AssemblyQualifiedName;
		WOALight newLight = new WOALight (values);
		newLight.statsEffect = effect.CreateInstance();
		return newLight;
	}

	#endregion
}
