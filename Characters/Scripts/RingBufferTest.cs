using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RingBufferTest : MonoBehaviour {

	RingBuffer<int> ringBuffer;
	FloatRingBuffer floatRingBuffer;
	[TextArea(3, 5)]
	public string result;

	// Use this for initialization
	void Start () {
		result = "Checking RingBuffer<int>\n";
		ringBuffer = new RingBuffer<int>(20);
		ringBuffer.Add(2);
		check("(1) Add/Count", 1, ringBuffer.Count);
		ringBuffer.Add(45);
		ringBuffer.Add(3);
		check("(2) Add/Count", 3, ringBuffer.Count);
		int[] d = ringBuffer.ToArray();
		check("(3) ToArray", d.Length, 3);
		check("(4) ToArray", 2, d[0]);
		check("(4) ToArray", 45, d[1]);
		check("(4) ToArray", 3, d[2]);

		result += "Checking FloatRingBuffer\n";
		floatRingBuffer = new FloatRingBuffer(5);
		floatRingBuffer.Add(2.3f);
		check("(11) Add/Count", 1, floatRingBuffer.Count);
		floatRingBuffer.Add(45.7f);
		floatRingBuffer.Add(3f);
		check("(12) Add/Count", 3, floatRingBuffer.Count);
		float[] f = floatRingBuffer.ToArray();
		check("(13) ToArray", f.Length, 3);
		check("(14) ToArray", 2.3f, f[0]);
		check("(14) ToArray", 45.7f, f[1]);
		check("(14) ToArray", 3f, f[2]);
		check("(15) Min", 2.3f, floatRingBuffer.Min());
		check("(16) Max", 45.7f, floatRingBuffer.Max());
		floatRingBuffer.Add(1f);
		check("(17) Min", 1f, floatRingBuffer.Min());
		check("(17) Max", 45.7f, floatRingBuffer.Max());
		floatRingBuffer.Add(100f);
		check("(18) Min", 1f, floatRingBuffer.Min());
		check("(18) Max", 100f, floatRingBuffer.Max());
		floatRingBuffer.Add(10f);
		check("(19) Min", 1f, floatRingBuffer.Min());
		check("(19) Max", 100f, floatRingBuffer.Max());
		floatRingBuffer.Add(12f);
		check("(20) Min", 1f, floatRingBuffer.Min());
		check("(20) Max", 100f, floatRingBuffer.Max());
		floatRingBuffer.Add(11.3f);
		check("(21) Min", 1f, floatRingBuffer.Min());
		check("(21) Max", 100f, floatRingBuffer.Max());
		floatRingBuffer.Add(34.5f);
		check("(22) Min", 10f, floatRingBuffer.Min());
		check("(22) Max", 100f, floatRingBuffer.Max());
	}

	bool check(string test, int expectedValue, int currentValue) {
		if (currentValue != expectedValue) {
			result += test + " failed, got " + currentValue + " instead of " + expectedValue;
			return false;
		}
		return true;
	}

	bool check(string test, float expectedValue, float currentValue) {
		if (currentValue != expectedValue) {
			result += test + " failed, got " + currentValue + " instead of " + expectedValue;
			return false;
		}
		return true;
	}

	bool check(string test, string expectedValue, string currentValue) {
		if (currentValue != expectedValue) {
			result += test + " failed, got " + currentValue + " instead of " + expectedValue;
			return false;
		}
		return true;
	}

	bool check(string test, bool success) {
		if (!success) {
			result += test + " failed";
		}
		return success;
	}

	// Update is called once per frame
	void Update () {
		
	}
}
