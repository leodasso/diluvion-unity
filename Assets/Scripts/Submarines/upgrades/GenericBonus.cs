using UnityEngine;
using System.Collections.Generic;
using Diluvion.Ships;
using Sirenix.OdinInspector;

[CreateAssetMenu(fileName = "new generic bonus", menuName = "Diluvion/subs/bonus/generic")]
public class GenericBonus : Forging {

    [FoldoutGroup("forge")]
    public List<ModifierValue> mods = new List<ModifierValue>();
    
    [FoldoutGroup("forge")]
    [Tooltip("These attachments get instantiated as children of the ship when this bonus is added.")]
    public List<GameObject> attachments = new List<GameObject>();

    public override bool ApplyToShip(Bridge ship, SubChassis chassis)
    {
        if (!base.ApplyToShip(ship, chassis)) return false;

        foreach (ModifierValue mod in mods)
            mod.mod.Modify(ship.GetComponent<Bridge>(), mod.value);

        // Instantiate the attachments into the ship
        foreach (var VARIABLE in attachments)
        {
            Instantiate(VARIABLE, ship.transform);
        }

        return true;
    }
}