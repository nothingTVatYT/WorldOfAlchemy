using UnityEngine;

public class ItemHandler : MonoBehaviour, Interactable
{

	[SerializeField] WOAItemDefaults item;
	[SerializeField] float pickupDistance = 3;
	[SerializeField] float timeToRespawn = 10f;

	public void use (BaseCharacter actor)
	{
		if (item != null && Vector3.Distance(transform.position, actor.gameObject.transform.position) < pickupDistance) {
			if (actor.pickup (item.CreateInstance())) {
				RespawnManager.Respawn (gameObject, timeToRespawn);
				gameObject.SetActive (false);
			}
		}
	}

	public void OnMouseDown() {
		use (BaseCharacter.getCharacterFromGameObject (GameSystem.gameSystem.player));
	}
}
