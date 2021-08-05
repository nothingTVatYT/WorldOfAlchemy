using UnityEngine;

public class QuestListItemHandler : MonoBehaviour {

	public QuestLogUI questLogUI;

	public void OnItemClicked() {
		questLogUI.OnItemClicked (transform.GetSiblingIndex ());
	}
}
