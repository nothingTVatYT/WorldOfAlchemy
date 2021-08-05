using UnityEngine;

public class QHealer1 : WOAQuest {
	const string INTRO = "intro";
	const string INTRO2 = "intro2";
	const string INTRO3 = "intro3";
	const string EXPLAIN1 = "explain1";
	const string EXPLAIN2 = "explain2";
	const string FETCH_HERBS = "fetch_herbs";
	const string SELL_HERBS = "sell_herbs";
	const string HERBS_SUBSTANCE_ID = "bf1";
	const string BEARFLOWER_LOCATION = "BearFlowers-scene-1-2";
	const string HARVEST_SKILL_ID = "harvest";
	WOAMoney price = new WOAMoney(0,0,6);

	#region implemented abstract members of WOAQuest

	protected override bool CanStartOrContinue (BasePlayer player, WOAQuestState state)
	{
		if (state == null || state.state == NONE) {
			return player.SkillLevel (HARVEST_SKILL_ID) < 2;
		}
		return true;
	}

	public override void Run (BaseCharacter actor)
	{
		Debug.Log ("current state: " + currentState + ", dialog panel at " + dialogUI.gameObject.transform.position);
		switch (currentState) {
		case NONE:
			Dialog ("Greetings, stranger.", "May I help you?", () => AdvanceAndContinue(INTRO), "Have a nice day.", Dismiss);
			break;
		case INTRO:
			Dialog ("Would you like to bring me some herbs?", "Sure.", () => AdvanceAndContinue(INTRO2), "Maybe later.", Dismiss);
			break;
		case INTRO2:
			Dialog ("Do you know the bear flower and where it grows?", "Yes.", () => AdvanceAndContinue (INTRO3), "No.", () => AdvanceAndContinue (EXPLAIN1));
			break;
		case EXPLAIN1:
			Dialog ("So follow me and I will tell you something about those.", "Thank you.", () => DismissAndLeadCharacter (EXPLAIN2, LocationRegistry.LocationByName(BEARFLOWER_LOCATION)));
			break;
		case EXPLAIN2:
			Transform bearFlowers = LocationRegistry.LocationByName (BEARFLOWER_LOCATION);
			if (Vector3.Distance (actor.transform.position, bearFlowers.position) > 3) {
				Dialog ("Follow me and I will show you the place I know.", "Lead on.", () => DismissAndLeadCharacter (EXPLAIN2, LocationRegistry.LocationByName (BEARFLOWER_LOCATION)));
			} else {
				StopLeadingCharacter ();
				Dialog ("Here you can find lots of flowers but make sure you take only the ones near the ground. We want to come back next year as well, right?", "I understand.", () => AdvanceAndContinue (INTRO3));
				player.AddAndIncreaseSkill (HARVEST_SKILL_ID, 0, 0.1f);
			}
			break;
		case INTRO3:
			Dialog ("Well, I pay you - let's say " + price + " for ten flowers.", "Deal!", () => AdvanceAndDismiss (FETCH_HERBS));
			break;
		case FETCH_HERBS:
			if (player.ItemCount (HERBS_SUBSTANCE_ID) >= 10) {
				Dialog ("Oh, mighty fine flowers!", "You can buy 10.", () => AdvanceAndContinue (SELL_HERBS), "I need them for myself.", Dismiss);
			} else
				Dialog ("You could not find anymore?", "That's it for now.", Dismiss);
			break;
		case SELL_HERBS:
			if (player.ItemCount (HERBS_SUBSTANCE_ID) >= 10) {
				player.GiveItems (HERBS_SUBSTANCE_ID, 10, price);
				Dialog ("I will take 10.", "You can buy more.", () => AdvanceAndContinue(SELL_HERBS), "That's all I have.", () => AdvanceAndDismiss (FETCH_HERBS));
			} else
				Dialog ("You could not find anymore?", "That's it for now.", () => AdvanceAndDismiss (FETCH_HERBS));
			break;
		default:
			Debug.LogWarning ("unknown quest state in " + id + ": " + currentState);
			break;
		}
	}
	
	public override string id {
		get {
			return "herbs1";
		}
	}
	public override string name {
		get {
			return "The basic herbs - Level 1";
		}
	}
	public override string QuestState() {
		switch(currentState) {
		case SELL_HERBS:
		case FETCH_HERBS:
			return "I should collect at least ten bear flowers and sell them to the healer.";
		case EXPLAIN1:
		case EXPLAIN2:
			return "The healer offered to teach me about bear flowers. I should follow him into the forest.";
		default:
			return "Maybe I should talk to the healer and ask him whether he needs help.";
		}
	}
	#endregion
}
