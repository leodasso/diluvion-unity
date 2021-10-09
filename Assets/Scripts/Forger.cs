using System.Collections;
using System.Collections.Generic;
using Diluvion;
using Sirenix.OdinInspector;
using UnityEngine;
using DUI;

/// <summary>
/// Forger is a component that will generally be attached to characters who do forging. The player can interact with them 
/// to forge new upgrades for their ship, or to dismantle upgrades they've already installed. Forgers can't upgrade the
/// chassis of a ship; only ship-brokers can do that.
/// </summary>
public class Forger : MonoBehaviour
{
    [AssetsOnly] 
    public ForgerInfo forgerInfo;

    /// <summary>
    /// Creates the forge UI menu
    /// </summary>
    [Button]
    public void CreateUI()
    {
        SetDefaultForgerInfo();
        
        ForgerPanel newPanel = UIManager.Create(UIManager.Get().forgeMenu as ForgerPanel);
        newPanel.InitForgePanel(this);
    }

    /// <summary>
    /// Sets my forger info to the default one 'default forger info' in resources folder. note that this WILL NOT override
    /// an info that's already referenced. 
    /// </summary>
    public void SetDefaultForgerInfo()
    {
        if (forgerInfo == null)
        {
            forgerInfo = Resources.Load<ForgerInfo>("default forger info");
        }
    }
}
