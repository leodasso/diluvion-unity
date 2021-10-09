using UnityEngine;
using System.Collections;
using Diluvion;
using Diluvion.Ships;

public class AddSurfaceOnTrigger : MonoBehaviour
{
    public bool onlyPlayer = true;

    void OnTriggerEnter(Collider col)
    {
       AddSurfaceToBridge(col);
	    
    }

    void AddSurfaceToBridge(Collider col)
    {
	    Bridge b = col.GetComponent<Bridge>();
	    if (!b) return;
	    if (!b.IsPlayer()&&onlyPlayer) return;
	    SurfaceSub targetSurfaceSub = b.GetComponent<SurfaceSub>();
	    if (!targetSurfaceSub) return;
	    targetSurfaceSub.Surface();       
    }

	
	
}
