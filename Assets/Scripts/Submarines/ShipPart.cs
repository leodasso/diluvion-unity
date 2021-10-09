using UnityEngine;
using System.Collections.Generic;
using Diluvion;
using Diluvion.Ships;

/// <summary>
/// A class with simple functions for easily accessing components of the ship 
/// from anywhere in the hierarchy.  </summary>
public class ShipPart : MonoBehaviour {

	/// <summary>
	/// Reference to the main bridge that controls these turrets
	/// </summary>
	Bridge bridge;

	/// <summary>
	/// Returns the bridge of the ship this part is on. 
	/// </summary>
	public Bridge FindBridge() {
		
		if (!GetComponent<Bridge>()) 
            return GetComponentInParent<Bridge>();

        return GetComponent<Bridge>();
    }
}