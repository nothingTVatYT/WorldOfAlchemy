using UnityEngine;
using UnityEngine.AI;

public class WaypointFollower : MonoBehaviour {

	Vector3[] waypoints;
	public Transform waypointsList;
	public LineRenderer lineRenderer;
	public float initialDelay;
	private int currentIndex;
	private float pauseTime;
	private NavMeshAgent agent;
	private Animator animator;

	void Start () {
		agent = GetComponent<NavMeshAgent> ();
		animator = GetComponent<Animator> ();
		currentIndex = 0;
		pauseTime = initialDelay;
		int numberWaypoints = waypointsList.childCount;
		waypoints = new Vector3[numberWaypoints];
		for (int i = 0; i < numberWaypoints; i++) {
			waypoints [i] = waypointsList.GetChild (i).position;
		}
		if (lineRenderer != null) {
			lineRenderer.positionCount = waypoints.Length;
			lineRenderer.SetPositions (waypoints);
		}
	}
	
	void Update () {
		if (pauseTime == 0 || Time.time > pauseTime + 10f) {
			if (agent != null) {
				if (agent.isOnNavMesh && !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance) {
					if (animator != null)
						if (Vector3.Distance(waypoints[currentIndex], transform.position) <= agent.stoppingDistance)
							animator.SetBool ("walking", false);
					agent.SetDestination(waypoints [currentIndex]);
					currentIndex++;
					if (currentIndex >= waypoints.Length)
						currentIndex = 0;
					if (animator != null)
						animator.SetBool ("walking", true);
				}
			}
		}
	}
}
