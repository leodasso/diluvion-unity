using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using SpiderWeb;

namespace Diluvion
{
    /// <summary>
    /// A description of a hazard's reaction to a particular crew action or item usage.
    /// <see cref="Hazard"/>
    /// </summary>
    [CreateAssetMenu(fileName = "hazard reaction", menuName = "Diluvion/hazard reaction")]
    public class HazardReaction : ScriptableObject
    {
        [BoxGroup("", false)]
        public ReactionType reactionType = ReactionType.stat;

        [BoxGroup("", false)]
        [ShowIf("IsItem"), AssetsOnly]
        public List<Loot.DItem> items;

        [BoxGroup("", false)]
        [HideIf("IsItem"), AssetsOnly]
        public CrewStatObject stat;

        [MinValue(0)]
        [HideIf("IsItem")]
        [Tooltip("Higher defense means an attack of this type has a lower chance to hit.")]
        [Range(0, 10)]
        public int defense = 1;

        [Range(1, 10)]
        [Tooltip("If the hit is successful, how much damage will it do to the hazard?")]
        public int damage = 1;

        [HideLabel, MultiLineProperty(2)]
        [InfoBox("No loc found for this object. Check 'hazard_action_[name]' in the I2 Languages prefab.", InfoMessageType.Warning, "NoLoc")]
        public string attempt = "Crab avoids your punches.";

        [ MultiLineProperty(2), LabelWidth(70)]
        public string pass;

        [HideIf("IsItem")]
        [ MultiLineProperty(2), LabelWidth(70)]
        public string fail;

        public enum ReactionType { item, stat }

        string prefix = "hazard_action_";

        [ButtonGroup]
        void AddLoc()
        {
            Localization.AddToKeyLib(prefix + name, attempt);
            Localization.AddToKeyLib(prefix + name + "_pass", pass);
            Localization.AddToKeyLib(prefix + name + "_fail", fail);
        }

        [ButtonGroup]
        void Refresh()
        {
            //attempt = LocAttempt();
            //pass = LocPass();
            //fail = LocFail();
        }

        public string LocAttempt()
        {
            return Localization.GetFromLocLibrary(prefix + name, "[no loc]" + attempt, false);
        }

        public string LocPass()
        {
            return Localization.GetFromLocLibrary(prefix + name + "_pass", "[no loc]" + pass, false);
        }

        public string LocFail()
        {
            return Localization.GetFromLocLibrary(prefix + name + "_fail", "[no loc]" + fail, false);
        }

        bool NoLoc()
        {
            if (LocAttempt().Contains("[no loc]")) return true;
            return false;
        }

        bool IsItem()
        {
            if (reactionType == ReactionType.item) return true;
            return false;
        }

        /// <summary>
        /// The weakness of the reaction. 
        /// </summary>
        public float Weakness()
        {
            if (reactionType == ReactionType.item)
                return 1;

            // Get an estimate of defense b/t 0 and 1
            float def = Calc.LogBase(defense, 10, 1);
            float w = 1 - def;
            return w;
        }

        public override string ToString()
        {
            string s = "";

            s += "\n" + attempt + "\n";

            if (reactionType == ReactionType.item && items != null)
            {
                foreach (var i in items) 
                    s += "weakness to " + i.name + " for " + damage + " damage.";
            }
                

            if (reactionType == ReactionType.stat && stat != null)
            {
                s += "Defense against " + stat.LocalizedStatName() + ": {0}";
                s += "\n pass: " + LocPass();
                s += "\n fail: " + LocFail();
            }

            return s;
        }
    }
}