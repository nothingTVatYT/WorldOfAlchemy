using UnityEngine;
using UnityEngine.UI;

public class ShowInput : MonoBehaviour {

	[SerializeField] Text text;
	RingBuffer<string> keyPresses = new RingBuffer<string>(5);
	bool bufferChanged;

	void Start() {
		text.text = "";
	}

	void Update () {
		bufferChanged = false;
		string modifier = "";
		if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
			modifier += "shift ";
		if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
			modifier += "ctrl ";
		if (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt))
			modifier += "alt ";
		
		string s = Input.inputString;
		if (!s.Equals ("") || !modifier.Equals(""))
			Add (modifier + s);
		for (int button = 0; button < 3; button++) {
			if (Input.GetMouseButtonDown (button))
				Add ("MB#" + button + " down");
			if (Input.GetMouseButtonUp (button))
				Add ("MB#" + button + " up");
		}
		if (bufferChanged)
			text.text = string.Join ("\n", keyPresses.ToArray ());
	}

	void Add(string s) {
		keyPresses.Add (s);
		bufferChanged = true;
	}
}
