using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Mirror;

public class LootSpawner : NetworkBehaviour
{
	public string[] commonLoot;
	public string[] rareLoot;
	public Transform target;

	public bool generateLoot = true;

	void Start() {
		if (isServer) {
			if (target == null) target = transform;

			if (generateLoot) {
				foreach (Transform t in target) {
					if (t.name.ToLower().Contains("shelf")) {
						if (Random.Range(1, 11) == 1) {
							Vector3 pos = t.position + (Vector3.up * Random.Range(1, 4) * 0.5f);
							GameObject item = Instantiate(Resources.Load("PlacedItem") as GameObject, pos, Quaternion.identity);

							NetworkServer.Spawn(item);

							SetLootUp(item.GetComponent<PlacedItem>());
						}
					}
				}
			} else {
				foreach (Transform t in target) {
					if (t.name == "PlacedItem") {
						GameObject item = Instantiate(Resources.Load("PlacedItem") as GameObject, t.position, Quaternion.identity);

						NetworkServer.Spawn(item);

						SetLootUp(item.GetComponent<PlacedItem>());

						Destroy(t.gameObject, 0.1f);
					}
				}
			}
		}
	}

	private void SetLootUp(PlacedItem item) {
		string loot = commonLoot[Random.Range(0, commonLoot.Length)];

		if (Random.Range(1, 4) == 1 && rareLoot.Length > 0) loot = rareLoot[Random.Range(0, rareLoot.Length)];

		item.item = loot;
	}
}
