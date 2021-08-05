using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Granate : MonoBehaviour, IWeaponObject {

	private BaseCharacter actor;
	private GameObject target;
	private BaseCharacter npc;
	private float launchTime;
	private bool targetHit = false;
	private Vector3 scaleFactor;
	[SerializeField] private float speed = 4;
	[SerializeField] private float hitDistance = 2f;
	[SerializeField] private WOAWeapon weapon;
	[SerializeField] private GameObject effect;
	[SerializeField] private GameObject mesh;
	[SerializeField] private float rotationSpeed;
	[SerializeField] private Vector3 rotationWhenFlying;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if (targetHit)
			return;
		if (target != null) {
			float distance = Vector3.Distance (target.transform.position, transform.position);
			if (distance <= hitDistance) {
				if (actor.causeDamage (npc, weapon) > 0) {
					targetHit = true;
					if (effect != null) {
						GameObject effectObject = Instantiate (effect);
						effectObject.transform.position = transform.position;
						StartCoroutine (EffectUnloader (effectObject, 2));
						AudioSource audio = GetComponent<AudioSource> ();
						if (audio != null)
							audio.Play ();
					} else
						Destroy (gameObject);
				} else
					Destroy (gameObject);
			} else {
				Vector3 direction = (target.transform.position + Vector3.up * 0.8f) - transform.position;
				transform.rotation = Quaternion.LookRotation (direction);
				Vector3 moveDir = transform.TransformDirection (Vector3.forward) * speed * Time.deltaTime;
				transform.position += moveDir;
				if (mesh != null && rotationSpeed > 0) {
					float s = rotationSpeed * 360f * Time.deltaTime;
					scaleFactor.Set (s, s, s);
					mesh.transform.localRotation *= Quaternion.Euler(Vector3.Scale(rotationWhenFlying,scaleFactor));
				}
			}
		}
	}

	public void attackTarget(GameObject actorGameObject, GameObject enemy) {
		actor = actorGameObject.GetComponent<BaseCharacter>();
		target = enemy;
		npc = enemy.GetComponent<BaseCharacter> ();
	}

	IEnumerator EffectUnloader(GameObject effect, float delayTime)
	{
		yield return new WaitForSeconds(delayTime);
		Destroy (effect);
		Destroy (gameObject);
	}

}
