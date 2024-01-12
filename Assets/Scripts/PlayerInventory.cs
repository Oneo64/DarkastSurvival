using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

using Mirror;

public class PlayerInventory : NetworkBehaviour
{
	public Item[] inventory = new Item[16] {
		null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null
	};

	public Transform rigs;
	public Transform tool;
	public int selected = 1;

	public Item selectedItem {
		private set {}

		get {
			return inventory[selected];
		}
	}

	float scroll;
	float scrollReset;

	PlayerCore core;

	void Start() {
		core = GetComponent<PlayerCore>();

		if (isLocalPlayer) UpdateInventory();

		AddItem("flashlight");
		AddItem("canned_tuna", 3);
	}

	void Update() {
		if (isLocalPlayer) {
			if (Input.GetAxis("Mouse ScrollWheel") > 0) {
				selected = Mathf.Clamp(selected - 1, 0, 15);
				UpdateInventory();
				CheckAnimations();
			}

			if (Input.GetAxis("Mouse ScrollWheel") < 0) {
				selected = Mathf.Clamp(selected + 1, 0, 15);
				UpdateInventory();
				CheckAnimations();
			}
		}
	}

	public void UpdateInventory() {
		string inventoryText = "";

		for (int i = 0; i < inventory.Length; i++) {
			if (selected == i) inventoryText += ">";

			if (inventory[i] == null) {
				inventoryText += "<i>Empty</i>";
			} else {
				inventoryText += inventory[i].GetName() + ((inventory[i].amount > 1) ? (" x" + inventory[i].amount) : "");
			}

			if (i < inventory.Length - 1) inventoryText += "\n";
		}

		GameObject.Find("/Canvas/Inventory").GetComponent<Text>().text = inventoryText;
	}

	[Command]
	public void CmdUpdateToolModel(string modelName) {
		RpcUpdateToolModel(modelName);
	}

	[ClientRpc]
	public void RpcUpdateToolModel(string modelName) {
		if (!isLocalPlayer) UpdateToolModel(modelName);
	}

	public void UpdateToolModel(string modelName) {
		for (int i = 0; i < tool.childCount; i++) {
			Destroy(tool.GetChild(i).gameObject);
		}

		if (modelName != "") {
			GameObject model = Instantiate(Resources.Load("ItemModels/" + modelName) as GameObject, tool);

			model.transform.localScale = Vector3.one;
			model.transform.localPosition = Vector3.zero;
			model.transform.localEulerAngles = Vector3.zero;
		}
	}

	public void CheckAnimations() {
		if (inventory[selected] != null && inventory[selected].GetData() is Gun) {
			Gun gun = (Gun) inventory[selected].GetData();

			core.animator.CrossFade(gun.model[1], 0.2f, 1);
			
			CmdUpdateToolModel(gun.model[0]);
			UpdateToolModel(gun.model[0]);
		} else if (inventory[selected] != null && inventory[selected].GetData() is Tool) {
			Tool t = (Tool) inventory[selected].GetData();

			core.animator.CrossFade("Hold", 0.2f, 1);
			
			CmdUpdateToolModel(t.model);
			UpdateToolModel(t.model);
		} else if (inventory[selected] != null && inventory[selected].GetData() is Food) {
			Food t = (Food) inventory[selected].GetData();

			core.animator.CrossFade("Hold", 0.2f, 1);
			
			CmdUpdateToolModel(t.model);
			UpdateToolModel(t.model);
		} else {
			core.animator.CrossFade("Empty", 0.2f, 1);
			
			CmdUpdateToolModel("");
			UpdateToolModel("");
		}
	}

	public void AddItem(string id, int amount = 1) {
		if (Database.items[id].canStack) {
			for (int i = 0; i < inventory.Length; i++) {
				if (inventory[i] != null && inventory[i].id == id) {
					inventory[i].amount += amount;

					UpdateInventory();

					return;
				}
			}
		}

		Item item = new Item(id, amount);

		for (int i = 0; i < inventory.Length; i++) {
			if (inventory[i] == null) {
				inventory[i] = item;

				UpdateInventory();

				return;
			}
		}
	}

	public void AddItem(Item item2) {
		Item item = new Item(item2.id, item2.amount);

		item.externalData = item2.externalData;

		if (Database.items[item.id].canStack) {
			for (int i = 0; i < inventory.Length; i++) {
				if (inventory[i] != null && inventory[i].id == item.id) {
					inventory[i].amount += item.amount;

					UpdateInventory();

					return;
				}
			}
		}

		for (int i = 0; i < inventory.Length; i++) {
			if (inventory[i] == null) {
				inventory[i] = item;

				UpdateInventory();

				return;
			}
		}
	}

	public bool HasSpaceFor(string n) {
		for (int i = 0; i < inventory.Length; i++) {
			if (inventory[i] == null || (inventory[i] != null && inventory[i].id == n)) return true;
		}

		return false;
	}

	public bool HasItem(string n, int amount = 1) {
		for (int i = 0; i < inventory.Length; i++) {
			if (inventory[i] != null && inventory[i].id == n && amount <= inventory[i].amount) return true;
		}

		return false;
	}

	public void RemoveItem(string n, int amount = 1) {
		for (int i = 0; i < inventory.Length; i++) {
			if (inventory[i] != null && inventory[i].id == n) {
				inventory[i].amount -= amount;				

				if (inventory[i].amount <= 0) inventory[i] = null;

				UpdateInventory();

				return;
			}
		}
	}
}
