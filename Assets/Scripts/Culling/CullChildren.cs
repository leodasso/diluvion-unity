using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CullChildren : CullerBase 
{
    protected override bool SetState(bool enabled)
    {
        foreach (Transform t in transform) t.gameObject.SetActive(enabled);
        return base.SetState(enabled);
    }
}
