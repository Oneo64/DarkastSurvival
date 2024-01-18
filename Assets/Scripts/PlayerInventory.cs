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
	public int craftingSelected = 0;
	public string recipe;

	public Item selectedItem {
		private set {}

		get {
			return inventory[selected];
		}
	}

	float scroll;
	float scrollReset;

	bool crafting;

	PlayerCore core;

	void Start() {
		core = GetComponent<PlayerCore>();

		if (isLocalPlayer) UpdateInventory();
	}

	void Update() {
		if (isLocalPlayer) {
			if (Input.GetAxis("Mouse ScrollWheel") > 0) {
				if (crafting) {
					craftingSelected = Mathf.Clamp(craftingSelected - 1, 0, Database.crafting.Count - 1);
				} else {
					selected = Mathf.Clamp(selected - 1, 0, 15);
				}

				UpdateInventory();
				CheckAnimations();
			}

			if (Input.GetAxis("Mouse ScrollWheel") < 0) {
				if (crafting) {
					craftingSelected = Mathf.Clamp(craftingSelected + 1, 0, Database.crafting.Count - 1);
				} else {
					selected = Mathf.Clamp(selected + 1, 0, 15);
				}

				UpdateInventory();
				CheckAnimations();
			}

			if (crafting && Input.GetKeyDown(KeyCode.Mouse0)) {
				bool has = true;
				bool garanteedSpace = false;

				foreach (KeyValuePair<string, int> kvp in Database.crafting[recipe]) {
					if (!HasItem(kvp.Key, kvp.Value)) {
						has = false;
						break;
					} else if (GetItemCount(kvp.Key) == kvp.Value) garanteedSpace = true;
				}

				if (has && (HasSpaceFor(recipe) || garanteedSpace)) {
					foreach (KeyValuePair<string, int> kvp in Database.crafting[recipe]) {
						RemoveItem(kvp.Key, kvp.Value);
					}

					AddItem(recipe);

					core.PlayLocalSound("Craft");
				}
			}

			if (Input.GetKeyDown(KeyCode.R)) {
				crafting = !crafting;
				UpdateInventory();
			}
		}
	}

	public void UpdateInventory() {
		string inventoryText = "";

		if (crafting) {
			int i = 0;

			foreach (KeyValuePair<string, Dictionary<string, int>> kvp in Database.crafting) {
				if (craftingSelected == i) {
					inventoryText += ">";
					recipe = kvp.Key;
				}

				string ing = "";
				int i2 = 0;

				foreach (KeyValuePair<string, int> ingredient in kvp.Value) {
					ing += Database.items[ingredient.Key].name + " x" + ingredient.Value + (i2 != kvp.Value.Count - 1 ? ", " : "");
					i2++;
				}

				inventoryText += Database.items[kvp.Key].name + " (" + ing + ")";

				if (i < Database.crafting.Count - 1) inventoryText += "\n";

				i++;
			}
		} else {
			for (int i = 0; i < inventory.Length; i++) {
				if (selected == i) inventoryText += ">";

				if (inventory[i].id == "") {
					inventoryText += "<i>Empty</i>";
				} else {
					inventoryText += inventory[i].GetName() + ((inventory[i].amount > 1) ? (" x" + inventory[i].amount) : "");
				}

				if (i < inventory.Length - 1) inventoryText += "\n";
			}
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

			model.transform.localPosition = Vector3.zero;
			model.transform.localEulerAngles = Vector3.zero;
		}
	}

	public void CheckAnimations() {
		if (inventory[selected].id != "" && inventory[selected].GetData() is Gun) {
			Gun gun = (Gun) inventory[selected].GetData();

			core.animator.CrossFade(gun.model[1], 0.2f, 1);
			
			CmdUpdateToolModel(gun.model[0]);
			UpdateToolModel(gun.model[0]);
		} else if (inventory[selected].id != "" && inventory[selected].GetData() is Tool) {
			Tool t = (Tool) inventory[selected].GetData();

			core.animator.CrossFade("Hold", 0.2f, 1);
			
			CmdUpdateToolModel(t.model);
			UpdateToolModel(t.model);
		} else if (inventory[selected].id != "" && inventory[selected].GetData() is Food) {
			Food t = (Food) inventory[selected].GetData();

			core.animator.CrossFade("Hold", 0.2f, 1);
			
			CmdUpdateToolModel(t.model);
			UpdateToolModel(t.model);
		} else if (inventory[selected].id != "" && inventory[selected].GetData() is Throwable) {
			Throwable t = (Throwable) inventory[selected].GetData();

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
				if (inventory[i].id != "" && inventory[i].id == id) {
					inventory[i].amount += amount;

					UpdateInventory();

					return;
				}
			}
		}

		Item item = new Item(id, amount);

		for (int i = 0; i < inventory.Length; i++) {
			if (inventory[i].id == "") {
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
				if (inventory[i].id != "" && inventory[i].id == item.id) {
					inventory[i].amount += item.amount;

					UpdateInventory();

					return;
				}
			}
		}

		for (int i = 0; i < inventory.Length; i++) {
			if (inventory[i].id == "") {
				inventory[i] = item;

				UpdateInventory();

				return;
			}
		}
	}

	public bool HasSpaceFor(string n) {
		for (int i = 0; i < inventory.Length; i++) {
			if (inventory[i].id == "" || (inventory[i].id != "" && inventory[i].id == n && inventory[i].GetData().canStack)) {
				return true;
			}
		}

		return false;
	}

	public bool HasItem(string n, int amount = 1) {
		for (int i = 0; i < inventory.Length; i++) {
			if (inventory[i].id != "" && inventory[i].id == n && amount <= inventory[i].amount) return true;
		}

		return false;
	}

	public int GetItemCount(string n) {
		for (int i = 0; i < inventory.Length; i++) {
			if (inventory[i].id != "" && inventory[i].id == n) return inventory[i].amount;
		}

		return 0;
	}

	public void RemoveItem(string n, int amount = 1) {
		for (int i = 0; i < inventory.Length; i++) {
			if (inventory[i].id != "" && inventory[i].id == n) {
				inventory[i].amount -= amount;				

				if (inventory[i].amount <= 0) inventory[i].id = "";

				UpdateInventory();

				return;
			}
		}
	}
}
