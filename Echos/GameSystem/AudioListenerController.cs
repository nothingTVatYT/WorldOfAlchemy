using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioListenerController : MonoBehaviour {

	AudioListener audioListener;

	void Start () {
		audioListener = GetComponent<AudioListener>();
		GlobalGameState.gameState.registerAudioListener(audioListener);
	}

	void OnDestroy()
	{
		GlobalGameState.gameState.unregisterAudioListener(audioListener);
	}	
}
