using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Mirror;

public class Infector : Enemy
{
	public int minDamage = 20;
	public int maxDamage = 40;
	public int raiseDistance = 10;

	public IEnumerator Attack() {
		animator.CrossFade("Attack", 0.2f);

		yield return new WaitForSeconds(0.4f);

		if (target != null && Vector3.Distance(transform.position, target.position) <= reach) {
			target.GetComponent<PlayerCore>().RpcDamage(Random.Range(minDamage, maxDamage + 1), transform.forward * 3);
		}
	}

	public IEnumerator SpecialAbility() {
		Transform t = LookForBody();

		if (t && Vector3.Distance(transform.position, t.position) <= raiseDistance) {
			animator.CrossFade("Attack", 0.2f);

			yield return new WaitForSeconds(0.4f);

			if (target != null) {
				GameObject minion = Instantiate(Resources.Load("Enemies/" + minionId) as GameObject, t.position, Quaternion.identity);

				minion.GetComponent<Enemy>().follow = transform;
				minion.GetComponent<Enemy>().isLeader = false;

				NetworkServer.Spawn(minion);

				Destroy(t.gameObject);
			}
		}
	}

	public Transform LookForBody() {
		Transform newTarget = null;
		float dist = detectionRange;

		foreach (GameObject body in GameObject.FindGameObjectsWithTag("Ragdoll")) {
			Transform targ = body.transform;

			if (!Physics.Linecast(transform.position + (Vector3.up * 1.5f), targ.position + (Vector3.up * 1.5f), LayerMask.GetMask("Default"))) {
				float dist2 = Vector3.Distance(transform.position, targ.position);

				if (dist2 < dist) {
					newTarget = targ;
					dist = dist2;
				}
			}
		}

		return newTarget;
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
