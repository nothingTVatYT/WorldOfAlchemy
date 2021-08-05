using System;

public class DummyQuest2 : WOAQuest
{
	#region implemented abstract members of WOAQuest
	protected override bool CanStartOrContinue (BasePlayer player, WOAQuestState state)
	{
		return true;
	}
	public override void Run (BaseCharacter actor)
	{
		throw new NotImplementedException ();
	}
	public override string QuestState ()
	{
		return "This is the second dummy quest without a real quest state. Just imagine there is own and it is here to read.";
	}
	public override string id {
		get {
			return "dummy2";
		}
	}
	public override string name {
		get {
			return "Dummy quest number two";
		}
	}
	#endregion
}

