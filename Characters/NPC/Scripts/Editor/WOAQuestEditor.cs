using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(WOAQuestID))]
public class WOAQuestEditor : PropertyDrawer
{
	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
		EditorGUI.BeginProperty(position, label, property);
		string questId = property.FindPropertyRelative("id").stringValue;
		int idx = WOAQuestRegistry.IndexOf (questId);
		int newIndex = EditorGUI.Popup (position, idx, WOAQuestRegistry.QuestNames);
		if (newIndex != idx) {
			property.FindPropertyRelative("id").stringValue = WOAQuestRegistry.Quest (newIndex).id;
		}
		EditorGUI.EndProperty();
	}
}

