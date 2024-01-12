using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Mirror;

public class Door : NetworkBehaviour
{
	public Animator animator;
	bool open;

	void Interact() {
		CmdInteract();
	}

	[Command(requiresAuthority = false)]
	private void CmdInteract() {
		open = !open;

		animator.SetBool("IsOpen", open);
	}
}
