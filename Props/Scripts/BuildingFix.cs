using UnityEngine;

public class BuildingFix : MonoBehaviour {

	public AudioClip doorOpenSound;
	public AudioClip doorCloseSound;
	public float doorOpenSpeed = 1;
	public float audioMaxDistance = 10;
	public float audioMinDistance = 4;
	public float audioVolume = 0.6f;

	void Start () {
		HideCollisionMeshes ();
		InstallDoors (transform);
	}
	
	public void HideCollisionMeshes() {
		foreach (MeshRenderer r in GetComponentsInChildren<MeshRenderer>()) {
			if (r.name.StartsWith("Collision")) {
				r.enabled = false;
			}
		}
	}

	public void InstallAllDoors() {
		InstallDoors (transform);
	}

	void InstallDoors(Transform t) {
		for (int i = 0; i < t.childCount; i++) {
			Transform ch = t.GetChild (i);
			if (ch.name.StartsWith ("Door") || ch.name.StartsWith("VerticalShade")) {
				ch.gameObject.isStatic = false;
				BoxCollider boxCollider = null;
				foreach (Collider coll in ch.GetComponents<Collider> ()) {
					if (coll is BoxCollider) {
						if (boxCollider != null) {
							if (Application.isEditor) {
								DestroyImmediate (coll);
							} else {
								Destroy (coll);
							}
						} else {
							boxCollider = coll as BoxCollider;
						}
					}
					if (coll is MeshCollider) {
						if (Application.isEditor)
							DestroyImmediate (coll);
						else
							Destroy (coll);
					}
				}
				if (boxCollider == null)
					boxCollider = ch.gameObject.AddComponent<BoxCollider> ();
				boxCollider.isTrigger = true;
				ChestOpener co = ch.GetComponent<ChestOpener> ();
				if (co == null) {
					co = ch.gameObject.AddComponent<ChestOpener> ();
					co.openSpeed = doorOpenSpeed;
					if (ch.name.StartsWith ("VerticalShade")) {
						co.rotationOpened = new Vector3 (120f, 0, 0);
						co.initialState = ChestOpener.State.Open;
					} else
						co.rotationOpened = new Vector3 (0, 120f, 0);
					bool needAudioSource = false;
					if (doorOpenSound != null) {
						co.doorOpenSound = doorOpenSound;
						needAudioSource = true;
					}
					if (doorCloseSound != null) {
						co.doorCloseSound = doorCloseSound;
						needAudioSource = true;
					}
					if(needAudioSource) {
						AudioSource audioSource = ch.gameObject.GetComponent<AudioSource> ();
						if (audioSource == null) {
							audioSource = ch.gameObject.AddComponent<AudioSource> ();
							audioSource.maxDistance = audioMaxDistance;
							audioSource.minDistance = audioMinDistance;
							audioSource.spatialBlend = 1f;
							audioSource.volume = audioVolume;
						}
					}
				}
			} else {
				InstallDoors (ch);
			}

		}
	}
}
