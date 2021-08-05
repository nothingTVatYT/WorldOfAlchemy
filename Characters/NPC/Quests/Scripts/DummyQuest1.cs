using System;
using UnityEngine;

public class DummyQuest1 : WOAQuest
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
		return "This is just some dummy quest and there is no real description. But here is some stupid text to check the UI is working.";
	}
	public override string name {
		get {
			return "Dummy quest number one";
		}
	}

    public override string id
    {
        get
        {
            return "dummy1";
        }
    }
    #endregion
}

