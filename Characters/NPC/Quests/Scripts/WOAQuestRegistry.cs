using System;
using System.Collections.Generic;
using UnityEngine;

public class WOAQuestRegistry
{
	static bool initialized = false;
	static List<string> allNames = new List<string> ();
	static List<WOAQuest> allQuests = new List<WOAQuest> ();
	static Dictionary<string, WOAQuest> questsById = new Dictionary<string, WOAQuest> ();

	static void Initialize ()
	{
		Add (new QHealer1 ());
		Add (new DummyQuest1 ());
		Add (new DummyQuest2 ());
		Add (new QTeleportBridge ());
		initialized = true;
	}

	static void Add (WOAQuest quest)
	{
		if (!questsById.ContainsKey (quest.id)) {
			allQuests.Add (quest);
			allNames.Add (quest.name + " (" + quest.id + ")");
			questsById.Add (quest.id, quest);
		} else
			Debug.LogError (String.Format ("The id for quest {0} ({1}) is already registered as {2}", quest.id, quest.name, questsById [quest.id].name));
	}

	static WOAQuest Instantiate(WOAQuest template) {
		return Activator.CreateInstance(template.GetType ()) as WOAQuest;
	}

	public static WOAQuest Quest(int idx) {
		if (!initialized)
			Initialize ();
		return allQuests [idx];
	}

	public static WOAQuest Quest(string id) {
		if (!initialized)
			Initialize ();
		if (questsById.ContainsKey(id))
			return questsById [id];
		return null;
	}

	public static WOAQuest QuestInstance(string id) {
		if (!initialized)
			Initialize ();
		if (questsById.ContainsKey(id))
			return Instantiate(questsById [id]);
		return null;
	}

	public static int IndexOf(string id) {
		if (!initialized)
			Initialize ();
		WOAQuest quest = Quest(id);
		if (quest != null)
			return allQuests.IndexOf (quest);
		return -1;
	}

	public static WOAQuest[] Quests {
		get {
			if (!initialized)
				Initialize ();
			return allQuests.ToArray();
		}
	}

	public static string[] QuestNames {
		get {
			if (!initialized)
				Initialize ();
			return allNames.ToArray();
		}
	}
}

