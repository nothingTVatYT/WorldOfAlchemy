using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(paintTerrain))]
public class TerrainBuilderEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        paintTerrain myScript = (paintTerrain)target;
		if (GUILayout.Button("Paint Terrain (Steepness)"))
		{
			myScript.PaintTexturesForSteepness();
		}
		if (GUILayout.Button("Paint Terrain"))
		{
			myScript.PaintTextures();
		}
		if (GUILayout.Button ("Generate from grayscale map")) {
			myScript.GenerateFromGrayscale ();
		}
		if (GUILayout.Button ("Split terrain")) {
			myScript.SplitTerrain ();
		}
		if (GUILayout.Button ("Smooth heightmap")) {
			myScript.SmoothHeightmap ();
		}
        if (GUILayout.Button("Generate Terrain"))
        {
            myScript.Generate();
        }
    }
}
