using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu (fileName = "New Recorded Locations", menuName = "Terrain/Recorded Locations")]
public class RecordedLocations : ScriptableObject
{
	[System.Serializable]
	public class NamedPosition {
		[SerializeField] public string name;
		[SerializeField] public Vector3 position;
		[SerializeField] public Vector3 rotation;
		[SerializeField] public bool save;
	}

	public List<NamedPosition> locations = new List<NamedPosition>();

	public void addLocation(Transform here) {
		CleanUp ();
		NamedPosition newLocation = new NamedPosition ();
		newLocation.name = "New Location " + (locations.Count + 1);
		newLocation.position = here.position;
		newLocation.rotation = here.rotation.eulerAngles;
		locations.Add (newLocation);
	}

	void CleanUp() {
		List<NamedPosition> newList = locations.FindAll (x=>x.name != "");
		locations = newList;
	}
}

