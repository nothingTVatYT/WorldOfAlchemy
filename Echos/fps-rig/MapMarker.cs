using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapMarker : MonoBehaviour {

	private GameObject player;
	[SerializeField] private Vector3 world1;
	[SerializeField] private Vector3 world2;
	[SerializeField] private Vector3 map1;
	[SerializeField] private Vector3 map2;
	private float scaleX, scaleY;

	private Vector3 mapped(Vector3 worldLocation) {
		return new Vector3 ((worldLocation.x - world1.x) * scaleX + map1.x, (worldLocation.z - world1.z) * scaleY + map1.y, 0);
	}

	void Start () {
		scaleX = (map2.x - map1.x) / (world2.x - world1.x);
		scaleY = (map2.y - map1.y) / (world2.z - world1.z);
		player = GameSystem.gameSystem.player;
	}
	
	void Update () {
		transform.localPosition = mapped (player.transform.position);
	}
}
