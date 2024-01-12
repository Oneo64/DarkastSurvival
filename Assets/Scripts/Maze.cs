using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Mirror;

public class Maze : NetworkBehaviour
{
	public int tileSize = 10;
	public int size = 10;
	public int maxSize = 100;
	public Object[] tiles;
	public Object[] rareTiles;
	public List<Transform> spawnedTiles;
	public bool randomRot;

	void Start() {
		if (isServer) {
			spawnedTiles = new List<Transform>();

			for (int i = 0; i < size; i++) {
				GameObject[] connectors = GameObject.FindGameObjectsWithTag("Connector");

				for (int k = 0; k < connectors.Length; k++) {
					Transform connector = connectors[k].transform;

					Collider[] overlapping = Physics.OverlapBox(
						connector.position + (connector.forward * (tileSize / 2f)) + (Vector3.up * (randomRot ? 0 : 2)),
						new Vector3((tileSize / 2f) - 0.2f, (randomRot ? (tileSize / 2f) - 0.2f : 1.5f), (tileSize / 2f) - 0.2f),
						connector.rotation,
						LayerMask.GetMask("Default")
					);

					Vector3 pos = connector.position + (connector.forward * (tileSize / 2f));

					if (overlapping.Length == 0 && pos.x >= -maxSize && pos.x <= maxSize && pos.z >= -maxSize && pos.z <= maxSize) {
						GameObject spawnedTile = null;

						Vector3 rot = connector.eulerAngles;

						if (randomRot) {
							//rot.x = Random.Range(0, 4) * 90;
							//rot.y = Random.Range(0, 4) * 90;
							rot.z = Random.Range(0, 4) * 90;
						}

						if (Random.Range(1, 21) == 1 && rareTiles.Length > 0) {
							spawnedTile = Instantiate(rareTiles[Random.Range(0, rareTiles.Length)] as GameObject, pos, Quaternion.Euler(rot), transform.parent);
						} else {
							spawnedTile = Instantiate(tiles[Random.Range(0, tiles.Length)] as GameObject, pos, Quaternion.Euler(rot), transform.parent);
						}

						spawnedTiles.Add(spawnedTile.transform);
						NetworkServer.Spawn(spawnedTile);
					}

					Destroy(connector.gameObject);
				}
			}
		}
	}
}
