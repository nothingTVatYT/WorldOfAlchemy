using System;

[Serializable]
public class WOAQuestState
{
	public string id;
	public string state;
	public bool finished;
	public WOAQuestState() {}
	public WOAQuestState(string questId, string newState, bool finished = false) {
		id = questId;
		state = newState;
		this.finished = finished;
	}
}

