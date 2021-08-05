using System;
using UnityEngine;

public class DelegateInfo {

	public static bool IsSubscribed(Delegate theDelegate, object obj) {
		if (theDelegate == null || obj == null)
			return false;
		foreach (Delegate d in theDelegate.GetInvocationList()) {
			if (d.Target == obj)
				return true;
		}
		return false;
	}
}
