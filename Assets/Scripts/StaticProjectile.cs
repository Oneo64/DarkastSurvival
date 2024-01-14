using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Mirror;

public class StaticProjectile : MonoBehaviour
{
	[Header("Movement")]
	public float speed = 50;
	public bool drop;
	public bool canRicochet = true;
	public bool canPenetrate = true;

	[Header("Damage")]
	public int maxDamage = 40;
	public int minDamage = 20;
	public float explosionRadiusMin;
	public float explosionRadiusMax;

	[Header("Cosmetics")]
	public int force = 2000;
	public int trailChance = 5;
	public float destroyTime;
	public float destroyTimeAfterSpawn = 2;
	public string hitName = "BulletHit";
	
	public Transform owner;

	bool stop;
	bool trails;

	float startTime;
	float gravity;

	Vector3 startPos;

	int penetration = 10;
	int accurateRange = 50;

	void Start() {
		startTime = Time.time;
		startPos = transform.position;

		if (Camera.main != null && Vector3.Distance(Camera.main.transform.position, transform.position) > 20) {
			transform.Find("Sound").GetComponent<AudioSource>().Play();
		}

		Vector3 oldPos = transform.position;

		transform.position += transform.forward * 1;

		if (!Check(oldPos, transform.position)) {
			if (GetComponent<TrailRenderer>()) GetComponent<TrailRenderer>().enabled = true;
		}
	}

	void Update() {
		if (!stop) {
			if (startTime + destroyTimeAfterSpawn < Time.time && !stop) {
				Destroy(gameObject);
			}

			if (Vector3.Distance(startPos, transform.position) > 0 && GetComponent<MeshRenderer>()) {
				GetComponent<MeshRenderer>().enabled = true;
			}

			if (drop && Vector3.Distance(startPos, transform.position) >= accurateRange) {
				gravity += Physics.gravity.y * Time.deltaTime;
				speed += (Physics.gravity.y / 2f) * Time.deltaTime;

				if (speed < 5) speed = 5;
				if (gravity < Physics.gravity.y) gravity = Physics.gravity.y;
			}

			//transform.Find("Sound").GetComponent<AudioSource>().maxDistance = speed <= 400 ? 20 : 40;

			Vector3 oldPos = transform.position;

			transform.position += ((transform.forward * speed) + (Vector3.up * gravity)) * Time.deltaTime;

			Check(oldPos, transform.position);
		}
	}

	private bool Check(Vector3 pos1, Vector3 pos2) {
		if (Physics.Linecast(pos1, pos2, out RaycastHit hit, LayerMask.GetMask(new string[] {"Default", "Player"}))) {
			if (hit.transform == owner || hit.transform.root == owner) return false;

			bool blood = false;

			if (NetworkClient.localPlayer.isServer) {
				int dmg = Random.Range(minDamage, maxDamage + 1);

				float dmg2 = dmg / 100f;
				int newForce = (int) Mathf.Round(force * (dmg2 * dmg2));

				if (hit.transform.name == "Skull") dmg *= 2;
				if (hit.transform.name.Contains("Arm") || hit.transform.name.Contains("Leg")) dmg = (int) Mathf.Round(dmg * 0.2f);
				if (speed <= 100) dmg = (int) Mathf.Round(dmg * 0.5f);

				if (explosionRadiusMax > 0) {
					Collider[] colliders = Physics.OverlapSphere(hit.point, explosionRadiusMax);

					foreach (Collider c in colliders) {
						if (c.name.Contains("LowerSpine")) {
							dmg = Random.Range(minDamage, maxDamage + 1);

							dmg2 = dmg / 100f;
							newForce = (int) Mathf.Round(force * (dmg2 * dmg2));

							Vector3 forceDir = (c.transform.position - hit.point).normalized;

							if (c.transform.GetComponentInParent<Enemy>()) {
								c.transform.GetComponentInParent<Enemy>().CmdDamage(
									dmg, forceDir * newForce, owner.GetComponent<PlayerCore>()
								);
								
								Blood(c.transform.position, forceDir, forceDir);
							}

							if (c.transform.GetComponentInParent<PlayerCore>()) {
								c.transform.GetComponentInParent<PlayerCore>().RpcDamage(dmg, forceDir * newForce);
							}

							if (c.transform.GetComponentInParent<HasBlood>()) Blood(c.transform.position, forceDir, forceDir);

							if (c.transform.GetComponentInParent<Rigidbody>()) {
								c.transform.GetComponentInParent<Rigidbody>().AddForceAtPosition(forceDir * (newForce / 10f), hit.point);
							}
						}
					}
				} else {
					if (hit.transform.GetComponentInParent<Enemy>()) {
						hit.transform.GetComponentInParent<Enemy>().CmdDamage(
							dmg, transform.forward * newForce, owner.GetComponent<PlayerCore>()
						);
					}

					if (hit.transform.GetComponentInParent<PlayerCore>()) {
						hit.transform.GetComponentInParent<PlayerCore>().RpcDamage(dmg, transform.forward * newForce);
					}
				}
			}

			blood = hit.transform.GetComponentInParent<HasBlood>() != null;

			if (hit.transform.GetComponent<Rigidbody>()) {
				hit.transform.GetComponent<Rigidbody>().AddForceAtPosition(transform.forward * force, hit.point);
			}

			transform.position = hit.point;

			if (blood && explosionRadiusMax == 0) Blood(hit.point, transform.forward, hit.normal);
				
			CreateBulletHole(hit, hit, blood);

			if (GetComponent<MeshRenderer>()) GetComponent<MeshRenderer>().enabled = false;

			bool canPenetrate2 = penetration > 0 && canPenetrate;
			bool isThin = Physics.Raycast(hit.point + (transform.forward * 0.5f), -transform.forward, out RaycastHit hit2, 0.5f, LayerMask.GetMask("Default"));

			if (canPenetrate2 && isThin) {
				Vector3 rand = new Vector3(Random.Range(-0.05f, 0.05f), Random.Range(-0.05f, 0.05f), Random.Range(-0.05f, 0.05f));
				
				transform.position = hit2.point + (transform.forward * 0.05f);
				transform.forward += rand;

				CreateBulletHole(hit, hit2, blood);

				penetration -= 1;
			} else if (canRicochet && Random.Range(1, 5) == 1 && Vector3.Angle(transform.forward, hit.normal) > 45) {
				Vector3 rand = new Vector3(Random.Range(-0.05f, 0.05f), Random.Range(-0.05f, 0.05f), Random.Range(-0.05f, 0.05f));
				
				transform.position = hit.point + (hit.normal * 0.05f);
				transform.forward = Vector3.Reflect(transform.forward, hit.normal);

				CreateBulletHole(hit, hit2, blood);

				penetration -= 1;
			} else {
				stop = true;

				StopSound();

				Destroy(gameObject, destroyTime);
			}

			return true;
		}

		return false;
	}

