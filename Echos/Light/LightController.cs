using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightController : MonoBehaviour, Interactable {

	public GameObject lightSource;
	public float percentageOff = 20f;
	public float intensityOff = 0.1f;
	public float intensityOn = 1f;
	public float flickerInterval = 0.1f;
	public float switchOnFadeTime = 4;
	[SerializeField] private ParticleSystem[] particles;
	public bool lightIsOn = true;
	private bool switchingOn;
	private float lastTime;
	private float timeSwitchedOn;
	private Light lightComponent;

	// Use this for initialization
	void Start () {
		lightComponent = lightSource.GetComponent<Light> ();
		if (lightIsOn) {
			switchOn ();
			switchingOn = false;
		} else {
			switchOff ();
		}
	}

	public void switchOn() {
		lightIsOn = true;
		lightComponent.enabled = true;
		switchingOn = true;
		timeSwitchedOn = Time.time;
		foreach (ParticleSystem ps in particles) {
			ps.Play ();
		}
		AudioSource audioSource = GetComponent<AudioSource>();
		if (audioSource != null)
			audioSource.Play ();
	}

	public void switchOff() {
		lightIsOn = false;
		switchingOn = false;
		lightComponent.enabled = false;
		foreach (ParticleSystem ps in particles) {
			ps.Stop ();
		}
		AudioSource audioSource = GetComponent<AudioSource>();
		if (audioSource != null)
			audioSource.Stop ();
	}

	// Update is called once per frame
	void Update () {
		if (lightIsOn) {
			float intensityFactor = 1f;
			if (switchingOn) {
				intensityFactor = (Time.time - timeSwitchedOn) / switchOnFadeTime;
				if (intensityFactor > 1) {
					intensityFactor = 1;
					switchingOn = false;
				}
			}
			// slow sin wave
			//light.GetComponent<Light> ().intensity = Mathf.Sin (Time.time) * 0.5f + 0.5f;
			// flicker
			if ((Time.time - lastTime) > flickerInterval) {
				lightComponent.intensity = intensityFactor * (Random.Range (0, 100) <= percentageOff ? intensityOff : intensityOn);
				lastTime = Time.time;
			}
		}
	}

	public void use(BaseCharacter actor) {
		if (actor.getEquippedLight () != null)
			switchOn ();
	}
}
