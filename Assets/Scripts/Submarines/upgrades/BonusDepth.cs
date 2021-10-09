using System.Collections.Generic;
using UnityEngine;
using Diluvion.Ships;
using Diluvion;
using DUI;
using Sirenix.OdinInspector;

[CreateAssetMenu(fileName = "kit pressure resist", menuName = "Diluvion/subs/bonus/depth")]
public class BonusDepth : Forging {

    [TabGroup("forge")]
    public float extraDepth = 150;

    [TabGroup("forge"), Tooltip("Popup that appears when player tries to dismantle a depth upgrade that will put their ship in danger.")]
    public PopupObject cantDismantlePopup;

    public override bool ApplyToShip(Bridge ship, SubChassis chassis)
    {
        if (!base.ApplyToShip(ship, chassis)) return false;
        Hull hull = ship.GetComponent<Hull>();

        if (hull == null)
        {
            Debug.LogError("No hull found!", ship);
            return false;
        }

        float totalDepth = Mathf.Round(extraDepth * Multiplier(chassis));
        hull.testDepth -= totalDepth;
        return true;
    }

    public override bool RemoveFromShip(Bridge b, SubChassis sc)
    {
        if (!base.RemoveFromShip(b, sc)) return false;
        
        Hull hull = b.GetComponent<Hull>();

        if (hull == null)
        {
            Debug.LogError("No hull found!", b);
            return false;
        }

        float totalDepth = Mathf.Round(extraDepth * Multiplier(sc));
        hull.testDepth += totalDepth;
        return true;
        
    }

    public override string LocalizedBody()
    {
        string body = base.LocalizedBody();
        return body.Replace("[d]", extraDepth.ToString());
    }

    [Button()]
    void TestDismantleCheck()
    {
        CanDismantle();
    }
    
    public override bool CanDismantle()
    {
        var ship = PlayerManager.PlayerShip();
        if (!ship) return false;

        float depth = ship.transform.position.y;
        Debug.Log("Player ship's current depth is " + depth);

        float crushDepth = PlayerManager.PlayerHull().testDepth;
        Debug.Log("Player ship's current crush depth is " + crushDepth);

        // How much the crush depth will be if this chunk is removed
        float pendingCrushDepth = crushDepth + extraDepth;
        Debug.Log("If " + name + " is removed from player ship, the new crush depth will be " + pendingCrushDepth);

        if (pendingCrushDepth > depth + 10)
        {
            Debug.Log("Can't dismantle " + name + " because it will put the player ship in danger.");
            cantDismantlePopup.CreateUI();
            return false;
        }
        
        return base.CanDismantle();
    }
}