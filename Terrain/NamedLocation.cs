using System;
using UnityEngine;

[CreateAssetMenu(fileName = "New Named Location", menuName = "Terrain/Named Location")]
public class NamedLocation : ScriptableObject
{
	[SerializeField] public new string name;
	[SerializeField] public string scene;
	[SerializeField] public Vector3 position;
	[SerializeField] public Vector3 rotation;
}

