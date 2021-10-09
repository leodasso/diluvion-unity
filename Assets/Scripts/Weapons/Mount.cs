using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Diluvion;
using FluffyUnderware.DevTools.Extensions;

public class Mount : WeaponPart
{

    // Event for every time this mount is fired
    public event System.Action onFire;
    
    public Vector3 launchDir;
    //this is true if the mount is pointed at the target close enough to hit
   
    public bool mountLOScheck;
    public bool hideMesh = true;
    public Cannon cannonInstance;

    //Dot product threshold
    float closeEnoughToFire = 0.995f;
    bool targetisLOS = false;
    List<WeaponPart> parentParts = new List<WeaponPart>();
		
	void Start ()
    {
        //losMask = LayerMask.GetMask("BlockSonar");
        parentParts.Clear();
        parentParts.AddRange(GetComponentsInParent<WeaponPart>());
	}

    /// <summary>
    /// Equips the new cannon, and returns the equipped instance.
    /// </summary>
    public Cannon EquipCannon(Cannon newCannon)
    {
        if (newCannon == null) return null;

        // destroy the old cannon instance
        List<GameObject> children = new List<GameObject>();
        foreach (Transform t in transform) children.Add(t.gameObject);

        if (Application.isPlaying)
            children.ForEach(child => Destroy(child));
        else children.ForEach(child => DestroyImmediate(child));
        

        cannonInstance = Instantiate(newCannon, transform.position, transform.rotation) as Cannon;
        cannonInstance.transform.SetParent(transform);

        //Turn off renderer of mount visuals
        if (GetComponent<Renderer>() && hideMesh) GetComponent<Renderer>().enabled = false;
        return cannonInstance;
    }

    /// <summary>
    /// Clears delegate functions from the onFire action
    /// </summary>
    public void ClearOnFire()
    {
        onFire = null;
    }


    public GameObject FireWeapon() 
	{
	    onFire?.Invoke();
	    
		return cannonInstance == null ? null : cannonInstance.Fire();
	}

    /// <summary>
    /// Mounts are fire ready if all parent turrets are also fire ready.
    /// </summary>
    public override bool FireReady()
    {
        foreach (WeaponPart w in parentParts)
        {
            TurretRotator t = w as TurretRotator;
            if (!t) continue;
            if (!t.FireReady()) return false;
        }

        return true;
    }
}