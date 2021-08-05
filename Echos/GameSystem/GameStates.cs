using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GameStates
{
	float lx,ly,lz;
	float rx, ry, rz;
	public Vector3 playerLocation { get { return new Vector3 (lx, ly, lz); } set { lx = value.x; ly = value.y; lz = value.z;}}
	public Vector3 playerRotation { get { return new Vector3 (rx, ry, rz); } set { rx = value.x; ry = value.y; rz = value.z;}}
	public List<string> scenes = new List<string>();
	public BaseCharacterData playerData;

	public override string ToString ()
	{
		return string.Format ("[GameStates playerLocation={0}, playerRotation={1}, scenes={2}, playerData={3}]",
			playerLocation, playerRotation, String.Join(",", scenes.ToArray()), playerData);
	}
}

