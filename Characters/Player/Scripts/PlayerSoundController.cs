using UnityEngine;

[RequireComponent (typeof(AudioSource))]
public class PlayerSoundController : MonoBehaviour {

	AudioSource audioSource;

	public enum SoundEvent {Effort, Pickup, EarnMoney}
	[System.Serializable]
	public class SoundFX {
		public AudioClip clip;
		public SoundEvent soundEvent;
	}

	[SerializeField] private SoundFX[] sounds;

	// Use this for initialization
	void Start () {
		audioSource = GetComponent<AudioSource> ();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void PlayOneShot(SoundEvent soundEvent) {
		for (int i = 0; i < sounds.Length; i++) {
			if (sounds [i].soundEvent == soundEvent) {
				audioSource.PlayOneShot (sounds [i].clip);
				break;
			}
		}
	}
}
