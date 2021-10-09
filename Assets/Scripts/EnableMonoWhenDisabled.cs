using UnityEngine;
using System.Collections;
using HeavyDutyInspector;
using Diluvion;

[RequireComponent(typeof(Hull))]
public class EnableMonoWhenDisabled : MonoBehaviour
{
    [ComponentSelection()]
    public MonoBehaviour behaviour;

    public void OnDisable()
    {     
        EnableTarget();
    }

    public void EnableTarget()
    {
        if (!Application.isPlaying) return;
        if (behaviour == null) return;
        if (behaviour.gameObject.activeInHierarchy)
            behaviour.enabled = true;
        else
            if (behaviour.GetComponent<EnableMonoWhenDisabled>())
                behaviour.GetComponent<EnableMonoWhenDisabled>().EnableTarget();
    }
}