using UnityEngine;
using System.Collections.Generic;
using System;
using Loot;
using Queries;

namespace Quests
{

    /// <summary>
    /// An objective that can take a list of queries
    /// </summary>
    [CreateAssetMenu(fileName = "queries objective", menuName = "Diluvion/Quests/queries objective")]
    public class QueriesObjective : Objective
    {
        public List<Query> queriesToCheck = new List<Query>();

        /// <summary>
        /// Will set the objective complete if the player has more of the given items than required.
        /// </summary>
        public override void CheckObjective(DQuest forQuest)
        {
            foreach (Query q in queriesToCheck)
            {
                if (!q.IsTrue(null)) return;
            }

            ProgressObjective(forQuest);
        }

        public override GameObject CreateGUI(string overrideObjectiveName)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns items referenced in any of the queries I check
        /// </summary>
        public override List<StackedItem> ReferencedItems ()
        {
            List<StackedItem> items = new List<StackedItem>();

            foreach (Query q in queriesToCheck)
            {
                if (q.ReferencedItems() != null) items.AddRange(q.ReferencedItems());
            }

            return items;
        }
    }
}
