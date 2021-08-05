using System;
using UnityEngine;

public interface ISpellEffect
{
	void setSpell (WOASpell spell);
	bool attach (GameObject target);
}

