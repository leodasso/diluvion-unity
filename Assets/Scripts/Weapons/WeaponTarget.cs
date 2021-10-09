using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpiderWeb;

/// <summary>
/// Weapon target is placed near a target that you want to shoot. Calculates lead with the given bullet speed
/// and shooter position.
/// </summary>
[RequireComponent(typeof(PseudoVelocity))]
public class WeaponTarget : MonoBehaviour {

    static GameObject prefab;
    PseudoVelocity velo;

    /// <summary>
    /// Creates an instance of weapon target.
    /// </summary>
    public static WeaponTarget Create()
    {
//        Debug.Log("Created WeaponTarget, nerd");
        WeaponTarget w = Instantiate(Prefab()).GetComponent<WeaponTarget>();
        return w;
    }

    static GameObject Prefab()
    {
        if (prefab) return prefab;
        return Resources.Load("weapon target") as GameObject;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, .5f);
    }
    
	void Start ()
    {
        velo = GetComponent<PseudoVelocity>();
	}

    public Vector3 Velocity()
    {
        velo = GO.MakeComponent<PseudoVelocity>(gameObject);
        return velo.velocity;
    }
}