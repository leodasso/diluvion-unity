using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using DUI;

namespace Diluvion
{
    /// <summary>
    /// The base class for all actions hazards can take.
    /// </summary>
    public class HazardAction : Action
    {
        [MultiLineProperty(), LabelWidth(70)]
        public string attempt = "[haz] dashes forward to do an action!";
        [MultiLineProperty(), LabelWidth(70)]
        public string fail = "...but misses!";
        
        [MinMaxSlider(0, 1), Tooltip("Chance of this action succeeded. Min is min level, and max is max level.")]
        public Vector2 minMaxChance = new Vector2(.1f, .9f);

        //[MinValue(0), OnValueChanged("CalculateDanger")]
        //public float dangerFactor;

        //[ReadOnly, SerializeField, ShowInInspector]
        //protected float danger;

        /// <summary>
        /// Returns true if the roll to do this action with the given hazard container succeeded. Roll chance depends on 
        /// the hazard instance's level.
        /// </summary>
        /// <param name="hazard">The hazard container we're rolling for.</param>
        public bool RollSucceeded(HazardContainer hazard)
        {
            float chance = Hazard.StatForRange(minMaxChance.x, minMaxChance.y, 1, 25, hazard.instanceLevel); //hazard.hazard.StatForLevel(minMaxChance.x, minMaxChance.y, hazard.instanceLevel);
            return UnityEngine.Random.value <= chance;
        }

        public string LocAttempt()
        {
            return SpiderWeb.Localization.GetFromLocLibrary("hazAction_attempt_" + name, "[" + attempt + "]");
        }

        public string LocFail()
        {
            return SpiderWeb.Localization.GetFromLocLibrary("hazAction_fail_" + name, "[" + fail + "]");
        }

        public override string ToString ()
        {
            throw new NotImplementedException();
        }

        public override bool DoAction (UnityEngine.Object o)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Does the attack action for the given hazard and logs it to the battle panel.
        /// </summary>
        public virtual void DoAttack(Hazard hazard)
        {
            // Log this attack
            BattleLog newLog = new BattleLog(hazard.LocAttack(), hazard: hazard.LocName(), t: 5);
            BattlePanel.Log(newLog);
            newLog.onEnd += BattlePanel.Iterate;
            DoAction(null);
            BattlePanel.Shake(2, 1);
        }

        /// <summary>
        /// Called by hazard when the battle begins
        /// </summary>
        public virtual void BattleBeginPrep() {}

        protected override void Test ()
        {
            Debug.Log(ToString());
        }

        /*
        /// <summary>
        /// Returns the danger value for a theft
        /// </summary>
        /// <param name="minAmt">The min gold amount value of the theft</param>
        /// <param name="maxAmt">The max possible gold amount value of the theft</param>
        protected float TheftDanger(float minAmt, float maxAmt)
        {
            return ((minAmt + maxAmt) / 2f * dangerFactor) * ProbabilityFactor();
        }

        protected float ProbabilityFactor()
        {
            return (minMaxChance.x + minMaxChance.y) / 2f;
        }

        [Button]
        protected void CalculateDanger()
        {
            Danger();
        }

        public virtual float Danger()
        {
            danger = ProbabilityFactor() * dangerFactor;
            return danger;
        }
        */
    }
}