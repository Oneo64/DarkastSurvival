using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Mirror;

public class Worm : Enemy
{
	public List<Vector3> wormPoints;
	public Transform wormStart;
	public ParticleSystem deathParticles;

	public int minDamage = 30;
	public int maxDamage = 50;

	float pointAddWait;

	void Initialize() {
		Transform p = wormStart;

		for (int i = 0; i < 13; i++) {
			wormPoints.Add(p.position);

			p = p.GetChild(0);
		}
	}

	public void UpdateLoop() {
		if (isServer && !isDead) {
			float dist = Vector3.Distance(wormStart.position, wormPoints[0]);

			if (dist > 1) {
				wormPoints.Insert(0, wormStart.position);
				dist = 0;
			}

			if (wormPoints.Count > 20) wormPoints.RemoveAt(wormPoints.Count - 1);

			Transform p = wormStart.GetChild(0);

			for (int i = 1; i < 13; i++) {
				p.position = Vector3.Lerp(wormPoints[i], wormPoints[i - 1], 1 - dist);

				Vector3 angles = Quaternion.LookRotation((wormPoints[i - 1] - wormPoints[i]).normalized).eulerAngles;

				angles.x -= 90;

				p.eulerAngles = angles;

				p = p.GetChild(0);
			}
		}
	}

	public IEnumerator Attack() {
		animator.CrossFade("Attack", 0.2f);

		yield return new WaitForSeconds(0.9f);

		if (target != null && Vector3.Distance(transform.position, target.position) <= reach) {
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
		deathParticles.Play();

		Destroy(gameObject, 30);
	}
}
