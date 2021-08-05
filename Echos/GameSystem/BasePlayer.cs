using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class BasePlayer : BaseCharacter
{
	public List<WOAItem> commands { get { return volatileData.commands; } }
	public List<WOAQuestState> questStates { get { return volatileData.questStates; } }
	public List<WOAQuest> quests;
	DialogUI dialogUI;

	public delegate void OnQuestsChanged ();

	public OnQuestsChanged onQuestsChangedCallback;

	public override void InitializePlayer () {
		quests = new List<WOAQuest> ();
		//questStates.Add (new WOAQuestState ("dummy1", "", true));
		//questStates.Add (new WOAQuestState ("dummy2", ""));
		foreach (WOAQuestState questState in questStates) {
			WOAQuest quest = WOAQuestRegistry.QuestInstance (questState.id);
			//Debug.Log ("Initialize quest " + questState.id + " to " + quest);
			quest.Initialize (this, questState.state);
			quests.Add (quest);
		}
		dialogUI = GameSystem.gameSystem.canvas.GetComponentInChildren<DialogUI> (true);
	}

	public bool CanRunQuest(string questId) {
		bool isUnknownQuest = true;
		foreach (WOAQuest quest in quests) {
			if (quest.id.Equals(questId)) {
				isUnknownQuest = false;
				if (quest.CanRunQuest (this))
					return true;
			}
		}
		if (isUnknownQuest) {
			WOAQuest newQuest = WOAQuestRegistry.QuestInstance (questId);
			return newQuest.CanRunQuest (this);
		}
		return false;
	}

	public void RunQuest(string questId, BaseCharacter actor) {
		foreach (WOAQuest quest in quests) {
			if (quest.id.Equals(questId)) {
				quest.Run (dialogUI, this, actor);
				return;
			}
		}
		WOAQuest newQuest = WOAQuestRegistry.QuestInstance (questId);
		if (newQuest != null) {
			quests.Add (newQuest);
			if (onQuestsChangedCallback != null)
				onQuestsChangedCallback.Invoke ();
			newQuest.Run (dialogUI, this, actor);
		}
	}

	public override void EarnMoney(WOAMoney m) {
		base.EarnMoney (m);
		gameObject.GetComponent<PlayerSoundController> ().PlayOneShot (PlayerSoundController.SoundEvent.EarnMoney);
	}

	public override void AnnounceSkillGain(WOASkill skill, float gain, int gainedIntLevel) {
		if (gain > Mathf.Epsilon) {
			if (gainedIntLevel > 0)
				GameConsole.gameConsole.println (GameConsole.InfoLevel.General, string.Format ("You trained the skill {0} and gained a level.", skill.name));
			else
				GameConsole.gameConsole.println (GameConsole.InfoLevel.General, string.Format ("You trained the skill {0}.", skill.name));
		} else {
			GameConsole.gameConsole.println (GameConsole.InfoLevel.General, string.Format ("Your skill {0} hasn't changed.", skill.name));
		}
	}

	public void setCommand(int index, WOAItem item) {
		if (index < 1) {
			Debug.LogError ("Cannot set command item in slot " + index);
			return;
		}
		while (commands.Count < index)
			commands.Add (null);
		commands [index - 1] = item;
	}

	public override int causeDamage (BaseCharacter enemy, WOAWeapon item)
	{
		int dmgP = base.causeDamage (enemy, item);
		GameConsole.gameConsole.println (GameConsole.InfoLevel.FineDetail, string.Format ("You hit {0} with {1} ({2} damage)", enemy.characterName, item.itemName, dmgP));
		if (enemy.isDead && dmgP > 0) {
			GameConsole.gameConsole.println (GameConsole.InfoLevel.Detail, enemy.characterName + " is dead.");
		}
		return dmgP;
	}

	public override bool CanPickup(WOAItem item) {
		bool result = base.CanPickup (item);
		if (!result)
			GameConsole.gameConsole.println (GameConsole.InfoLevel.General, string.Format ("You cannot pick up this thing."));
		return result;
	}

	public override bool pickup (WOAItem item)
	{
		bool pickedUp = base.pickup (item);
		if (!pickedUp)
			GameConsole.gameConsole.println (GameConsole.InfoLevel.General, string.Format ("You cannot pick up {0}, inventory is full.", item.itemName));
		else {
			GameConsole.gameConsole.println (GameConsole.InfoLevel.General, string.Format ("You picked up {0}.", item.itemName));
			gameObject.GetComponent<PlayerSoundController> ().PlayOneShot (PlayerSoundController.SoundEvent.Pickup);
		}
		return pickedUp;
	}

	public override void FollowTarget(bool follow) {
		if (follow)
			GetComponent<IPlayerController> ().Follow(target.transform);
		else
			GetComponent<IPlayerController> ().Follow(null);
	}

	public bool IsFollowing() {
		return GetComponent<IPlayerController> ().isFollowing;
	}

	public WOAQuestState GetQuestState(WOAQuest quest) {
		return GetQuestState(quest.id);
	}

	public WOAQuestState GetQuestState(string questId) {
		foreach (WOAQuestState state in questStates) {
			if (state.id.Equals(questId))
				return state;
		}
		return null;
	}

	public WOAQuest GetQuest(string questId) {
		foreach (WOAQuest quest in quests) {
			if (quest.id.Equals(questId)) {
				return quest;
			}
		}
		return null;
	}

	public void SetQuestState(string questId, string newState) {
		foreach (WOAQuestState state in questStates) {
			if (state.id.Equals (questId)) {
				state.state = newState;
				return;
			}
		}
		questStates.Add (new WOAQuestState (questId, newState));
		if (onQuestsChangedCallback != null)
			onQuestsChangedCallback.Invoke ();
	}
}
