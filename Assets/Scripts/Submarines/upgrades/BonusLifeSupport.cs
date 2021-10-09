using UnityEngine;
using Diluvion.Ships;
using Sirenix.OdinInspector;


[CreateAssetMenu(fileName = "kit life support", menuName = "Diluvion/subs/bonus/life support")]
public class BonusLifeSupport : Forging {

    [TabGroup("forge")]
    public int extraCrew = 2;

    public override bool ApplyToShip(Bridge ship, SubChassis chassis)
    {
        if (!base.ApplyToShip(ship, chassis)) return false;

        Bridge br = ship.GetComponent<Bridge>();
        if (br == null)
        {
            Debug.LogError("No bridge found!", ship);
            return false;
        }

        else
        {
            int totalCrew = Mathf.RoundToInt(extraCrew * Multiplier(chassis));
            br.crewManager.maxCrew += totalCrew;
            return true;
        }
    }

    public override string LocalizedBody()
    {
        string body = base.LocalizedBody();
        return body.Replace("[c]", extraCrew.ToString());
    }
}
