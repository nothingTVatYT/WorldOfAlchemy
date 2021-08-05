public class QTeleportBridge : WOAQuest
{
	const string learnedSpell = "teleportBridge";
	const string INTRO1 = "intro1";
	const string LEARN = "learn";
	const string LEARNED = "learned";

	#region implemented abstract members of WOAQuest
	protected override bool CanStartOrContinue (BasePlayer player, WOAQuestState state)
	{
		return player.Spell(learnedSpell) == null;
	}

	public override void Run (BaseCharacter actor)
	{
		switch(currentState) {
		case NONE:
			Dialog ("Hello stranger!", "Can you teach me something?", () => AdvanceAndContinue (INTRO1), "Have a nice day.", Dismiss);
			break;
		case INTRO1:
			Dialog ("I can teach you to teleport to the valley.", "I would really like that.", () => AdvanceAndContinue (LEARN), "Maybe later.", Dismiss);
			break;
		case LEARN:
			player.addSpell (SpellRegistry.registry.CreateSpell (learnedSpell));
			Dialog ("Read and memorize this and you will be able to teleport whenever you want.", "Thank you.", () => AdvanceAndDismiss (LEARNED));
			break;
		}
	}

	public override string QuestState ()
	{
		throw new System.NotImplementedException ();
	}
	public override string id {
		get {
			return "teleportBridge";
		}
	}
	public override string name {
		get {
			return "Teleportation to bridge";
		}
	}
	#endregion
    
}