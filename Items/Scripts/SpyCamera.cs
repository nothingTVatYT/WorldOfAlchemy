using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpyCamera : MonoBehaviour, ISpellEffect {

	[SerializeField] private GameObject target;
	[SerializeField] private float rotationSpeed;
	[SerializeField] private float speed;
	private bool attaching;
	private float timeSummoned;
	private float lifeTime;

	// Use this for initialization
	void Start () {
		transform.position = GameSystem.gameSystem.player.transform.position;
		transform.rotation = GameSystem.gameSystem.player.transform.rotation;
	}
	
	// Update is called once per frame
	void Update () {
		if (target != null) {
			if (attaching) {
				Vector3 lookDirection = target.transform.position - transform.position;
				float distance = lookDirection.magnitude;
				if (distance > 1) {
					transform.rotation = Quaternion.Slerp (transform.rotation, Quaternion.LookRotation (lookDirection), rotationSpeed * Time.deltaTime);
					Vector3 step = transform.TransformDirection (Vector3.forward) * speed * Time.deltaTime;
					transform.position += step;
				} else
					attaching = false;
			} else {
				float duration = Time.time - timeSummoned;
				if (duration > lifeTime)
					Destroy(gameObject);
				transform.rotation = target.transform.rotation;
				transform.position = target.transform.position;
			}
		}
	}

	public bool attach(GameObject newTarget) {
		target = newTarget;
		attaching = true;
		timeSummoned = Time.time;
		return true;
	}

	public void setSpell(WOASpell spell) {
		lifeTime = spell.spellEffectDuration + 2;
	}
}
