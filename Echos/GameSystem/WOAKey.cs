using System;

[Serializable]
public class WOAKey : WOAItem
{
	public string keyCode;

	public WOAKey(WOAKey other) :base(other) {
		keyCode = other.keyCode;
	}

	public bool canUnlock(string lockCode) {
		if (lockCode.Equals (""))
			return true;
		if (lockCode.Equals (keyCode))
			return true;
		if (keyCode.Length < lockCode.Length && lockCode.StartsWith (keyCode))
			return true;
		return false;
	}
}

