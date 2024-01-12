using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Mirror;

public class ZombieColors {
	public static Color[] shirtColors = {
		new Color(1, 0, 0), new Color(1, 1, 0), new Color(0, 1, 0), new Color(0, 1, 1), new Color(0, 0, 1), new Color(1, 0, 1), Color.white, Color.black
	};

	public static Color[] pantsColors = {
		new Color(0, 0.8f, 0), new Color(0, 0, 0.8f), new Color(1, 0, 1), new Color32(245, 245, 220, 1), Color.white, Color.black
	};
}

public class Zombie : Enemy
{
	public SkinnedMeshRenderer renderer;
	[SyncVar(hook="UpdateAppearance")] public Color shirt;
	[SyncVar(hook="UpdateAppearance")] public Color pants;

	public int minDamage = 20;
	public int maxDamage = 40;

	void Initialize() {
		if (isServer) {
			shirt = ZombieColors.shirtColors[Random.Range(0, ZombieColors.shirtColors.Length)];
			pants = ZombieColors.pantsColors[Random.Range(0, ZombieColors.pantsColors.Length)];
		}
	}

	public void UpdateAppearance(Color oldColor, Color newColor) {
		Material shirtMat = renderer.materials[2];
		Material pantsMat = renderer.materials[0];

		shirtMat.SetColor("_BaseColor", shirt);
		pantsMat.SetColor("_BaseColor", pants);
	}

	public IEnumerator Attack() {
		animator.CrossFade("Attack", 0.2f);

		yield return new WaitForSeconds(0.4f);

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
		NetworkClient.localPlayer.GetComponent<PlayerCore>().CmdCreateRagdoll(
			ragdollname,
			transform.position,
			transform.eulerAngles,
			Database.GetLimbs(transform.Find("Armature"), agent.velocity + force),
			shirt, pants
		);

		Destroy(gameObject, 0);
	}
}
