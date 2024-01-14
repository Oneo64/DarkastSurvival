using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightFlicker : MonoBehaviour
{
	float wait;

	void Update() {
		if (wait < Time.time) {
			GetComponent<Light>().enabled = !GetComponent<Light>().enabled;
			wait = Time.time + (Random.Range(1, 5) == 1 ? Random.Range(3f, 4f) : Random.Range(0.05f, 0.1f));
		}
	}
}
