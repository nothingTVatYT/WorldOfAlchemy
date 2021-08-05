using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(VillageCreator))]
public class VillageCreatorEditor : Editor
{
	public override void OnInspectorGUI() {
		DrawDefaultInspector();
		VillageCreator vc = target as VillageCreator;
		if (GUILayout.Button("Start genetic algorithm")) {
			vc.StartGeneticAlgorithm ();
		}
		if (GUILayout.Button("Start simulated annealing")) {
			vc.StartSimulatedAnnealing ();
		}
		if (GUILayout.Button("Reset locations")) {
			vc.Reset ();
		}
	}
}

