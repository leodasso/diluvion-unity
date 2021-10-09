using UnityEngine;
using SpiderWeb;
using Sirenix.OdinInspector;

[System.Serializable]
[CreateAssetMenu( menuName = "Diluvion/Characters/Crew Stat")]
public class CrewStatObject : StatObject
{

    [BoxGroup("Localized Text")]
    [DisplayAsString, HideIf("NoLocUsage")]
    [InfoBox("No localization is in place for name! Check for 'stat_[name]_usage' in the I2 languages prefab.", InfoMessageType.Warning, "NoLocUsage")]
    public string usageString = "no loc";

    bool NoLocUsage()
    {
        if (usageString == "no loc") return true;
        return false;
    }

    
    protected override void SetLoc()
    {
        base.SetLoc();
        usageString = LocalizedUsageString();
    }

    public virtual string LocalizedUsageString()
    {
        return Localization.GetFromLocLibrary(locPrefix + statName + "_usage", "no loc");
    }
    
    protected override void AddLoc()
    {
        base.AddLoc();
        Localization.AddToKeyLib(locPrefix + statName + "_usage", "does a rad thing to {0}!");
        SetLoc();
    }
}
