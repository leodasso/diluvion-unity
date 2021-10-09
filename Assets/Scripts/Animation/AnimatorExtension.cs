using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[RequireComponent(typeof(Animator))]
public class AnimatorExtension : MonoBehaviour {

    [ReadOnly]
    public Animator animator;

    public string boolName;

    private void Awake ()
    {
        animator = GetComponent<Animator>();
    }

    public void SetBool(bool value)
    {
        animator.SetBool(boolName, value);
    }
}
