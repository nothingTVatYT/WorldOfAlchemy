using UnityEngine;

public class PanelPrefs
{
	public static void SaveTransformPosition(string name, Transform transform) {
		PlayerPrefs.SetFloat (name + "-x", transform.position.x);
		PlayerPrefs.SetFloat (name + "-y", transform.position.y);
		PlayerPrefs.SetFloat (name + "-z", transform.position.z);
	}

	public static void RestoreTransformPosition(string name, Transform transform) {
		Vector3 v = transform.position;
		if (PlayerPrefs.HasKey(name + "-x")) {
			v.x = PlayerPrefs.GetFloat (name + "-x");
		}
		if (PlayerPrefs.HasKey(name + "-y")) {
			v.y = PlayerPrefs.GetFloat (name + "-y");
		}
		if (PlayerPrefs.HasKey(name + "-z")) {
			v.z = PlayerPrefs.GetFloat (name + "-z");
		}
		transform.position = v;
	}
}

