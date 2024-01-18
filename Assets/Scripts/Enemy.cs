using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.AI;

using Mirror;

public class Enemy : NetworkBehaviour
{
	public int detectionRange = 50;
	public int health = 100;
	public int score = 10;
	public float reach;
	public bool isDead;

	public float attackInterval = 1;
	public float specialAbilityInterval = 3;
	public float moveInterval = 0.5f;

	public string ragdollname = "HumanoidRagdoll";

	public Transform follow;
	public bool isLeader;
	public int leaderChance = 100;
	public string minionId;
	public int minionCount = 2;
	public int minionAttackDistance = 20;

	public bool slowReaction;

	[HideInInspector] public NavMeshAgent agent;
	[HideInInspector] public Animator animator;

	float searchWait;
	[HideInInspector] public float walkWait;
	[HideInInspector] public float attackWait;
	[HideInInspector] public float specialWait;
	float doorWait;
	float forgetWait;

	float seeWait;

	bool canSee;

	[HideInInspector] public Transform target;

	void Start() {
		agent = GetComponent<NavMeshAgent>();
		animator = GetComponent<Animator>();

		if (isServer && !agent.isOnNavMesh) TryFixAgent();

		if (isServer && isLeader) {
			if (Random.Range(1, 101) <= leaderChance) {
				for (int i = 0; i < Random.Range(minionCount, (minionCount * 2) + 1); i++) {
					GameObject minion = Instantiate(Resources.Load("Enemies/" + minionId) as GameObject, transform.position, Quaternion.identity);

					minion.GetComponent<Enemy>().follow = transform;
					minion.GetComponent<Enemy>().isLeader = false;

					NetworkServer.Spawn(minion);
				}
			}
		}

		SendMessage("Initialize", SendMessageOptions.DontRequireReceiver);
	}

	void Update() {
		if (isServer && !isDead) {
			if (searchWait < Time.time) {
				LookForEnemies();

				searchWait = Time.time + Random.Range(0.3f, 0.6f);
			}

			if (specialWait < Time.time) {
				specialWait = Time.time + Random.Range(specialAbilityInterval * 0.8f, specialAbilityInterval * 1.2f);

				SendMessage("SpecialAbility", SendMessageOptions.DontRequireReceiver);
			}

			if (target != null) {
				Vector3 dir = (target.position - transform.position).normalized;

				dir.y = 0;
				dir = dir.normalized;

				if (canSee) {
					if (attackWait < Time.time && Vector3.Angle(transform.forward, dir) <= 45 && Vector3.Distance(transform.position, target.position) <= reach) {
						attackWait = Time.time + Random.Range(attackInterval * 0.8f, attackInterval * 1.2f);

						SendMessage("Attack");
					}

					forgetWait = Time.time + 10;
				}

				if (seeWait < Time.time) {
					canSee = !Physics.Linecast(
						transform.position + (Vector3.up * 1.5f), target.position + (Vector3.up * 1.5f), LayerMask.GetMask("Default")
					);

					seeWait = Time.time + (Random.Range(0.5f, 1f) * (slowReaction ? 2 : 1));
				}
			}

			if (walkWait < Time.time && moveInterval > 0) {
				walkWait = Time.time + Random.Range(moveInterval, moveInterval * 2);

				if (follow != null && follow.GetComponent<Enemy>().target != null && Vector3.Distance(follow.position, follow.GetComponent<Enemy>().target.position) <= follow.GetComponent<Enemy>().minionAttackDistance) {
					SetAgentDestination(follow.GetComponent<Enemy>().target.position + GetRandomPosition(1f, 2f));
				} else if (follow != null && Random.Range(1, 5) != 1) {
					SetAgentDestination(follow.position + GetRandomPosition(2f, 4f));
				} else {
					SendMessage("Movement", SendMessageOptions.DontRequireReceiver);
				}
			}

			if (doorWait < Time.time) {
				foreach (Door door in Object.FindObjectsOfType<Door>()) {
					if (Vector3.Distance(door.transform.position, transform.position) < 2) {
						door.SendMessage("Interact");
						break;
					}
				}

				doorWait = Time.time + Random.Range(4f, 6f);
			}

			animator.SetBool("Walking", agent.velocity.magnitude >= 0.2f);
		}

		SendMessage("UpdateLoop", SendMessageOptions.DontRequireReceiver);
	}

	List<PlayerCore> players = new List<PlayerCore>();
	float playerGetWait = 0;

	public void LookForEnemies() {
		Transform newTarget = null;
		float dist = detectionRange;

		if (playerGetWait < Time.time) {
			players = new List<PlayerCore>(Object.FindObjectsOfType<PlayerCore>());
			playerGetWait = Time.time + Random.Range(4f, 6f);
		}

		foreach (PlayerCore player in players) {
			Transform targ = player.transform;

			if (!Physics.Linecast(transform.position + (Vector3.up * 1.5f), targ.position + (Vector3.up * 1.5f), LayerMask.GetMask("Default"))) {
				float dist2 = Vector3.Distance(transform.position, targ.position);

				if (dist2 < dist) {
					newTarget = targ;
					dist = dist2;
				}
			}
		}

		if (target != newTarget || (target == null && newTarget != null)) walkWait = 0;
		if (newTarget == null && forgetWait > Time.time) return;

		target = newTarget;
	}

	[Command(requiresAuthority = false)]
	public void CmdDamage(int damage, Vector3 force, PlayerCore source) {
		if (isDead) return;
		health -= damage;

		if (health <= 0) {
			isDead = true;
			agent.enabled = false;

			animator.SetBool("Walking", false);
			if (GetComponent<Ambience>()) RpcStopAmbience();

			if (source) source.score += score;

			SendMessage("Die", force, SendMessageOptions.DontRequireReceiver);
		} else {
			SendMessage("Hit", SendMessageOptions.DontRequireReceiver);
		}
	}

	public void SetAgentDestination(Vector3 pos) {
		if (agent && agent.isOnNavMesh) {
			NavMeshPath path0 = new NavMeshPath();

			if (NavMesh.CalculatePath(transform.position, pos, 1, path0)) {
				agent.SetPath(path0);
			} else {
				agent.SetDestination(pos);
			}
		}
	}

	public Vector3 GetRandomPosition(float min, float max) {
		Vector2 pos = Random.insideUnitCircle * Random.Range(min, max);

		return new Vector3(pos.x, 0, pos.y);
	}

	public void TryFixAgent() {
		if (!agent.isOnNavMesh) {
			agent.enabled = false;

			if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 20, NavMesh.AllAreas)) {
				transform.position = hit.position;
			}

			agent.enabled = true;
		}
	}

	[ClientRpc]
	public void RpcStopAmbience() {
		GetComponent<Ambience>().enabled = false;
	}

	void OnParticleCollision(GameObject particle) {
		if (isServer) {
			if (particle.transform.name == "Fire") {
				CmdDamage(1, particle.transform.forward * 5, particle.GetComponentInParent<PlayerCore>());
			}
		}
	}
}
