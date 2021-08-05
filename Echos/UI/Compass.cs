using UnityEngine;

public class Compass : MonoBehaviour {

	public float angleOffset = 0;
	public float angleDirectionFactor = -1;
	private Transform playerTransform;
	private RectTransform compassTransform;

	// Use this for initialization
	void Start () {
		playerTransform = GameSystem.gameSystem.player.transform;
		compassTransform = gameObject.GetComponent<RectTransform> ();
	}
	
	// Update is called once per frame
	void Update () {
		float angle = playerTransform.eulerAngles.y;
		compassTransform.transform.localRotation = Quaternion.AngleAxis (angleDirectionFactor * angle + angleOffset, Vector3.forward);
	}
}
