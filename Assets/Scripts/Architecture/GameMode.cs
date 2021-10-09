using System.Collections.Generic;
using UnityEngine;
using Diluvion.SaveLoad;
using Sirenix.OdinInspector;

namespace Diluvion {

    [CreateAssetMenu(fileName = "new game mode", menuName = "Diluvion/Game/game mode", order = 0)]
    public class GameMode : ScriptableObject {

        #region general settings
        [FoldoutGroup("General settings")]
        public GameZone defaultZone;

        [FoldoutGroup("General settings")]
        public DebugSave startingSave;

        [FoldoutGroup("General settings")]
        [Tooltip("For hitboxes that load cosmetic scenes, the player must be out of the hitbox this long before unloading the scene." +
                 "This is to prevent repeat load / unload of a scene when player is meandering around the border of a loading zone.")]
        public float cosmeticSceneCushionTime = 5;
        
        //TODO Yagni Make the achievement system not need monobehaviours, change from update to event system
     
        [Space] [FoldoutGroup("General settings")] 
        [Tooltip("The max possible for officers. (Level represents how many sailors they can have in their station)")]
        public int maxOfficerLevel = 4;
        
        [FoldoutGroup("General settings")]
        [Tooltip("A critical hit will be multiplied by this number ")]
        public float criticalHit = 3;

        [FoldoutGroup("General settings")]
        public float reSpawnTime = 5;

        [FoldoutGroup("General settings")]
        [Tooltip("How close you need to be to an objective before the GUI waypoint shows up")]
        public float waypointVisibleDistance = 50;

        [FoldoutGroup("General settings")]
        [Tooltip("Percentage of materials you get back from dismantling an upgrade chunk")]
        public float dismantleReturns = .75f;

        
        #endregion
        
        #region difficulty

        [FoldoutGroup("difficulty")]
        public float shipEnemyDifficulty = 1;

        [FoldoutGroup("difficulty")]
        public float creatureEnemyDifficulty = 1;

        [FoldoutGroup("difficulty"), Tooltip("Amount of time a character is out of commision once they've been injured")]
        public float crewInjuryCooldown = 60;
        
        [Space]
        [FoldoutGroup("difficulty")]
        [Tooltip("When calculating the danger of a ship, each mount on the ship will count for this much danger")]
        public int dangerPerMount = 10;
        
        [FoldoutGroup("difficulty")]
        [Tooltip("When calculating the danger of a ship, each station slot on the ship will count for this much danger")]
        public int dangerPerStationSlot = 20;
        
        [FoldoutGroup("Balance values")]
        [Tooltip("When calculating the danger of a ship, each bonus slot on the ship will count for this much danger")]
        public int dangerPerBonusSlot = 5;
        
        [FoldoutGroup("difficulty")]
        [Tooltip("When calculating the danger of a ship, multiplies the summed danger of all the modules.")]
        public int moduleDangerMultiplier = 1;

        [FoldoutGroup("difficulty"), Space]
        public float hazardDangerCostMultiplier = 1;
        
        [FoldoutGroup("difficulty")]
        [Tooltip("The hazard level is applied this power before multiplying by the base danger.")]
        public float hazardDangerPower = 1.2f;

        [FoldoutGroup("difficulty"), Tooltip("The base amount of gold a lv 1 hazard uses to generate rewards")]
        public int baseHazardReward = 100;
        [FoldoutGroup("difficulty"), Tooltip("Used as an exponent to hazard level to get amount of gold a hazard uses to generate rewards. " +
                                             "for example, a level 5 hazard will have 5^[value] * baseHazardReward")]
        public float hazardRewardPower = 1.2f;
        

        [FoldoutGroup("difficulty")] 
        [Tooltip("How close the aim must be to a target's lead position to fix aim to the lead.")]
        public float leadHelpRadius = .5f;
        
        #endregion

        #region rewards
        
        [FoldoutGroup("Rewards")] 
        [Tooltip("Per room, each reward spawned divides the chance of another reward being spawned by this much.")]
        public float rewardRatio = 3;

        [FoldoutGroup("Rewards")] [Tooltip("The base gold cost of a sailor with no points.")]
        public int sailorBaseCost = 100;
        
        [FoldoutGroup("Rewards")]
        [Tooltip("The value cost (gold) of one stat point in sailor generation.")]
        public int costPerSailorPoint = 60;

        [FoldoutGroup("Rewards")] 
        [Tooltip("The exponent by which cost per point increases. Increasing this makes point 8, 9, n much more expensive than point 1, 2, 3")]
        public float sailorPointPower = 1.5f;
        
        [FoldoutGroup("Rewards")]
        [Tooltip("The minimum number of stat points that can be alloted to a sailor during generation. Sailors " +
                 "with less reward points than this won't be spawned.")]
        public int minPointsForSailor = 6;

        [FoldoutGroup("Rewards"), Button] 
        void PreviewSailorCosts()
        {
            for (int i = minPointsForSailor; i < minPointsForSailor + 20; i++)
            {
                Debug.Log("cost of sailor with " + i + "stat points: " + Sailor.CostOfSailor(i));
            }
        }
        
        #endregion

        [MinValue(0), MaxValue(9999)]
        [FoldoutGroup("Balance values")]
        [Tooltip("Adjusts the amount of force added to a rigidbody that's impacted by a Munition / explosion")]
        public float impactForceMultiplier = 1;
    }
}