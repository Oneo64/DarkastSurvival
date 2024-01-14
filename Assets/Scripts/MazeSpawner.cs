using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Mirror;

public class MazeSpawner : NetworkBehaviour
{
	public Object[] enemies;
	public int maxEnemies = 50;
	public int spawnRange = 20;
	[Min(1)] public int spawnInterval = 10;

	IEnumerator Start() {
		if (GetComponent<Maze>() || GetComponent<DynamicMaze>()) {
			yield return new WaitUntil(() => isServer && ((GetComponent<Maze>() && GetComponent<Maze>().spawnedTiles.Count > 0) || (GetComponent<DynamicMaze>() && GetComponent<DynamicMaze>().spawnedTiles.Count > 0)));
		}
		
		Transform[] tiles = {};

		if (GetComponent<Maze>()) tiles = GetComponent<Maze>().spawnedTiles.ToArray();
		if (GetComponent<DynamicMaze>()) tiles = GetComponent<DynamicMaze>().spawnedTiles.ToArray();

		if (tiles.Length == 0) {
			List<Transform> l = new List<Transform>() {};

			foreach (GameObject g in GameObject.FindGameObjectsWithTag("Spawner")) {
				l.Add(g.transform);
			}

			tiles = l.ToArray();
		}

		while (true) {
			if (GetEnemyCount() < maxEnemies) {
				Transform tile = tiles[Random.Range(0, tiles.Length)];

				bool canSpawn = true;

				foreach (PlayerCore player in Object.FindObjectsOfType<PlayerCore>()) {
					if (Vector3.Distance(player.transform.position, tile.position) <= spawnRange) canSpawn = false;
				}

				if (canSpawn) NetworkServer.Spawn(Instantiate(enemies[Random.Range(0, enemies.Length)] as GameObject, tile.position, Quaternion.identity));
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
