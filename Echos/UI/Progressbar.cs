using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Progressbar : MonoBehaviour
{
	public float value { get { return filledArea.fillAmount; } set { filledArea.fillAmount = value; } }

	[SerializeField] private Image filledArea;
	[SerializeField] private Text detailedValues;

	public void setDetailedValues(int value, int maxValue) {
		if (detailedValues != null) {
			detailedValues.text = string.Format ("{0}/{1}", value, maxValue);
		}
	}
}
