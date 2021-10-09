using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Diluvion;
using Diluvion.Ships;

namespace Queries
{

    [CreateAssetMenu(fileName = "sailor count query", menuName = "Diluvion/queries/sailor count", order = 1)]
    public class SailorCount : Query
    {
        public int minSailors = 1;
        public int maxSailors = 999;

        public override bool IsTrue (Object o)
        {
            CrewManager c = PlayerManager.PlayerCrew();
            if (!c) return false;

            int sailors = c.TotalSailors();

            return (sailors >= minSailors && sailors <= maxSailors);
        }

        public override string ToString ()
        {
            string s = "Query is true if player has between " + minSailors + " and " + maxSailors + " sailors.";
            return s;
        }
    }
}