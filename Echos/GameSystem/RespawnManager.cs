using System;
using System.Collections.Generic;
using UnityEngine;

public class RespawnManager : MonoBehaviour {

	static RespawnManager instance;

	[Serializable]
	struct Respawnable {
		public GameObject go;
		public float time;
		public Respawnable(GameObject go, float time) {
			this.go = go;
			this.time = time;
		}
	}

	List<Respawnable> items = new List<Respawnable>();
	float nextCheck = 0;

	void Awake() {
		instance = this;
	}

	void Update () {
		if (Time.time >= nextCheck)
			CheckRespawnables ();
	}

	void CheckRespawnables() {
		nextCheck += 60;
		if (items.Count > 0) {
			List<Respawnable> newList = new List<Respawnable> ();
			foreach (Respawnable r in items) {
				if (r.time <= Time.time) {
					if (r.go != null) {
						r.go.SetActive (true);
					}
				} else {
					if (r.go != null) {
						newList.Add (r);
						if (r.time < nextCheck)
							nextCheck = r.time;
					}
				}
			}
			items = newList;
		}
	}

	void RespawnInternal(GameObject go, float delay) {
		float timeToActivate = Time.time + delay;
		if (timeToActivate < nextCheck)
			nextCheck = timeToActivate;
		items.Add (new Respawnable (go, timeToActivate));
	}

	public static void Respawn(GameObject go, float delay) {
		instance.RespawnInternal (go, delay);
	}
}
