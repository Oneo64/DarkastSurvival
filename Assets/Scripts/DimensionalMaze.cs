using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Mirror;

public class DimensionalMaze : NetworkBehaviour
{
	public int size = 10;
	public int maxSize = 100;
	public Object[] tiles;
	public Object[] rareTiles;
	public List<Transform> spawnedTiles;

	void Start() {
		if (isServer) {
			spawnedTiles = new List<Transform>();

			for (int i = 0; i < size; i++) {
				GameObject[] connectors = GameObject.FindGameObjectsWithTag("Connector");

				for (int k = 0; k < connectors.Length; k++) {
					Transform connector = connectors[k].transform;
					GameObject tileToSpawn = null;

					if (Random.Range(1, 21) == 1 && rareTiles.Length > 0) {
						tileToSpawn = rareTiles[Random.Range(0, rareTiles.Length)] as GameObject;
					} else {
						tileToSpawn = tiles[Random.Range(0, tiles.Length)] as GameObject;
					}

					Bounds bounds = tileToSpawn.GetComponent<MeshFilter>().sharedMesh.bounds;

					Collider[] overlapping = Physics.OverlapBox(
						connector.position + (connector.forward * bounds.size.z * 0.5f) + (Vector3.up * bounds.size.y * 0.5f),
						(bounds.size / 2f) - (Vector3.one * 0.1f),
						connector.rotation,
						LayerMask.GetMask("Default")
					);

					Vector3 pos = connector.position + (connector.forward * bounds.size.z * 0.5f);

					if (overlapping.Length == 0 && pos.x >= -maxSize && pos.x <= maxSize && pos.z >= -maxSize && pos.z <= maxSize) {
						GameObject spawnedTile = Instantiate(tileToSpawn, pos, connector.rotation, transform.parent);

						spawnedTiles.Add(spawnedTile.transform);
						NetworkServer.Spawn(spawnedTile);
					}

					Destroy(connector.gameObject);
				}
			}
		}
	}
}
