using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

using Mirror;

public class Timer : NetworkBehaviour
{
	public MapLoader loader;
	public Text text;

	public string startingMap = "house";
	public string[] maps;

	[SyncVar(hook="UpdateTimer")] int minutes = 2;
	[SyncVar(hook="UpdateTimer")] int seconds;

	IEnumerator Start() {
		yield return new WaitUntil(() => NetworkClient.localPlayer != null);

		if (NetworkClient.localPlayer.isServer) {
			loader.LoadMap(startingMap);

			while (true) {
				seconds -= 1;

				yield return new WaitForSeconds(1);

				if (seconds <= 0) {
					minutes -= 1;
					seconds = 60;

					if (minutes < 0) {
						loader.LoadMap(maps[Random.Range(0, maps.Length)]);

						minutes = 4;
					}
				}
			}
		}
	}

	public void UpdateTimer(int oldValue, int newValue) {
		text.text = "TIME LEFT TO SURVIVE: " + minutes + ":" + (seconds < 10 ? "0" + seconds : seconds);
	}
}
