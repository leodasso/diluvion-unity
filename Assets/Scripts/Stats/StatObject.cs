using UnityEngine;
using System.Collections;
using SpiderWeb;
using Sirenix.OdinInspector;

[System.Serializable]
public class StatObject : ScriptableObject {

    [LabelText("Loc key")]
    [OnValueChanged("SetLoc", true)]
	public string statName;

    public Sprite statIcon;

    [BoxGroup("Localized Text")]
    [DisplayAsString, HideIf("NoLocName")]
    [InfoBox("No localization is in place for name! Check for 'stat_[name]' in the I2 languages prefab.", InfoMessageType.Warning, "NoLocName")]
    public string localizedName = "no loc";

    [DisplayAsString]
    [BoxGroup("Localized Text", order:1), HideIf("NoLocDescr")]
    [InfoBox("No localization is in place for description! Check for 'stat_[name]_descr' in the I2 languages prefab.", InfoMessageType.Warning, "NoLocDescr")]
    public string description = "does the things";

    [ColorPalette("Diluvion GUI"), HideLabel]
    public Color statColor = Color.red;

	protected const string locPrefix = "stat_";

    public bool IsEqualTo(StatObject o)
    {
        if (o.statName == statName) return true;
        else return false;     
    }

    [ButtonGroup("Loc", order: 2)]
    void AddToLoc()
    {
        AddLoc();
    }

    [ButtonGroup("Loc")]
    void Refresh()
    {
        SetLoc();
    }

	protected virtual void AddLoc() {
		Localization.AddToKeyLib(locPrefix + statName, statName);
        Localization.AddToKeyLib(locPrefix + statName + "_descr", "a totally rad description");
    }

    protected virtual void SetLoc()
    {
        localizedName = LocalizedStatName();
        description = LocDescription();
    }

    protected bool NoLocName()
    {
        if (localizedName == "no loc") return true;
        return false;
    }

    protected bool NoLocDescr()
    {
        if (description == "no loc") return true;
        return false;
    }

    public virtual string LocalizedStatName() {
		return Localization.GetFromLocLibrary(locPrefix + statName, "no loc");
	}

    public virtual string LocDescription()
    {
        return Localization.GetFromLocLibrary(locPrefix + statName + "_descr", "no loc");
    }
}
