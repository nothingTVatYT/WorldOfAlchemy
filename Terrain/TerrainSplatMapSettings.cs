using System;
using UnityEngine;

[CreateAssetMenu (fileName = "new Splat Map Settings", menuName = "Terrain/SplatMapSettings")]
public class TerrainSplatMapSettings : ScriptableObject
{
	public delegate void OnSettingsChanged();
	public OnSettingsChanged onSettingsChanged;

	[System.Serializable]
	public class SplatHeights
	{
		public int textureIndex;
		public int startingHeight;
		public int overlap;
	}

	public SplatHeights[] splatHeights;
	[Range(0.01f, 1f)]
	public float steepnessThreshold = 0.6f;

	public void OnValidate() {
		if (onSettingsChanged != null)
			onSettingsChanged.Invoke ();
	}
}

