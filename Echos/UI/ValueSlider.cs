using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

public class ValueSlider : MonoBehaviour {

	public Rigidbody attachedObject;
	public string property;
	public Text titleLabel;
	public Text valueLabel;
	public Slider slider;
	private PropertyInfo propertyInfo;

	// Use this for initialization
	void Start () {
		propertyInfo = attachedObject.GetType ().GetProperty (property);
		if (propertyInfo != null)
			slider.value = (float)propertyInfo.GetValue (attachedObject, null);
		else
			Debug.LogError ("property " + property + " not found on " + attachedObject);
		UpdateValueText ();
		titleLabel.text = property + " of " + attachedObject;
	}
	
	// Update is called once per frame
	void Update () {
	}

	public void UpdateValueText() {
		valueLabel.text = string.Format ("{0:F}", slider.value);
	}

	public void OnSliderChanged() {
		UpdateValueText ();
		if (propertyInfo != null)
			propertyInfo.SetValue (attachedObject, slider.value, null);
	}
}
