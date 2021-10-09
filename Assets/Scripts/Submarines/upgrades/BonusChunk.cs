using UnityEngine;
using System.Collections.Generic;
using Loot;
using Diluvion.SaveLoad;
using Diluvion;
using Diluvion.Ships;
using Sirenix.OdinInspector;

public abstract class Forging : DItem
{
    [FoldoutGroup("forge")]
    [Space] public List<StackedItem> synthesisMaterials = new List<StackedItem>();
    
    [FoldoutGroup("forge")] public bool canDismantle;
    
    [FoldoutGroup("forge")]
    public GameObject visuals;
    
    [FoldoutGroup("forge")]
    public Sprite previewSprite;
    
    [FoldoutGroup("forge")]
    public bool onePerShip;

    public override void Use()
    {
        GameObject playerShip = PlayerManager.PlayerShip();
        if (playerShip == null)
        {
            Debug.LogError("Player ship couldnt be found while trying to use " + name);
            return;
        }

        if (DSave.current == null)
        {
            Debug.LogError("Save file couldnt be found while trying to use " + name);
            return;
        }

        SubChassis playerSubChassis = DSave.current.playerShips[0].ChassisObject();
        if (playerSubChassis == null)
        {
            Debug.LogError("Chassis " + DSave.current.playerShips[0].chassisName + " couldn't be found while trying to use " + name);
            return;
        }

        ApplyToShip(playerShip.GetComponent<Bridge>(), playerSubChassis);
    }

    [Button]
    public void ApplyToPlayerShip()
    {
        Bridge b = PlayerManager.pBridge;
        if (!b)
        {
            Debug.LogError("No player ship exists.");
            return;
        }

        SubChassis playerChassis = b.chassis;
        ApplyToShip(b, playerChassis);
    }

    /// <summary>
    /// Remoevs this bonus's synthesis materials from the player's inventory.
    /// </summary>
    public void TakeSynthMaterials()
    {
        Inventory playerInv = PlayerManager.PlayerInventory();
        if (!playerInv)
        {
            Debug.LogError("No player inventory! Can't take synthesis items.", this);
            return;
        }
        
        
        foreach (var stack in synthesisMaterials)
        {
            playerInv.RemoveItem(stack);
        }
    }

    /// <summary>
    /// Adds the required materials to the player inventory
    /// </summary>
    [Button]
    void AddSynthMaterials()
    {
        Inventory playerInv = PlayerManager.PlayerInventory();
        if (!playerInv)
        {
            Debug.LogError("No player inventory!", this);
            return;
        }

        foreach (var stack in synthesisMaterials)
            playerInv.AddItem(stack);
    }

    /// <summary>
    /// Returns the list of items that will be returned when this upgrade is dismantled
    /// </summary>
    public List<StackedItem> DismantleMaterials()
    {
        List<StackedItem> materials = new List<StackedItem>();
        
        float salvagedPortion = GameManager.Mode().dismantleReturns;
        
        foreach (var stack in synthesisMaterials)
        {
            int qty = Mathf.CeilToInt(stack.qty * salvagedPortion);
            materials.Add(new StackedItem(stack.item, qty));
        }

        return materials;
    }

    /// <summary>
    /// This applies the bonus to the instance of the object. When a ship is spawned, every one of its
    /// bonuses will run this function.
    /// </summary>
    public virtual bool ApplyToShip(Bridge b, SubChassis sc)
    {
        //Debug.Log("Applying chunk " + name + " to ship.", b);      
        if (b == null) return false;
        if (onePerShip && b.bonusChunks.Contains(this))
        {
            Debug.Log("Didn't add " + name + " because a ship can only contain one of them.");
            return false;
        }

        // Check if the ship is full
        if (b.bonusChunks.Count >= sc.bonusSlots)
        {
            Debug.Log("Didn't add " + name + " because the ship has no slots remaining.");
            return false;
        }
        
        // add this to the bridge's list of bonuses
        b.bonusChunks.Add(this);

        BonusSlot slot = b.NextAvailableBonusSlot();
        if (!slot)
        {
            Debug.LogError("No slots were available on " + b.name + " even though they should be. Check that the chassis" +
                           " 'bonusSlots' count matches the actual number of slots in the prefab. This won't prevent stats from being added, " +
                           "but it will result in the bonus chunk cosmetics not showing properly.");
        }
        else slot.ApplyBonus(this);

        return true;
    }

    /// <summary>
    /// Removes this upgrade slot from the given ship. 
    /// </summary>
    public virtual bool RemoveFromShip(Bridge b, SubChassis sc)
    {
        Debug.Log("removing chunk " + name + " to ship.", b);      
        if (b == null) return false;
        if (!b.bonusChunks.Contains(this))
        {
            Debug.LogError("can't remove " + name + " because " + b.name + " doesn't have it.");
            return false;
        }
        
        // remove this from the bridge's list of bonuses
        b.bonusChunks.Remove(this);

        BonusSlot slot = b.FindBonusSlotWithChunk(this);
        if (!slot)
        {
            Debug.LogError("No bonus slots on " + b.name + " contained " + name + ", so it can't be removed.");
            return false;
        }
        slot.RemoveBonus();

        return true;
    }

    public virtual bool CanDismantle()
    {
        return canDismantle;
    }

    protected float Multiplier(SubChassis forChassis)
    {
        if (forChassis.chassisBonus == null) return 1;

        System.Type myType = GetType();
        System.Type bonusType = forChassis.chassisBonus.GetType();

        //Debug.Log("Comparing " + myType + " and " + bonusType);
        if (bonusType == myType) return forChassis.bonusMultiplier;

        //Debug.Log("Comparison failed.");
        return 1;
    }
}