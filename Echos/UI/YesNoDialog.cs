using System;
using UnityEngine;
using UnityEngine.UI;

public class YesNoDialog : MonoBehaviour {

	[SerializeField] GameObject dialogPanel;
	[SerializeField] Text dialogText;
	Action clickedYes;
	Action clickedNo;

	public void ShowDialog(string text, Action clickedYes, Action clickedNo) {
		dialogPanel.SetActive (true);
		dialogText.text = text;
		this.clickedYes = clickedYes;
		this.clickedNo = clickedNo;
	}

	void CloseDialog() {
		dialogPanel.SetActive (false);
	}

	void Start () {
		dialogPanel.SetActive (false);
	}
	
	public void OnClickYes() {
		clickedYes ();
		CloseDialog ();
	}

	public void OnClickNo() {
		clickedNo ();
		CloseDialog();
	}
}
