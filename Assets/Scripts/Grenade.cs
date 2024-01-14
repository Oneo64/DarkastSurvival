using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Mirror;

public class Grenade : NetworkBehaviour
{
	public float fuse = 4;

	[Header("Damage")]
	public int maxDamage = 40;
	public int minDamage = 20;
	public float explosionRadiusMin;
	public float explosionRadiusMax;

	[Header("Cosmetics")]
	public int force = 2000;
	public float destroyTime;

	public Transform owner;

	IEnumerator Start() {
		if (!isServer) GetComponent<Rigidbody>().isKinematic = true;

		yield return new WaitForSeconds(fuse);
		Check();
	}

	private void Check() {
		if (NetworkClient.localPlayer.isServer) {
			int dmg = Random.Range(minDamage, maxDamage + 1);

			float dmg2 = dmg / 100f;
			int newForce = (int) Mathf.Round(force * (dmg2 * dmg2));

			Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadiusMax);

			foreach (Collider c in colliders) {
				Vector3 forceDir = (c.transform.position - transform.position).normalized;

				dmg = Random.Range(minDamage, maxDamage + 1);

				dmg2 = dmg / 100f;
				newForce = (int) Mathf.Round(force * (dmg2 * dmg2));					

				if (c.transform.GetComponentInParent<Enemy>()) {
					c.transform.GetComponentInParent<Enemy>().CmdDamage(
						dmg, forceDir * newForce, owner.GetComponent<PlayerCore>()
					);
				}

				if (c.transform.GetComponentInParent<PlayerCore>()) {
					c.transform.GetComponentInParent<PlayerCore>().RpcDamage(dmg, forceDir * newForce);
				}

				if (c.transform.GetComponentInParent<HasBlood>()) Blood(c.transform.position, forceDir, forceDir);

				if (c.transform.GetComponent<Rigidbody>() && c.transform.name == "LowerSpine") {
					c.transform.GetComponent<Rigidbody>().AddForceAtPosition(forceDir * newForce, transform.position);
				}
			}

			GetComponent<MeshRenderer>().enabled = false;
			GetComponent<Rigidbody>().isKinematic = true;
			GetComponent<MeshCollider>().enabled = false;

			RpcExplode();

			Destroy(gameObject, destroyTime);
		}
	}

	[ClientRpc]
	private void RpcExplode() {
		if (transform.Find("Sound")) transform.Find("Sound").GetComponent<AudioSource>().Play();
		transform.Find("Flash").GetComponent<ParticleSystem>().Play();
		transform.Find("Debris").GetComponent<ParticleSystem>().Play();
		transform.Find("Smoke").GetComponent<ParticleSystem>().Play();
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
}
