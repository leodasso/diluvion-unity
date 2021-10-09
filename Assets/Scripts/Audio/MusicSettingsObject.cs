using UnityEngine;

[CreateAssetMenu(fileName = "new music settings", menuName = "Diluvion/New Music Settings")]
public class MusicSettingsObject : ScriptableObject
{
    [Tooltip("Max range to the closest enemy that the combat music will play")]
    public float combatMaxRange = 200;

    [Tooltip("The rate at which the nearest combatant value increases")]
    public float combatCooldownSpeed = 5;

    [Tooltip("If no creature attacks occur for this long, type switch switches from creature to ship. ")]
    public float creatureCooldown = 10;
    
    
    [Tooltip("The range where the regular low-key combat song switches to high-intensity")]
    public float intenseCombatRange = 50;
    
    [Space]
    public int combatPriority = 10;
    public int combatEndPriority = 11;
    public int explorablePriority = 12;

    [Space] 
    [Tooltip("Any hazard with a danger level below this will play the lowest intensity battle music.")]
    public int tierOneBattle = 30;
    
    [Tooltip("Any hazard with a danger level above this will play the high intensity battle music.")]
    public int tierTwoBattle = 100;




}