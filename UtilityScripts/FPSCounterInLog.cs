using UnityEngine;

public class FPSCounterInLog : MonoBehaviour
{
	public float fpsMeasurePeriod = 3f;
	private int m_FpsAccumulator = 0;
	private float m_FpsNextPeriod = 0;
	private int m_CurrentFps;
	const string display = "{0} FPS";

	private void Start ()
	{
		m_FpsNextPeriod = Time.realtimeSinceStartup + fpsMeasurePeriod;
	}

	private void Update ()
	{
		// measure average frames per second
		m_FpsAccumulator++;
		if (Time.realtimeSinceStartup > m_FpsNextPeriod) {
			m_CurrentFps = (int)(m_FpsAccumulator / fpsMeasurePeriod);
			m_FpsAccumulator = 0;
			m_FpsNextPeriod += fpsMeasurePeriod;
			Debug.Log (string.Format (display, m_CurrentFps));
		}
	}
}
