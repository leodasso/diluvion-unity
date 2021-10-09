using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivateAnimator : MonoBehaviour
{
	private Animator animator;

	private Animator MyAnimator
	{
		get
		{
			if (animator != null) return animator;
			return animator = GetComponent<Animator>();
		}
	}

	public void Activate()
	{
		MyAnimator.enabled = true;
	}
	
}
