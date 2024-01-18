using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Mirror;

public class DroppedItem : NetworkBehaviour
{
	[SyncVar(hook="UpdateModel")] public Item item;

	void Start() {
		if (!isServer) GetComponent<Rigidbody>().isKinematic = true;
	}

	[Command(requiresAuthority = false)]
	public void CmdDestroy() {
		print("kill");
		Destroy(gameObject);
	}

	private void UpdateModel(Item oldValue, Item newValue) {
		if (transform.childCount > 0) Destroy(transform.GetChild(0).gameObject);

		if (newValue != null) {
			string modelName = "";

			if (Database.items[newValue.id] is Gun) {
				modelName = ((Gun) Database.items[newValue.id]).model[0];
			} else if (Database.items[newValue.id] is Tool) {
				modelName = ((Tool) Database.items[newValue.id]).model;
			} else if (Database.items[newValue.id] is Food) {
				modelName = ((Food) Database.items[newValue.id]).model;
			} else if (Database.items[newValue.id] is Throwable) {
				modelName = ((Throwable) Database.items[newValue.id]).model;
			}

			if (Resources.Load("ItemModels/" + modelName)) {
				GameObject model = Instantiate(Resources.Load("ItemModels/" + modelName) as GameObject, transform);
				Mesh m = model.GetComponent<MeshFilter>().sharedMesh;

				model.transform.localScale = Vector3.one;
				model.transform.localPosition = Vector3.zero;
				model.transform.localEulerAngles = Vector3.zero;

				BoxCollider collider = gameObject.AddComponent<BoxCollider>();

				collider.center = m.bounds.center;
				collider.size = m.bounds.size;
			}
		}
	}
}
