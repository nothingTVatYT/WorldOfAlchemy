using UnityEngine;
using UnityEngine.UI;

public class InfoPanelHandler : MonoBehaviour {

	public Text infoText;
	public Text coordinatesText;
	public GameObject player;

	float lastUpdate;

	// Use this for initialization
	void Start () {
		player = GameSystem.gameSystem.player;
	}
	
	// Update is called once per frame
	void Update () {
		if (Time.time - lastUpdate >= 1f) {
			if (player != null && player.transform != null) {
				coordinatesText.text = player.transform.position.ToString();
				infoText.text = LocationRegistry.LocationByTransform(player.transform);
			}
			lastUpdate = Time.time;
		}
	}
}
