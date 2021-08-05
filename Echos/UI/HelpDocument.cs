using System;
using UnityEngine;

public class HelpDocument : MonoBehaviour {

	[Serializable]
	public class HelpPage {
		public string title;
		[TextArea(3,8)]
		public string text;
	}

	[SerializeField] DialogUI dialogUI;
	[SerializeField] HelpPage[] pages;

	void Start () {
		if (dialogUI == null)
			dialogUI = GetComponentInChildren<DialogUI> ();
	}

	void Update() {
		if (Input.GetKeyDown(KeyCode.F1) && !dialogUI.isVisible) {
			ShowHelpDocument ();
		}
	}

	public void ShowPage(int idx) {
		if (idx >= 0 && idx < pages.Length) {
			HelpPage page = pages [idx];
			dialogUI.ShowDialog (page.title, page.text, "Close", Dismiss);
		}
	}

	void Dismiss() {
		dialogUI.CloseDialog ();
	}

	public void ShowHelpDocument() {
		ShowPage (0);
	}
}
