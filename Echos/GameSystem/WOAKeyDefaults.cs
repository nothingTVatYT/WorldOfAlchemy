using UnityEngine;

[CreateAssetMenu(fileName="New Key", menuName="Items/Key")]
public class WOAKeyDefaults : WOAItemDefaults
{
	public WOAKey values;

	#region implemented abstract members of WOAItemDefaults

	public override WOAItem CreateInstance ()
	{
		values.templateName = name;
		values.templateClass = GetType ().AssemblyQualifiedName;
		return new WOAKey (values);
	}

	#endregion
}

