using UnityEngine;

public class MenuHandler : MonoBehaviour {

	public GameObject canvas;

	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown(KeyCode.U) && Input.GetKey(KeyCode.LeftControl)) {
			canvas.SetActive (!canvas.activeSelf);
		}
	}
}
