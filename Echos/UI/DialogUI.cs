using System;
using UnityEngine;
using UnityEngine.UI;

public class DialogUI : MonoBehaviour {

	[SerializeField] Text speakerName;
	[SerializeField] Text speakerText;
	[SerializeField] Text replyText1;
	[SerializeField] Text replyText2;
	[SerializeField] Text replyText3;
	[SerializeField] float dialogTimeout = 30f;
	Action callback1;
	Action callback2;
	Action callback3;
	const string PREFS_NAME = "DialogUI";
	float dialogOpened;

	public bool isVisible { get { return gameObject.activeSelf; }}

	void Start() {
		PanelPrefs.RestoreTransformPosition (PREFS_NAME, transform);
		gameObject.SetActive (false);
	}

	void Update() {
		if (dialogTimeout > 0 && Time.time - dialogOpened > dialogTimeout)
			CloseDialog ();
	}

	public void ShowDialog(string speaker, string text, string reply1, Action callback1, string reply2 = null, Action callback2 = null, string reply3 = null, Action callback3 = null) {
		if (!gameObject.activeSelf)
			gameObject.SetActive (true);
		dialogOpened = Time.time;
		this.callback1 = callback1;
		this.callback2 = callback2;
		this.callback3 = callback3;
		speakerName.text = speaker;
		speakerText.text = text;
		replyText1.text = reply1;
		if (reply2 != null && !reply2.Equals ("")) {
			replyText2.text = reply2;
			replyText2.transform.parent.gameObject.SetActive (true);
		} else {
			replyText2.text = "";
			replyText2.transform.parent.gameObject.SetActive (false);
		}
		if (reply3 != null && !reply3.Equals ("")) {
			replyText3.text = reply3;
			replyText3.transform.parent.gameObject.SetActive (true);
		} else {
			replyText3.text = "";
			replyText3.transform.parent.gameObject.SetActive (false);
		}
	}

	public void CloseDialog() {
		PanelPrefs.SaveTransformPosition (PREFS_NAME, transform);
		gameObject.SetActive (false);
	}

	public void ClickedOption1() {
		callback1 ();
	}
	public void ClickedOption2() {
		callback2 ();
	}
	public void ClickedOption3() {
		callback3 ();
	}
}
