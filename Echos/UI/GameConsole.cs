using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class GameConsole : MonoBehaviour {

	[SerializeField] Text console;
	[SerializeField] ScrollRect scroller;
	[SerializeField] int maxLines;
	public enum InfoLevel
	{
		FineDetail, Detail, General, Warning, SevereWarning
	}
	[SerializeField] InfoLevel minLevel = InfoLevel.Detail;
	List<string> lines = new List<string>();
	const string EOL = "\n";
	StringBuilder builder = new StringBuilder();
	public static GameConsole gameConsole { get; private set; }

	void Awake() {
		if (gameConsole != null)
			return;
		gameConsole = this;
	}

	public void Start() {
		updateTextWidget ();
	}

	public void println(InfoLevel level, string text) {
		if (level >= minLevel) {
			string line = text;
			if (!text.EndsWith (EOL))
				line = text + EOL;
			lines.Add (line);
			while (lines.Count > maxLines) {
				lines.RemoveAt (0);
			}
			updateTextWidget ();
		}
	}

	void updateTextWidget() {
		builder.Remove(0, builder.Length);
		foreach (string s in lines)
			builder.Append (s);
		console.text = builder.ToString ();
		if (scroller != null)
			scroller.verticalNormalizedPosition = 0;
	}
}
