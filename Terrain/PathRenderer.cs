using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class PathRenderer : MonoBehaviour
{
	public RecordedLocations recorder;
	public GameObject pathObjectTemplate;
	LineRenderer lineRenderer;

	void Start() {
		lineRenderer = GetComponent<LineRenderer> ();
		Vector3[] arr = new Vector3[recorder.locations.Count];
		int i = 0;
		foreach (RecordedLocations.NamedPosition loc in recorder.locations) {
			arr [i++] = loc.position;
		}
		lineRenderer.positionCount = i;
		lineRenderer.SetPositions (arr);
	}
}

