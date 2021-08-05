using System.Collections;
using UnityEngine;

public class MotivationTrigger : MonoBehaviour {

	public enum WhenToInject { EnterTrigger, ExitTrigger };
	[SerializeField] GameObject[] characterFilter;
	[SerializeField] WhenToInject whenToInject;
	[SerializeField] WOAMotivation[] motivation;
	[SerializeField] float delayInject = 0.2f;
	[SerializeField] float sleepAfterInject = 2f;
	float timeInjected = 0;

	public void OnTriggerEnter(Collider other) {
		if (motivation.Length > 0 && whenToInject == WhenToInject.EnterTrigger) {
			injectMotivations (other.gameObject);
		}
	}

	public void OnTriggerExit(Collider other) {
		if (motivation.Length > 0 && whenToInject == WhenToInject.ExitTrigger) {
			injectMotivations (other.gameObject);
		}
	}

	void injectMotivations(GameObject go) {
		if (affects (go)) {
			IMotivationController nmc = go.GetComponent<IMotivationController> ();
			if (nmc != null) {
				timeInjected = Time.time;
				StartCoroutine (DelayedInjection (delayInject, nmc, motivation));
			}
		}
	}

	bool affects(GameObject go) {
		if (timeInjected > 0 && Time.time < (timeInjected + sleepAfterInject))
			return false;
		if (characterFilter.Length > 0) {
			foreach (GameObject g in characterFilter) {
				if (go.Equals (g))
					return true;
			}
			return false;
		} else
			return true;
	}

	IEnumerator DelayedInjection(float delay, IMotivationController controller, WOAMotivation[] motivations) {
		yield return new WaitForSeconds (delay);
		for (int i = motivations.Length - 1; i >= 0; i--) {
			controller.injectMotivation (motivations [i]);
		}
	}
}
