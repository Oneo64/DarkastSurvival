using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Mirror;

public class Ambience : MonoBehaviour
{
	public AudioClip[] sounds;
	public AudioSource sound;
	[Min(1)] public float interval = 5;

	IEnumerator Start() {
		while (true) {
			sound.PlayOneShot(sounds[Random.Range(0, sounds.Length)]);

			yield return new WaitForSeconds(Random.Range(interval, interval * 2));
		}
	}
}
