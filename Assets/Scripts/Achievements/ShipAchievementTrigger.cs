using UnityEngine;
using System.Collections;
using Diluvion;
using Diluvion.SaveLoad;
using Diluvion.Ships;

namespace Diluvion.Achievements
{
    
    public class ShipAchievementTrigger : AchievementTrigger
    {
      
        Bridge bridge;
        Hull hull;
    
        protected Bridge HasBridge()
        {
            if (bridge != null) return bridge;
            bridge = GetComponent<Bridge>();
            return bridge;
        }
    
        protected Hull MyHull()
        {
            if (hull != null) return hull;
            hull = GetComponent<Hull>();
            return hull;
        }
    
        protected bool IsOnPlayer()
        {
            if (HasBridge() == null) return false;
            return HasBridge().IsPlayer();
        }

        public override void UpdateAch()
        {
            base.UpdateAch();
            if (DSave.current == null) return;
            DSave.current.shipKills++;
        }
      
    
       public override IEnumerator Start()
        {
             yield return base.Start();
       
          
            if (MyHull())
            {
                MyHull().defeatedByPlayer += UpdateAch;
                //Debug.Log("Stored Non Player Ship Achievement Trigger",gameObject);
            }
            
        }	
    }

}