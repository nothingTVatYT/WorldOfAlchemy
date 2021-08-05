using UnityEngine;

public class SpellLearnTrigger : MonoBehaviour, Interactable {

	public WOASpellDefaults spellTemplate;

	#region Interactable implementation

	public void use (BaseCharacter actor)
	{
		if (spellTemplate != null && actor is BasePlayer) {
			actor.addSpell (spellTemplate.CreateInstance() as WOASpell);
		}
	}

	#endregion

	public void OnMouseDown() {
		use (GameSystem.gameSystem.player.GetComponent<BasePlayer> ());
	}
}
