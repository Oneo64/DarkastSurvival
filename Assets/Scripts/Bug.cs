using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Mirror;

public class Bug : Enemy
{
	public int minDamage = 5;
	public int maxDamage = 10;

	public IEnumerator Attack() {
		animator.CrossFade("Attack", 0.4f);

		yield return new WaitForSeconds(0.4f);

		if (target != null && Vector3.Distance(transform.position, target.position) <= reach) {
			target.GetComponent<PlayerCore>().RpcDamage(Random.Range(minDamage, maxDamage + 1), transform.forward * 3);
		}
	}

	void Movement() {
		if (target != null) {
			SetAgentDestination(target.position + GetRandomPosition(0, 1f));
		} else {
			Vector3 pos = transform.position + (isLeader ? GetRandomPosition(5f, 10f) : GetRandomPosition(2f, 4f));

			SetAgentDestination(pos);

			walkWait = Time.time + (Vector3.Distance(transform.position, pos) / agent.speed) + Random.Range(3f, 6f);
		}
	}

	void Die(Vector3 force) {
		animator.SetBool("Walking", false);
		Destroy(gameObject, 10);
	}
}
