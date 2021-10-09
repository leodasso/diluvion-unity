using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Diluvion.Ships;

/// <summary>
/// Allows a ship to use overdrive when equipped
/// </summary>
[CreateAssetMenu(fileName = "new overdrive", menuName = "Diluvion/subs/bonus/overdrive")]
public class Overdrive : Forging {

    public override bool ApplyToShip (Bridge ship, SubChassis chassis)
    {
        if (!base.ApplyToShip(ship, chassis)) return false;

        ShipMover m = ship.GetComponent<ShipMover>();
        m.overdriveInstalled = true;

        return true;
    }
}
