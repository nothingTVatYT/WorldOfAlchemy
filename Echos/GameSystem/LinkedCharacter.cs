using System;
using UnityEngine;

public class LinkedCharacter : MonoBehaviour
{
	[SerializeField] private BaseCharacter characterTemplate;
	private BaseCharacter characterClone;

	public void Update() {
		if (characterClone != null)
			characterClone.Update ();
	}
}

