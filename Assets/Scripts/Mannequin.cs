using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Mirror;

public class Mannequin : Enemy
{
	public int speed = 10;

	public SkinnedMeshRenderer renderer;
	public int minDamage = 20;
	public int maxDamage = 40;

	float alert;

	int maxHealth;

	void Initialize() {
		maxHealth = health;
	}

	public void UpdateLoop() {
		if (isServer) {
			int spd = speed;

			if (alert < Time.time) {
				foreach (Camera cam in Object.FindObjectsOfType<Camera>(true)) {
					Vector3 dir = (transform.position - cam.transform.position).normalized;

					if (Physics.Linecast(transform.position + (Vector3.up * 1.5f), cam.transform.position, LayerMask.GetMask("Default"))) continue;
					if (Vector3.Dot(cam.transform.forward, dir) > 0.6f) {
						spd = 0;
						break;
					}
				}
			}

			agent.speed = spd;
		}
	}

	public IEnumerator Attack() {
		animator.CrossFade("Attack", 0.2f);

		alert = Time.time + 1f;

		yield return new WaitForSeconds(0.4f);

		if (target != null && Vector3.Distance(transform.position, target.position) <= reach) {
			target.GetComponent<PlayerCore>().RpcDamage(Random.Range(minDamage, maxDamage + 1), transform.forward * 3);
		}
	}

	public void Hit() {
		alert = Time.time + 5f;
	}

	void Movement() {
		if (target != null) {
			SetAgentDestination(target.position);
		} else {
			Vector3 pos = transform.position + GetRandomPosition(5, 21);

			SetAgentDestination(pos);

			walkWait = Time.time + (Vector3.Distance(transform.position, pos) / agent.speed) + Random.Range(10, 21);
		}
	}

	void Die(Vector3 force) {
		NetworkClient.localPlayer.GetComponent<PlayerCore>().CmdCreateRagdoll(
			ragdollname,
			transform.position,
			transform.eulerAngles,
			Database.GetLimbs(transform.Find("Armature"), agent.velocity + force),
			Color.white, Color.white
		);

		Destroy(gameObject, 0);
	}
}
