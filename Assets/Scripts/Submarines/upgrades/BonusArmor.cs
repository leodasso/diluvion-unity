using UnityEngine;
using Diluvion.Ships;
using Diluvion;
using Sirenix.OdinInspector;

[CreateAssetMenu(fileName = "new armor bonus", menuName = "Diluvion/subs/bonus/Armor")]
public class BonusArmor : Forging
{
    [TabGroup("forge")]
    public int HP = 1;
    private const float dangerHPFactor = 2;
    
    public override bool ApplyToShip(Bridge ship, SubChassis chassis)
    {
        if (!base.ApplyToShip(ship, chassis)) return false;

        //Debug.Log("adding " + HP + " armor to " + ship.name);

        Hull h = ship.GetComponent<Hull>();
        if (h == null)
        {
            Debug.LogError("Couldn't find hull while trying to add armor!", ship);
            return false;
        }

        float totalHP = HP * Multiplier(chassis);

        h.maxHealth += totalHP;
        h.currentHealth += totalHP;
        //Debug.Log("applying hp: " + totalHP + " to " + h.name);

        return true;
    }

    public override bool RemoveFromShip(Bridge b, SubChassis sc)
    {
        if (!base.RemoveFromShip(b, sc)) return false;
        
        Debug.Log("removing " + HP + " armor to " + b.name);

        Hull h = b.GetComponent<Hull>();
        if (h == null)
        {
            Debug.LogError("Couldn't find hull while trying to remove armor!", b);
            return false;
        }

        float totalHP = HP * Multiplier(sc);

        h.maxHealth -= totalHP;
        h.currentHealth -= totalHP;
        Debug.Log("removing hp: " + totalHP + " from " + h.name);
        return true;
    }

    public override int Danger()
    {
        return Mathf.RoundToInt(HP * dangerHPFactor);
    }

    public override string LocalizedBody()
    {
        string body = base.LocalizedBody();
        return body.Replace("[HP]", HP.ToString());
    }
}
