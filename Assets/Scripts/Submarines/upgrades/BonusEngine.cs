using UnityEngine;
using System.Collections;
using Diluvion.Ships;
using Sirenix.OdinInspector;

[CreateAssetMenu(fileName = "kit engine bonus", menuName = "Diluvion/subs/bonus/engine")]
public class BonusEngine : Forging {

    [TabGroup("forge")]
    public float heatTolerance = 1;

    public override bool ApplyToShip(Bridge ship, SubChassis chassis)
    {
        if (!base.ApplyToShip(ship, chassis)) return false;
        
        RecalculateHeat(ship);
        return true;
    }

    public override bool RemoveFromShip(Bridge b, SubChassis sc)
    {
        if (!base.RemoveFromShip(b, sc)) return false;
        RecalculateHeat(b);
        return true;
    }

    void RecalculateHeat(Bridge b)
    {
        // add extra heat tolerance
        ShipMover mover = b.GetComponent<ShipMover>();
        if (!mover) return;

        mover.RecalculateHeatTolerance();
    }

    public override string LocalizedBody()
    {
        string body = base.LocalizedBody();
        body = body.Replace("[p]", heatTolerance.ToString("0"));
        return body;
    }
}
