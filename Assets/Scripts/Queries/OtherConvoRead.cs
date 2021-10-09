using UnityEngine;
using System.Collections.Generic;
using System;
using Diluvion;
using Diluvion.SaveLoad;

namespace Queries
{
    [CreateAssetMenu(fileName = "other convo read query", menuName = "Diluvion/queries/other convo read", order = 2)]
    public class OtherConvoRead : Query
    {
        public enum ReadStatus { beenRead, notBeenRead}

        [Space]
        public List<Convo> otherConvos = new List<Convo>();
        public ReadStatus readStatus;

        /// <summary>
        /// Returns true if all the conversations listed have been read by the player.
        /// </summary>
        public override bool IsTrue(UnityEngine.Object o)
        {
            if (DSave.current == null) return false;

            GameObject GO = GetGameObject(o);
            if (o == null) return false;

            Character crew = GO.GetComponent<Character>();
            if (crew == null) return false;

            // Check each convo. If they haven't yet been shown, return false.
            foreach (Convo c in otherConvos)
            {
                if (DSave.current.ShownDialogKey(c.locKey, crew) && readStatus == ReadStatus.notBeenRead) return false;
                if (!DSave.current.ShownDialogKey(c.locKey, crew) && readStatus == ReadStatus.beenRead) return false;
            }
            return true;
        }

        protected override void Test()
        {
            Debug.Log(ToString() + IsTrue(null).ToString());
        }

        public override string ToString()
        {
            string s = "";
            foreach (Convo c in otherConvos)
            {
                s += c.titleText + "/n";
            }

            s += "Have been read ";
            return s;
        }
    }
}