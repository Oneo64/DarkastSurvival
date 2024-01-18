using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Database : Object
{
	// Item name
	// Model name, bullet name, animation to use, ammunition, sound close, sound far
	// Maximum bullets, automatic, min damage, max damage, bullet speed
	// Fire rate, spread, bullets per shot

	public static Dictionary<string, IItem> items = new Dictionary<string, IItem>() {
		{"makarov", new Gun(
			"Makarov", "bullet",
			new string[] {"makarov", "HoldPistol", "pistol_magazine"},
			7, false, 40, 60, 250,
			0.1f, 0.25f
		)},
		{"glock_17", new Gun(
			"Glock 17", "bullet",
			new string[] {"glock", "HoldPistol", "pistol_magazine"},
			10, false, 30, 40, 300,
			0.05f, 0.15f
		)},
		{"colt_navy", new Gun(
			"Colt 1851 Navy Revolver", "bullet",
			new string[] {"coltnavy", "HoldPistol", ".38_rimfire_box"},
			6, false, 50, 70, 200,
			0.4f, 0.5f
		)},

		{"ak_47", new Gun(
			"AK 47", "bullet",
			new string[] {"ak", "HoldRifle", "assault_rifle_magazine"},
			30, true, 50, 70, 500,
			0.1f, 0.25f
		)},
		{"handmade_rifle", new Gun(
			"Handmade Rifle", "bullet",
			new string[] {"handmaderifle", "HoldRifle", "assault_rifle_magazine"},
			20, true, 30, 40, 400,
			0.15f, 1f
		)},
		{"mossberg_500", new Gun(
			"Mossberg 500", "bullet",
			new string[] {"mossberg500", "HoldRifle", "12_guage_box"},
			8, false, 15, 17, 250,
			0.5f, 2, 9
		)},
		{"cz_455", new Gun(
			"CZ 455", "bullet",
			new string[] {"cz455", "HoldRifle", ".22_lr_magazine"},
			5, false, 140, 160, 600,
			1f, 0.05f
		)},
		{"p90", new Gun(
			"P90", "bullet",
			new string[] {"p90", "HoldRifle", "submachine_magazine"},
			50, true, 30, 40, 400,
			0.065f, 0.2f
		)},
		{"handmade_fire_pistol", new Gun(
			"Handmade Fire Pistol", "bullet",
			new string[] {"flamethrower", "HoldPistol", "propane_magazine"},
			5, false, 0, 0, 0,
			2, 0
		)},
		{"mac_10", new Gun(
			"MAC 10", "bullet",
			new string[] {"mac10", "HoldPistol", "submachine_magazine"},
			45, true, 30, 40, 250,
			0.05f, 0.1f
		)},
		{"energy_rifle", new Gun(
			"Energy Rifle", "bolt",
			new string[] {"energyrifle", "HoldRifle", "battery"},
			200, true, 15, 25, 150,
			0.1f, 0.5f
		)},

		{"flashlight", new Tool("Flashlight", "flashlight", false)},

		{"pistol_magazine", new Tool("Pistol Magazine", "magazine")},
		{"submachine_magazine", new Tool("Submachine Magazine", "magazine")},
		{"carbine_magazine", new Tool("Carbine Magazine", "magazine")},

		{"assault_rifle_magazine", new Tool("Assault Rifle Magazine", "magazine")},
		{"drum_magazine", new Tool("Drum Magazine", "magazine")},
		{"12_guage_box", new Tool("12 Guage Box", "magazine")},
		{".22_lr_magazine", new Tool(".22 Long Rifle Magazine", "magazine")},
		{".38_rimfire_box", new Tool(".38 Rimfire Box", "magazine")},
		{"propane_magazine", new Tool("Pistol Magazine (Propane Gas)", "magazine")},
		{"battery", new Tool("Battery", "battery")},
		{"wood", new Tool("Wood Plank", "wood")},
		{"metal", new Tool("Metal Scrap", "metal")},
		{"spring", new Tool("Spring", "spring")},
		{"gunpowder", new Tool("Gunpowder", "gunpowder")},
		{"string", new Tool("String", "string")},
		{"rope", new Tool("Rope", "rope")},
		{"paper", new Tool("Sheet of Paper", "paper")},

		{"flare", new Throwable("Flare", "flare")},
		{"grenade", new Throwable("Grenade", "grenade")},
		{"dynamite", new Throwable("Dynamite Bundle", "dynamite")},

		{"canned_soup", new Food("Canned Soup", "soup", 10)},
		{"canned_tuna", new Food("Canned Tuna", "tuna", 15)},
	};

	public static Dictionary<string, Dictionary<string, int>> crafting = new Dictionary<string, Dictionary<string, int>>() {
		{"handmade_rifle", new Dictionary<string, int>() {{"wood", 20}, {"metal", 5}, {"spring", 2}}},
		{"handmade_fire_pistol", new Dictionary<string, int>() {{"wood", 10}, {"metal", 4}}},
		{"grenade", new Dictionary<string, int>() {{"gunpowder", 6}, {"metal", 4}, {"string", 1}}},
		{"flare", new Dictionary<string, int>() {{"paper", 5}, {"rope", 2}, {"string", 1}}},
		{"dynamite", new Dictionary<string, int>() {{"paper", 20}, {"gunpowder", 15}, {"string", 10}}},
	};

	public static Dictionary<string, int> richochetProbs = new Dictionary<string, int>() {
		{"Metal", 4},
		{"Concrete", 8},
		{"Tiles", 8},
		{"Ice", 12},
		{"Grass", 16}
	};

	public static Dictionary<string, string> landEffects = new Dictionary<string, string>() {
		{"Metal", "Metal"},
		{"Grass", "Dirt"},
		{"Dirt", "Dirt"},
		{"Bark", "Wood"},
		{"Wood", "Wood"}
	};

	public static Limb[] GetLimbs(Transform armature, Vector3 force) {
		List<Limb> l = new List<Limb>() {};

		foreach (Transform t in armature.GetComponentsInChildren<Transform>()) {
			l.Add(new Limb(t.transform.name, t.transform.localPosition, t.transform.localEulerAngles, t.name == "Hip" ? force : Vector3.zero));
		}

		return l.ToArray();
	}
}

