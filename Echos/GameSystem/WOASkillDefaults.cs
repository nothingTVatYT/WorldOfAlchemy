using UnityEngine;

[CreateAssetMenu(fileName = "New Skill", menuName = "Game Logic/Skill")]
public class WOASkillDefaults : ScriptableObject
{
	public WOASkill values;
	public WOASkill CreateInstance() {
		WOASkill newSkill = new WOASkill (values);
		newSkill.templateClass = GetType ().AssemblyQualifiedName;
		newSkill.templateName = name;
		return newSkill;
	}
}