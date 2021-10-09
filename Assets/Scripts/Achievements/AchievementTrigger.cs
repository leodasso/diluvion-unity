using UnityEngine;
using System.Collections;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

public enum AchType
{
    Complete,
    Increment
}

namespace Diluvion.Achievements
{
    public class AchievementTrigger : MonoBehaviour
    {
        public AchType achType = AchType.Complete;
        public SpiderAchievement objectTochange;
        public int incrementAmount = 1;
        public bool onStart = false;
        private ISpiderAchievements achievementTarget;
    
    
        public virtual IEnumerator Start()
        {
            Debug.Log("Starting AchievementTrigger: " + this.name, gameObject);
            while (!SpiderAchievementHandler.Get().Initialized())
                yield return new WaitForSeconds(0.5f);
          
            Debug.Log("Achievement System done initializing on " + this.name, gameObject);
            if (onStart)
                UpdateAch();
        }
    
        public virtual void UpdateAch()
        {
            if (objectTochange == null) return;
            Debug.Log("Updating Achievement " + achType.ToString() + " " + objectTochange.name);
            if (achType == AchType.Complete)
                Complete();
            else
                Increment();
        }
    
        public void Complete()
        {
            if (objectTochange == null) return;
           objectTochange.SetComplete();
        }
        
        public void Increment()
        {
            if (objectTochange == null) return;
            if (!SpiderAchievementHandler.Get()) return;
            Debug.Log("Incrementing Achievement: " + objectTochange + " by " + incrementAmount);
            objectTochange.IncreaseProgress(incrementAmount);
        }
    }
}