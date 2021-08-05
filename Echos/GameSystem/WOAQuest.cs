using System;
using UnityEngine;

[Serializable]
public abstract class WOAQuest
{
	protected const string NONE = "";
	public abstract string id { get; }
	public abstract string name { get; }
	protected string currentState = "";
	protected BasePlayer player;
	protected BaseCharacter actor;
	protected DialogUI dialogUI;
	protected bool dismissed;
	protected string lastActorsName = "";

	protected abstract bool CanStartOrContinue (BasePlayer player, WOAQuestState state);

	protected void Dismiss() {
		dialogUI.CloseDialog ();
		if (actor != null) {
			NPCMotivationController npcm = actor.gameObject.GetComponent<NPCMotivationController> ();
			if (npcm != null)
				npcm.EndDialog ();
		}
	}

	protected void DismissAndLeadCharacter(string newState, Transform destination) {
		currentState = newState;
		player.SetQuestState (id, currentState);
		dialogUI.CloseDialog ();
		if (actor != null) {
			NPCMotivationController npcm = actor.gameObject.GetComponent<NPCMotivationController> ();
			if (npcm != null) {
				WOAMotivation newMotivation = new WOAMotivation (WOAMotivation.ActionType.WalkTo);
				newMotivation.destination = destination;
				npcm.EndDialog ();
				player.target = actor.gameObject;
				player.FollowTarget(true);
				npcm.injectMotivation (newMotivation);
			}
		}
	}

	protected void StopLeadingCharacter() {
		player.FollowTarget(false);
	}

	protected void AdvanceAndContinue(string newState) {
		currentState = newState;
		player.SetQuestState (id, currentState);
		Run (actor);
	}

	protected void AdvanceAndDismiss(string newState) {
		currentState = newState;
		player.SetQuestState (id, currentState);
		Dismiss ();
	}

	protected void Dialog(string text, string reply1, Action callback1, string reply2 = null, Action callback2 = null, string reply3 = null, Action callback3 = null) {
		dialogUI.ShowDialog (actor.characterName, text, reply1, callback1, reply2, callback2, reply3, callback3);
	}

	public bool CanRunQuest(BasePlayer player) {
		WOAQuestState state = player.GetQuestState (id);
		return CanStartOrContinue (player, state);
	}

	public virtual void Initialize (BasePlayer player, string state)
	{
		currentState = state;
		this.player = player;
	}

	protected string money(float amount) {
		return Mathf.RoundToInt (amount) + " gold";
	}

	public void Run (DialogUI dialogUI, BasePlayer player, BaseCharacter actor) {
		this.dialogUI = dialogUI;
		this.player = player;
		this.actor = actor;
		dismissed = false;
		lastActorsName = actor.characterName;
		Debug.Log ("initialized quest " + id + " with dialogUI=" + dialogUI);
		Run (actor);
	}

	public abstract void Run (BaseCharacter actor);

	public abstract string QuestState();
}

[Serializable]
public struct WOAQuestID {
	public string id;
}

