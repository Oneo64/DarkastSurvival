using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Mirror;

public class PlayerController : NetworkBehaviour
{
	public float bobSpeed = 2;
	public float bobIntensity = 0.1f;
	public Transform leftShoulder;
	public Transform rightShoulder;

	bool falling;
	float topYPos;
	float jumpWait;
	float hitWait;

	PlayerCore core;
	PlayerInventory inventory;
	Rigidbody controller;

	Vector2 rotation;
	Vector2 recoil;

	Vector3 camBobPos;

	float recoilTime;
	float bobTime;
	float bobMult;

	void Start() {
		core = GetComponent<PlayerCore>();
		inventory = GetComponent<PlayerInventory>();
		controller = GetComponent<Rigidbody>();

		if (!isLocalPlayer) {
			core.camera.gameObject.SetActive(false);
			controller.isKinematic = false;
		}

		camBobPos = core.camera.transform.localPosition;
	}

	void Update() {
		if (isLocalPlayer) {
			CheckMovement(!core.isDead);

			rotation += new Vector2(-Input.GetAxis("Mouse Y"), Input.GetAxis("Mouse X"));
			rotation.x = Mathf.Clamp(rotation.x, -80, 80);

			if (recoil != Vector2.zero) {
				rotation += recoil * Time.deltaTime;

				recoil = Vector2.Lerp(recoil, Vector2.zero, (Time.time - recoilTime) * 2);
			}

			if (rotation.y > 360) {
				rotation.y -= 360;
			}

			if (rotation.y < 0) {
				rotation.y += 360;
			}

			if (!core.isDead && transform.position.y < -200) {
				core.Damage(100, Vector3.zero);
			}

			Vector3 dir = controller.velocity;

			dir.y = 0;

			float bobX = Mathf.Sin(bobTime * bobSpeed * 0.5f) * bobMult * bobIntensity * 0.01f;
			float bobY = Mathf.Sin((bobTime + 0.5f) * bobSpeed) * bobMult * bobIntensity * 0.01f;

			bobTime += Time.deltaTime * Mathf.Max(dir.magnitude, 0.5f);

			if (dir.magnitude > 0.1f) bobMult += Time.deltaTime; else bobMult -= Time.deltaTime;

			bobMult = Mathf.Clamp(bobMult, 0.05f, 1);

			core.camera.transform.localPosition = camBobPos + (Vector3.right * bobX) + (Vector3.up * bobY);
			transform.localEulerAngles = Vector3.up * rotation.y;
			core.neck.localEulerAngles = Vector3.right * rotation.x;

			leftShoulder.localEulerAngles = new Vector3(rotation.x, 0, 102.339f);
			rightShoulder.localEulerAngles = new Vector3(rotation.x, 0, -102.339f);
		}

		GetComponent<AudioReference>().canPlay = Physics.CheckSphere(transform.position, 0.22f, LayerMask.GetMask(new string[] {"Default", "Ragdoll"}));
	}

	private void CheckMovement(bool canMove = true) {
		Vector3 way = Vector3.zero;
		float speed = 1;

		bool isGrounded = Physics.CheckSphere(transform.position, 0.22f, LayerMask.GetMask(new string[] {"Default", "Ragdoll"}));
		bool isUnderwater = transform.position.y < -0.5f;

		if (canMove) {
			if (Input.GetKey(KeyCode.W)) way.z += 1f;
			if (Input.GetKey(KeyCode.S)) {
				way.z -= 1f;
				speed = 0.5f;
			}
			if (Input.GetKey(KeyCode.A)) way.x -= 1f;
			if (Input.GetKey(KeyCode.D)) way.x += 1f;
		}

		if (Input.GetKey(KeyCode.LeftShift)) {
			int runSpeed = 6;

			if (core.perk == Perk.Athlete) runSpeed = 8; else if (core.perk == Perk.Monkey) runSpeed = 9;

			way = way.normalized * runSpeed * speed;
		} else {
			way = way.normalized * 3 * speed;
		}

		if (!falling && !isGrounded) falling = true;
		if (falling && isGrounded && topYPos - transform.position.y >= 1) falling = false;

		Vector3 velocity = (transform.right * way.x) + (transform.forward * way.z);

		controller.velocity = new Vector3(velocity.x, controller.velocity.y, velocity.z);

		if (isGrounded && canMove) {
			if (Input.GetKeyDown(KeyCode.Space) && jumpWait < Time.time) {
				controller.AddForce(Vector3.up * 6, ForceMode.VelocityChange);
				jumpWait = Time.time + 0.25f;
			}
		}

		if (!isGrounded && transform.position.y > topYPos) topYPos = transform.position.y;

		if (isGrounded && topYPos != -1000) {
			float diff = topYPos - transform.position.y;

			if (diff >= 3 && hitWait < Time.time) {
				core.Damage((int) Mathf.Ceil((diff - 3) * 10), controller.velocity);

				hitWait = Time.time + 0.1f;
			}

			topYPos = -1000;
		}

		core.animator.SetBool("Walking", way.magnitude > 0.5f && way.magnitude < 5f);
		core.animator.SetBool("Running", way.magnitude >= 5f);

		if (Input.GetKeyDown(KeyCode.LeftControl)) {
			core.animator.SetBool("Crouching", !core.animator.GetBool("Crouching"));
			core.animator.SetBool("Crawling", false);
		} else if (Input.GetKeyDown(KeyCode.C)) {
			core.animator.SetBool("Crawling", !core.animator.GetBool("Crawling"));
			core.animator.SetBool("Crouching", false);
		}
	}

	public void AddRecoil(Vector2 r, bool additive = false) {
		if (additive) recoil += r; else recoil = r;
		recoilTime = Time.time;
	}
}
