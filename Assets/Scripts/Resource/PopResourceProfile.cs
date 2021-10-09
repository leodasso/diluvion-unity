using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace Diluvion.Roll
{
    [CreateAssetMenu(fileName = "default popResourceProfile", menuName = "Diluvion/RollTables/PopResourceProfile")]
    public class PopResourceProfile : ScriptableObject
    {
        [MinMaxSlider(0, 1), Tooltip("As a percentage of total available resources, how much should be spent per explorable. Uses a random range between these two percentages.")]
        public Vector2 minAndMaxValuePerPlacer;

        public int valuePerTick = 1500;
        
        public float spawnChance = 30;

        public float spawnRate = 60;


        public int shipLimit = 10;
        
        public int usedPlacerLimit = 20;


        [Tooltip("The max, and the starting amount.")]
        public PopResources maxRes = new PopResources( 30000, 300);

    }
}
