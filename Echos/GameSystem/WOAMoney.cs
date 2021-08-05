using System;
using UnityEngine;

[Serializable]
public class WOAMoney
{
	public int gold; // { get; private set; }
	public int silver; // { get; private set; }
	public int copper; // { get; private set; }

	public WOAMoney() {}

	public WOAMoney(int gold, int silver, int copper) {
		this.gold = gold;
		this.silver = silver;
		this.copper = copper;
	}

	public WOAMoney Set(int gold, int silver, int copper) {
		this.gold = gold;
		this.silver = silver;
		this.copper = copper;
		return Normalize();
	}

	public WOAMoney Set(WOAMoney other) {
		gold = other.gold;
		silver = other.silver;
		copper = other.copper;
		return Normalize();
	}

	public WOAMoney Subtract(WOAMoney other) {
		gold -= other.gold;
		silver -= other.silver;
		copper -= other.copper;
		return Normalize ();
	}

	public WOAMoney Substract(int gold, int silver, int copper) {
		this.gold -= gold;
		this.silver -= silver;
		this.copper -= copper;
		return Normalize ();
	}

	public WOAMoney Add(WOAMoney other) {
		gold += other.gold;
		silver += other.silver;
		copper += other.copper;
		return Normalize ();
	}

	public WOAMoney Add(int gold, int silver, int copper) {
		this.gold += gold;
		this.silver += silver;
		this.copper += copper;
		return Normalize ();
	}

	public bool IsGreaterOrEqual(WOAMoney other) {
		return Compare(other) >= 0;
	}

	public int Compare(WOAMoney other) {
		if (gold > other.gold)
			return 1;
		if (gold < other.gold)
			return -1;
		if (silver > other.silver)
			return 1;
		if (silver < other.silver)
			return -1;
		if (copper > other.copper)
			return 1;
		if (copper < other.copper)
			return -1;
		return 0;
	}

	public override string ToString ()
	{
		string s = "";
		if (gold != 0)
			s += gold + " gold";
		if (silver != 0) {
			if (s != "")
				s += " ";
			s += silver + " silver";
		}
		if (copper != 0 || s == "") {
			if (s != "")
				s += " ";
			s += copper + " copper";
		}
		return s;
	}

	WOAMoney Normalize() {
		while (copper < 0) {
			copper += 100;
			silver--;
		}
		while (copper >= 100) {
			copper -= 100;
			silver++;
		}
		while (silver < 0) {
			silver += 100;
			gold--;
		}
		while (silver >= 100) {
			silver -= 100;
			gold++;
		}
		return this;
	}
}

