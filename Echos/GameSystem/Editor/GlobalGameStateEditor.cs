using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GlobalGameState))]
public class GlobalGameStateEditor : Editor {

	public override void OnInspectorGUI() {
		DrawDefaultInspector ();
		if (GUILayout.Button("Save Game")) {
			GlobalGameState g = target as GlobalGameState;
			GameStates gameStates = g.CreateNewDefaultGameStates ("EditorTest1");
			g.SaveGame (gameStates);

			// testing
			g.Refresh();
			GameStates loaded = g.getAllSavedGames()[0];
		}
	}
}
