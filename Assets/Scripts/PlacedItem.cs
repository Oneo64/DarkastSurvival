using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Mirror;

public class PlacedItem : NetworkBehaviour
{
	[SyncVar(hook="UpdateModel")] public string item;

	[Command(requiresAuthority = false)]
	public void CmdDestroy() {
		Destroy(gameObject);
	}

	private void UpdateModel(string oldValue, string newValue) {
		if (transform.childCount > 0) Destroy(transform.GetChild(0).gameObject);

		if (newValue != "") {
			string modelName = "";

			if (Database.items[newValue] is Gun) {
				modelName = ((Gun) Database.items[newValue]).model[0];
			} else if (Database.items[newValue] is Tool) {
				modelName = ((Tool) Database.items[newValue]).model;
			} else if (Database.items[newValue] is Food) {
				modelName = ((Food) Database.items[newValue]).model;
			} else if (Database.items[newValue] is Throwable) {
				modelName = ((Throwable) Database.items[newValue]).model;
			}

			if (Resources.Load("ItemModels/" + modelName)) {
				GameObject model = Instantiate(Resources.Load("ItemModels/" + modelName) as GameObject, transform);
				Mesh m = model.GetComponent<MeshFilter>().sharedMesh;

				model.transform.localScale = Vector3.one;
				model.transform.localPosition = Vector3.up * m.bounds.size.x;
				model.transform.localEulerAngles = Vector3.forward * 90;

				MeshCollider collider = gameObject.AddComponent<MeshCollider>();

				collider.sharedMesh = m;
				collider.convex = true;
				collider.isTrigger = true;
			}
		}
	}
}
