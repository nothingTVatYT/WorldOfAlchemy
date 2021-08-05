using UnityEngine;

public class EnterWagonTrigger : MonoBehaviour {

	public PhysicMaterial forcedPlayerMaterial;
	PhysicMaterial originalMaterial;

	void OnTriggerEnter (Collider other) {
		if (other.gameObject.Equals(GameSystem.gameSystem.player)) {
			originalMaterial = other.material;
			other.material = forcedPlayerMaterial;
		}
	}
	
	void OnTriggerExit(Collider other) {
		if (other.gameObject.Equals(GameSystem.gameSystem.player)) {
			other.material = originalMaterial;
		}
	}
}