	private void StopSound() {
		if (transform.Find("Sound") != null) transform.Find("Sound").GetComponent<AudioSource>().Stop();
	}

	private void Blood(Vector3 pos, Vector3 dir, Vector3 normal) {
		Destroy(
			Instantiate(Resources.Load("Blood") as GameObject, pos, Quaternion.LookRotation(normal)),
			3
		);

		int amount = Random.Range(1, 3);

		for (int i = 0; i < amount; i++) {
			if (Physics.Raycast(pos, dir + new Vector3(Random.Range(-0.3f, 0.3f), Random.Range(-0.3f, 0.3f), Random.Range(-0.3f, 0.3f)), out RaycastHit hit, 5, LayerMask.GetMask("Default"))) {
				Transform bloodParent = null;

				if (hit.transform.parent && hit.transform.parent.tag == "Door") bloodParent = hit.transform;

				Vector3 rot = Quaternion.LookRotation(-hit.normal).eulerAngles;

				Destroy(
					Instantiate(
						Resources.Load("Blood") as GameObject,
						hit.point,
						Quaternion.Euler(rot + (Vector3.forward * Random.Range(0, 360))),
						bloodParent
					),
					30
				);
			}
		}
	}

	private void CreateBulletHole(RaycastHit hit, RaycastHit hit2, bool blood) {
		Transform hitParent = null;

		if ((hit.transform.parent && hit.transform.parent.tag == "Door") || hit.transform.GetComponent<Rigidbody>()) hitParent = hit.transform;

		GameObject g = Instantiate(Resources.Load(hitName), hit2.point, Quaternion.LookRotation(hit2.normal), hitParent) as GameObject;

		g.transform.eulerAngles += Vector3.forward * Random.Range(0, 360);

		if (hitParent != null) {
			g.transform.localScale = new Vector3(
				1f / hitParent.localScale.x,
				1f / hitParent.localScale.y,
				1f / hitParent.localScale.z
			);
		}

		if (g.transform.Find("BulletLand")) g.transform.Find("BulletLand").GetComponent<AudioSource>().pitch = Random.Range(0.4f, 0.6f);

		if (hitName == "BulletHit") {
			if (hit.transform.name == "Terrain") {
				g.transform.Find("Dirt").gameObject.SetActive(true);
			} else {
				MeshRenderer renderer = hit.transform.GetComponent<MeshRenderer>();

				if (renderer != null) {
					string n = "Default";

					foreach (KeyValuePair<string, string> effect in Database.landEffects) {
						if (renderer.materials[0].name.Contains(effect.Key)) {
							n = effect.Value;
						}
					}

					if (blood) n = "Blood";

					g.transform.Find(n).gameObject.SetActive(true);
				} else g.transform.Find(blood ? "Blood" : "Default").gameObject.SetActive(true);
			}
		}

		Destroy(g, 10);
	}
}
