using UnityEngine;
using System.Collections;
using Diluvion.Ships;

namespace Diluvion
{
    /// <summary>
    /// Officers can't be randomly generated, don't have stats, but they are the only ones
    /// who can operate stations.
    /// </summary>
    public class Officer : Character
    {

        /// <summary>
        /// Attempt to join the station as an officer. If the station is for a different skill,
        /// already has an officer, or doesn't require an officer, uses the character.joinStation function.
        /// </summary>
        public override void JoinStation(Station st)
        {
            if (st.CanUseOfficer(this))
            {
                parentStation = st;

                //Debug.Log(name + " joining station " + st.name + " as an officer.");
                SetAnimation("officer", 0, true);
                st.EnableStation(this);
                if (st.officerSpot)
                    st.officerSpot.FinalizePlacement(this);
            }
            else base.JoinStation(st);
        }

        /// <summary>
        /// Leaves the given station.
        /// </summary>
        public override void LeaveStation(Station st)
        {
            if (!st) return;

            if (st.officer == this)
            {
                st.DisableStation();
            }

            base.LeaveStation(st);
        }

        protected override bool CanJoinStation(Station st)
        {
            if (st.CanUseOfficer(this)) return true;
            return base.CanJoinStation(st);
        }

        protected override string JoinStationInfo (Station st)
        {
            if (st.CanUseOfficer(this))
            {
                string s = "{0} will activate {1} and act as officer.";
                return string.Format(s, GetLocalizedName(), st.LocalizedName());
            }

            else return base.JoinStationInfo(st);
        }

        protected override string LeaveStationInfo (Station st)
        {
            if (st.CanUseOfficer(this))
            {
                string s = "{1} will be disabled as {0} leaves. Any sailors working in {1} will also return to the Crew Quarters.";
                return string.Format(s, GetLocalizedName(), st.LocalizedName());
            }

            else return base.LeaveStationInfo(st);
        }

    }
}