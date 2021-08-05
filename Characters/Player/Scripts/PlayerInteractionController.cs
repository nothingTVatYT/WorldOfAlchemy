using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(BasePlayer))]
public class PlayerInteractionController : MonoBehaviour {

	BasePlayer basePlayer;
	[SerializeField] float clickTargetRadius = 100;
	[SerializeField] float clickTalkRadius = 5;
	Vector3 targetDirection;
	AnimatedHeadLook animatedHeadLook;

	// Use this for initialization
	void Start () {
		basePlayer = GetComponent<BasePlayer> ();
		animatedHeadLook = GetComponentInChildren<AnimatedHeadLook> ();
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetButtonUp ("Fire1") && (EventSystem.current == null || !EventSystem.current.IsPointerOverGameObject())) {
			Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
			RaycastHit hit;
			if (Physics.Raycast (ray, out hit, clickTargetRadius, Physics.AllLayers, QueryTriggerInteraction.Collide)) {
				GameObject target = hit.collider.gameObject;
				BaseCharacter npc = target.GetComponent<BaseCharacter> ();
				IMotivationController npcm = target.GetComponent<IMotivationController> ();
				if (npc != null && target != GameSystem.gameSystem.player) {
					if (basePlayer.target == target) {
						if (Vector3.Distance(hit.point, transform.position) <= clickTalkRadius && npcm != null)
							npcm.injectMotivation (new WOAMotivation (WOAMotivation.ActionType.TalkTo));
					} else {
						basePlayer.target = target;
					}
				} else {
					basePlayer.target = null;
				}
			} else {
				basePlayer.target = null;
			}
		}

		if (basePlayer.target != null) {
			targetDirection = basePlayer.target.transform.position - transform.position;
			if (targetDirection.sqrMagnitude < 1000) {
				Vector3 targetDirectionNorm = targetDirection;
				targetDirectionNorm.Normalize ();
				float dot = Vector3.Dot (targetDirectionNorm, transform.forward);
				if (dot > 0)
					animatedHeadLook.setTargetVector (basePlayer.target.transform.position + AnimatedHeadLook.NPC_LOOK_OFFSET, dot);
				else
					animatedHeadLook.clearTargetVector ();
			}
		}

	}
}
