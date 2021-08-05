using UnityEngine;

public class TransportTrigger : MonoBehaviour {

	[SerializeField] private Transform targetLocation;
	[SerializeField] private string loadScene;
	[SerializeField] private string unloadScene;

	public void OnTriggerEnter(Collider other) {
		if (other.gameObject.tag == "Player") {
			if (loadScene != "") {
				GameSystem.gameSystem.switchScenes (loadScene, unloadScene);
			}
			other.gameObject.transform.position = targetLocation.position;
			other.gameObject.transform.rotation = targetLocation.rotation;
		}
	}
}
