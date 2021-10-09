using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DUI;

namespace Diluvion
{

    public class BattleLog
    {
        public string action;
        public string attackerName;
        public string hazardName;
        public float time = 3;

        public delegate void LogDelegate ();
        public LogDelegate onEnd;
        public LogDelegate onShow;

        public BattleLog (string actionString = "did an action", string attacker = "attacker", string hazard = "hazard", float t = 3)
        {
            action = actionString;
            attackerName = attacker;
            hazardName = hazard;
            time = t;
        }

        /// <summary>
        /// Returns the full formatted log.
        /// </summary>
        public string FullLog ()
        {
            action = action.Replace("[haz]", hazardName);
            action = action.Replace("[char]", attackerName);
            
            if (Hazard.subject)
                action = action.Replace("[subject]", Hazard.subject.GetLocalizedName());

            if (action.Contains("[rand]"))
            {
                Sailor randomSailor = PlayerManager.RandomBoardingPartyMember();
                action = action.Replace("[rand]", randomSailor.GetLocalizedName());
            }

            return action;
        }

        /// <summary>
        /// Runs the 'onShow' delegate
        /// </summary>
        public virtual void Show()
        {
            onShow?.Invoke();
        }

        /// <summary>
        /// Runs the 'onEnd' delegate
        /// </summary>
        public virtual void End ()
        {
            //Debug.Log(action + " log is ending.");
            if (onEnd != null) onEnd();
        }
    }
}