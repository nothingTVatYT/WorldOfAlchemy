using UnityEngine;

[CreateAssetMenu(fileName="New Substance", menuName = "Items/Substance")]
public class WOASubstanceDefaults : WOAItemDefaults
{
	public WOASubstance values;

	#region implemented abstract members of WOAItemDefaults
	public override WOAItem CreateInstance ()
	{
		values.templateName = name;
		values.templateClass = GetType ().AssemblyQualifiedName;
		return new WOASubstance (values);
	}
	#endregion
}

