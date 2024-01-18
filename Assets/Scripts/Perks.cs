using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Perks : MonoBehaviour
{
	public Perk perk;

	public void SetPerk(int p) {
		perk = (Perk) p;
		gameObject.SetActive(false);
	}
}
