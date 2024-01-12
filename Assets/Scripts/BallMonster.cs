using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Mirror;

public class BallMonster : Enemy
{
	public Transform body;
	public int normalRoll = 5;
	public int fastRoll = 10;

	public int minDamage = 8;
	public int maxDamage = 10;

	public ParticleSystem deathParticles;
	public ParticleSystem attack;

	void Initialize() {
		if (isServer) {
		}
	}

	public void UpdateLoop() {
		if (isServer && !isDead) {
			if (body != null && target != null) {
				body.rotation = Quaternion.RotateTowards(body.rotation, Quaternion.LookRotation((target.position - transform.position).normalized), Time.deltaTime * 180);
			}
		}
	}

	public void Attack() {
		if (target != null && Vector3.Distance(transform.position, target.position) <= reach) {
			if (attack != null) {
				if (agent.velocity.magnitude < 0.1f && Vector3.Dot((target.position - transform.position).normalized, body.forward) > 0.5f) attack.Play();
			} else {
				target.GetComponent<PlayerCore>().RpcDamage(Random.Range(minDamage, maxDamage + 1), transform.forward * (agent.speed * 500));
			}
		}
	}

	void Movement() {
		if (target != null) {
			if (attack != null) {
				SetAgentDestination(target.position + ((transform.position - target.position).normalized * (reach - 5)));
			} else {
				SetAgentDestination(target.position);

				agent.speed = Vector3.Distance(transform.position, target.position) <= 20 ? fastRoll : normalRoll;
			}
		} else {
			Vector3 pos = transform.position + GetRandomPosition(20, 41);

			SetAgentDestination(pos);

			walkWait = Time.time + (Vector3.Distance(transform.position, pos) / agent.speed) + Random.Range(20, 31);

			agent.speed = normalRoll;
		}
	}

	void Die(Vector3 force) {
		deathParticles.Play();
		GetComponent<Collider>().enabled = false;
		deathParticles.transform.parent.GetComponent<Renderer>().enabled = false;

		Destroy(gameObject, 10);
	}
}
