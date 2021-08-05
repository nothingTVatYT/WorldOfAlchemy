using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(WOAMotivation))]
public class WOAMotivationEditor : PropertyDrawer
{
	float typeRectWidth = 75;
	float speedMultRectWidth = 30;

	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
		// Using BeginProperty / EndProperty on the parent property means that
		// prefab override logic works on the entire property.
		EditorGUI.BeginProperty(position, label, property);

		// Draw label
		position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

		// Don't make child fields be indented
		int indent = EditorGUI.indentLevel;
		EditorGUI.indentLevel = 0;

		Rect typeRect = new Rect (position.x, position.y, typeRectWidth, position.height);
		//Rect destinationRect = new Rect (position.x + typeRectWidth + spacing, position.y, position.width - spacing - typeRectWidth - speedMultRectWidth, 16);
		Rect destinationRect = new Rect (position.x, position.y + 18, position.width, 16);
		Rect speedMultRect = new Rect (position.x + position.width - speedMultRectWidth, position.y, speedMultRectWidth, 16);
		SerializedProperty actionType = property.FindPropertyRelative ("actionType");
		EditorGUI.PropertyField (typeRect, actionType, GUIContent.none);
		EditorGUI.PropertyField (destinationRect, property.FindPropertyRelative ("destination"), GUIContent.none);
		EditorGUI.PropertyField (speedMultRect, property.FindPropertyRelative ("speedMultiplier"), GUIContent.none);

		if (actionType.enumValueIndex == (int)WOAMotivation.ActionType.PickUp) {
			Rect checkBox1 = new Rect (position.x, position.y + 34, position.width, 16);
			Rect checkBox2 = new Rect (position.x, position.y + 50, position.width, 16);
			EditorGUI.PropertyField (checkBox1, property.FindPropertyRelative ("pickupControlledByAnimation"), new GUIContent("use animation event"));
			EditorGUI.PropertyField (checkBox2, property.FindPropertyRelative ("kneelDown"), new GUIContent("add kneel down animation"));
		}

		if (actionType.enumValueIndex == (int)WOAMotivation.ActionType.DropBasket) {
			Rect checkBox1 = new Rect (position.x, position.y + 34, position.width, 16);
			EditorGUI.PropertyField (checkBox1, property.FindPropertyRelative ("destroyOnDrop"), new GUIContent("destroy on drop"));
		}

		if (actionType.enumValueIndex == (int)WOAMotivation.ActionType.DropObject) {
			Rect prefab1 = new Rect (position.x, position.y + 34, position.width, 16);
			EditorGUI.PropertyField (prefab1, property.FindPropertyRelative ("objectPrefab"), GUIContent.none);
		}

		EditorGUI.indentLevel = indent;

		EditorGUI.EndProperty();
	}

	public override float GetPropertyHeight (SerializedProperty property, GUIContent label) {
		int lines = 3;
		WOAMotivation.ActionType selectedType = (WOAMotivation.ActionType)property.FindPropertyRelative ("actionType").enumValueIndex;
		switch (selectedType) {
		case WOAMotivation.ActionType.PickUp:
			lines = 4;
			break;
		case WOAMotivation.ActionType.WalkTo:
			lines = 2;
			break;
		case WOAMotivation.ActionType.GetBasket:
			lines = 2;
			break;
		case WOAMotivation.ActionType.DropObject:
		case WOAMotivation.ActionType.DropBasket:
			lines = 3;
			break;
		case WOAMotivation.ActionType.WalkAlong:
			lines = 2;
			break;
		}
		return 16 * lines + 2 * (lines - 1);
	}
}