[System.Serializable]
public class Item {
	public string id;
	public int amount;

	public int externalData;

	public Item() {}

	public Item(string n, int a) {
		id = n;
		amount = a;
	}

	public string GetName() {
		return Database.items[id].name;
	}

	public IItem GetData() {
		return Database.items[id];
	}
}

public interface IItem {
	public string name { get; set; }
	public bool canStack { get; set; }
	public int amount { get; set; }
}

public struct Food : IItem {
	public string name { get; set; }
	public bool canStack { get; set; }
	public int amount { get; set; }

	public int heal;
	public string model;

	public Food(string n, string m, int h) {
		name = n;
		model = m;

		heal = h;

		canStack = true;
		amount = 1;
	}
}

public struct Junk : IItem {
	public string name { get; set; }
	public bool canStack { get; set; }
	public int amount { get; set; }

	public Junk(string n) {
		name = n;

		canStack = true;
		amount = 1;
	}
}

public struct Tool : IItem {
	public string name { get; set; }
	public bool canStack { get; set; }
	public int amount { get; set; }

	public string model;

	public Tool(string n, string m, bool stack = true) {
		name = n;
		model = m;

		canStack = stack;
		amount = 1;
	}
}

public struct Throwable : IItem {
	public string name { get; set; }
	public bool canStack { get; set; }
	public int amount { get; set; }

	public string model;

	public Throwable(string n, string m, bool stack = true) {
		name = n;
		model = m;

		canStack = stack;
		amount = 1;
	}
}

public struct Gun : IItem {
	public string name { get; set; }
	public bool canStack { get; set; }
	public int amount { get; set; }

	public string bullet;

	public string[] model;
	public int maxAmmunition;
	public int ammunition;
	public bool isAutomatic;

	public int minDamage;
	public int maxDamage;
	public int muzzleVelocity;

	public float fireRate;
	public float spread;
	public int shots;

	public Gun(string n, string b, string[] m, int ma, bool auto, int minDmg, int maxDmg, int vel, float fr, float s, int sh = 1) {
		name = n;
		bullet = b;
		model = m;
		maxAmmunition = ma;
		ammunition = 0;
		isAutomatic = auto;

		minDamage = minDmg;
		maxDamage = maxDmg;
		muzzleVelocity = vel;

		fireRate = fr;
		spread = s;
		shots = sh;

		canStack = false;
		amount = 1;
	}
}

public struct Limb {
	public string name;
	public Vector3 position;
	public Vector3 rotation;
	public Vector3 force;

	public Limb(string n, Vector3 p, Vector3 r, Vector3 f) {
		name = n;
		position = p;
		rotation = r;
		force = f;
	}
}

public enum Perk {
	NoPerk,
	Athlete,
	Engineer,
	ExplosionGuy,
	Monkey,
	Survivior
}