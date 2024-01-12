using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Mirror;

public class ShadowMonster : Enemy
{
	public int minDamage = 20;
	public int maxDamage = 40;

	void Initialize() {
		if (isServer) {
		}
	}

	public IEnumerator Attack() {
		animator.CrossFade("Attack", 0.2f);

		yield return new WaitForSeconds(0.4f);

		if (target != null && Vector3.Distance(transform.position, target.position) <= reach && Vector3.Dot((target.position - transform.position).normalized, transform.forward) > 0.5f) {
			target.GetComponent<PlayerCore>().RpcDamage(Random.Range(minDamage, maxDamage + 1), transform.forward * 3);
		}
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
			Color.black, Color.black
		);

		Destroy(gameObject, 0);
	}
}
