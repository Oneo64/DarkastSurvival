using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Mirror;

public class Soldier : Enemy
{
	public SkinnedMeshRenderer renderer;
	[SyncVar(hook="UpdateAppearance")] public Color shirt;
	[SyncVar(hook="UpdateAppearance")] public Color pants;

	public Transform weapon;
	public string gunId = "ak_47";

	void Initialize() {
		if (isServer) {
			shirt = ZombieColors.shirtColors[Random.Range(0, ZombieColors.shirtColors.Length)];
			pants = ZombieColors.pantsColors[Random.Range(0, ZombieColors.pantsColors.Length)];

			Gun gun = (Gun) Database.items[gunId];

			GetComponent<Animator>().CrossFade(gun.model[1], 0.2f, 1);
		}
	}

	public void UpdateLoop() {
		if (target != null) {
			Vector3 dir = (target.position - transform.position).normalized;

			dir.y = 0;
			dir = dir.normalized;

			transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(dir), 500 * Time.deltaTime);
		}
	}

	public void UpdateAppearance(Color oldColor, Color newColor) {
		Material shirtMat = renderer.materials[2];
		Material pantsMat = renderer.materials[0];

		shirtMat.SetColor("_BaseColor", shirt);
		pantsMat.SetColor("_BaseColor", pants);
	}

	public void Attack() {
		Gun gun = (Gun) Database.items[gunId];
		Vector3[] directions = new Vector3[gun.shots];

		float spread = gun.spread * 10;

		for (int i = 0; i < gun.shots; i++) {
			directions[i] = weapon.eulerAngles + (new Vector3(
				Random.Range(-spread, spread),
				Random.Range(-spread, spread),
				Random.Range(-spread, spread)
			));
		}

		RpcFireGun(weapon.position + (Vector3.up * 0.02f) - (weapon.forward * 0.2f), directions, gun);

		if (Random.Range(1, 6) == 1) {
			attackWait = Time.time + Random.Range(2f, 3f);
		}
	}

	[ClientRpc]
	private void RpcFireGun(Vector3 pos, Vector3[] dir, Gun gun) {
		FireGun(pos, dir, gun);
	}

	private void FireGun(Vector3 pos, Vector3[] dir, Gun gun) {
		for (int i = 0; i < dir.Length; i++) {
			GameObject g = Instantiate(Resources.Load("Bullet") as GameObject, pos, Quaternion.Euler(dir[i]));

			g.GetComponent<StaticProjectile>().minDamage = gun.minDamage;
			g.GetComponent<StaticProjectile>().maxDamage = gun.maxDamage;
			g.GetComponent<StaticProjectile>().speed = gun.muzzleVelocity;
			g.GetComponent<StaticProjectile>().owner = transform;
		}

		if (weapon.childCount > 0 && weapon.GetChild(0) != null && weapon.GetChild(0).Find("ShootNear") != null) {
			foreach (Transform t in weapon.GetChild(0)) {
				if (t.GetComponent<ParticleSystem>()) t.GetComponent<ParticleSystem>().Play();
			}
		
			AudioSource audio1 = weapon.GetChild(0).Find("ShootNear").GetComponent<AudioSource>();
			AudioSource audio2 = weapon.GetChild(0).Find("ShootFar").GetComponent<AudioSource>();

			audio1.PlayOneShot(audio1.clip);
			audio2.PlayOneShot(audio2.clip);
		}
	}

	void Movement() {
		if (target != null) {
			SetAgentDestination(transform.position + GetRandomPosition(2, 4));
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
