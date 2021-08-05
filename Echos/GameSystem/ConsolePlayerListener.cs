using UnityEngine;

public class ConsolePlayerListener : MonoBehaviour
{
	BasePlayer playerData;
	GameConsole console;
	bool initialized;

	public void Start() {
		console = GameConsole.gameConsole;
	}

	public void Update() {
		if (!initialized) {
			if (GameSystem.gameSystem.playerIsLoaded) {
				playerData = GameSystem.gameSystem.player.GetComponent<BasePlayer> ();
				playerData.onEffectChangedCallback += effectsChanged;
				playerData.onItemEquipChangedCallback += equipmentChanged;
				playerData.onSpellsChangedCallback += spellLearned;
				initialized = true;
			}
		}
	}

	void effectsChanged(WOAEffect removedEffect, WOAEffect newEffect) {
		if (removedEffect != null)
			console.println(GameConsole.InfoLevel.General, removedEffect.effectName + " has no longer an influence on you.");
		if (newEffect != null) {
			console.println (GameConsole.InfoLevel.General, "There is " + newEffect.effectName + " for you.");
		}
	}

	void spellLearned(WOASpell newSpell) {
		if (newSpell != null)
			console.println (GameConsole.InfoLevel.General, "You learned the spell \"" + newSpell.itemName + "\".");
	}

	void equipmentChanged(WOAItem item, bool equipped) {
		if (equipped)
			console.println(GameConsole.InfoLevel.Detail, "You are equipping " + item.itemName + ".");
		else
			console.println(GameConsole.InfoLevel.Detail, "You are putting " + item.itemName + " away.");
	}
}

