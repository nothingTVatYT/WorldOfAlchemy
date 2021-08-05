using System;
using UnityEngine;

[Serializable]
public class WOAItemWithCooldown : WOAItem
{
	private float lastUsed;

	public float coolDown;

	public float coolingProgress {
		get {
			if (lastUsed > 0 && Time.time - lastUsed < coolDown) {
				return (Time.time - lastUsed) / coolDown;
			} else
				return 1;
		}
	}

	public WOAItemWithCooldown() {
	}

	public WOAItemWithCooldown(WOAItemWithCooldown other) :base(other) {
		coolDown = other.coolDown;
	}

	public void startCooldown() {
		lastUsed = Time.time;
	}

	public void resetCooldown() {
		lastUsed = 0;
	}
}

