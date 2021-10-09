using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(menuName = "Diluvion/forger info")]
public class ForgerInfo : ScriptableObject
{
    [AssetsOnly] public List<Forging> possibleUpgrades = new List<Forging>();

}
