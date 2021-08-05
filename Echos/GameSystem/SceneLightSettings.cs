using System;
using UnityEngine;

[CreateAssetMenu (fileName = "New Light Settings", menuName = "Scenes/LightSettings")]
public class SceneLightSettings : ScriptableObject
{
	public String sceneName;
	public float ambientIntensity;
}

