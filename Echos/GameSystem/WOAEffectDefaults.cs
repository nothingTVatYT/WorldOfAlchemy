using UnityEngine;

[CreateAssetMenu(fileName="New Effect", menuName="Spells and Effects/Effect")]
public class WOAEffectDefaults : ScriptableObject
{
	public WOAEffect values;

	public WOAEffect CreateInstance() {
		values.templateName = name;
		values.templateClass = GetType().AssemblyQualifiedName;
		return new WOAEffect (values);
	}
}

