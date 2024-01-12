using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Mirror;

public class Spawner : NetworkBehaviour
{
	public Object[] enemies;
	public Transform target;
	public int maxEnemies = 50;
	[Min(1)] public int spawnInterval = 10;

	IEnumerator Start() {
		yield return new WaitUntil(() => isServer);

		while (true) {
			if (GetEnemyCount() < maxEnemies) {
				NetworkServer.Spawn(Instantiate(enemies[Random.Range(0, enemies.Length)] as GameObject, target.position, Quaternion.identity));
			}

			yield return new WaitForSeconds(spawnInterval);
		}
	}

	private int GetEnemyCount() {
		int count = 0;

		foreach (Enemy e in Object.FindObjectsOfType<Enemy>()) {
			if (!e.follow) count++;
		}

		return count;
	}
}
