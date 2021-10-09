using UnityEngine;
using System.Collections.Generic;
using Sirenix.OdinInspector;

/// <summary>
/// Contains a list of selected mesh renderers to apply skin to.
/// </summary>
/// 
namespace Diluvion
{


public class Skin : MonoBehaviour {

  
    public Material skin;
    public Material damagedSkin;

   
    public List<MeshRenderer> skinRenderers = new List<MeshRenderer>();

   
    [Button]
    void GetSkinRenderers()
    {
        skinRenderers.Clear();
        foreach (MeshRenderer mr in GetComponentsInChildren<MeshRenderer>())
        {
            if (mr.GetComponent<Mount>()) continue;
            if (mr.gameObject.layer == LayerMask.NameToLayer("Interior")) continue;
            skinRenderers.Add(mr);
        }
    }

    [Button]
    void ApplySkin() { ApplySkin(skin); }
    
    public void ApplySkin(Material m)
    {
        if (!m) return;
        skin = m;
        foreach (MeshRenderer mr in skinRenderers)
        {
            if (mr == null) continue;
            mr.sharedMaterial = m;
        }
    }
}
}