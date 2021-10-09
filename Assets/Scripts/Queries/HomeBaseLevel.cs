using UnityEngine;
using System.Collections.Generic;
using System;
using Diluvion.SaveLoad;

namespace Queries
{
    public enum Comparer { GreaterThan, LessThan, Equal }

    [CreateAssetMenu(fileName = "home base lvl query", menuName = "Diluvion/queries/home base level", order = 3)]
    public class HomeBaseLevel : Query
    {
        public Comparer comparison;
        [Space]
        public int reqLevel;

        [Tooltip("If true, the homeBase actual level must equal the cosmetic level for the query to return true")]
        public bool matchCosmeticLevel;

        /// <summary>
        /// Returns true if all the conversations listed have been read by the player.
        /// </summary>
        public override bool IsTrue(UnityEngine.Object o)
        {
            if (DSave.current == null) return false;

            if (matchCosmeticLevel)
            {
                if (DSave.current.homeBaseLevel != HomeBase.cosmeticLevel) return false;
            }
            
            int level = HomeBase.cosmeticLevel;

            if (comparison == Comparer.Equal && level == reqLevel)      return true;
            if (comparison == Comparer.GreaterThan && level > reqLevel) return true;
            if (comparison == Comparer.LessThan && level < reqLevel)    return true;

            return false;
        }
        

        protected override void Test()
        {
            Debug.Log(ToString() + ": " + IsTrue(null));
        }

        public override string ToString()
        {
            return "Home base level is " + comparison + " " + reqLevel;
        }
    }
}