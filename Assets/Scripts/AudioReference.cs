using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioReference : MonoBehaviour
{
	public AudioSource source;
	public bool canPlay;

	void PlaySound() {
		if (canPlay) source.PlayOneShot(source.clip);
	}
}
