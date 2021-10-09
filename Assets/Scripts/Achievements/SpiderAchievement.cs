using Diluvion.SaveLoad;
using JetBrains.Annotations;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Diluvion.Achievements
{

    [System.Serializable]
    [CreateAssetMenu(fileName = "SpiderAchievementObject", menuName = "Diluvion/Achievement/Default")]
    public class SpiderAchievement : ScriptableObject
    {
        public string statID = "stat_default";
    
        public string niceName = "An Achievement";
        public string description = " You did it! You got the Achievement";
    
        public int progress = 0;
        public int goal = 1;
    
        public bool completed = false;

        public DSaveAchievement saveCheck;
        
    
        [SerializeField]
        public ISpiderAchievements achSystem;
    
        private ISpiderAchievements AchSystem
        {
            get
            {
                if (achSystem != null) return achSystem;
                achSystem = SpiderAchievementHandler.Get().CurrentAchievementSystem;
                return achSystem;
            }
        }
    
        /// <summary>
        /// Increases the progress of the achievment by amount
        /// </summary>
        /// <param name="amount"></param>
        /// <returns>Returns true if achievement is completed</returns>
        public bool IncreaseProgress(int amount = 1)
        {
            Debug.Log("Increasing progress on " + niceName + " by" + amount);
            return SetProgress(progress + amount);
        }
     
        /// <summary>
        /// Sets the progress of the achievment by amount
        /// </summary>
        /// <param name="amount"></param>
        /// <returns>Returns true if achievement is completed</returns>
        public bool SetProgress(int prog)
        {
            if (completed) return true; // if it already been completed, dont bother
            if (prog <= progress) return false;//if the inptut progress is less than the current progress, or the same, dont bother updating
            if (prog > goal) prog = goal; // if the prog is greater than the goal, clamp it
            progress = prog; // Set the new progress
            if (progress >= goal) // if the progress reached the goal, complete it
                completed = true;
            //Debug.Log("Setting progress on " + niceName + " to" + progress + "/" + goal + " Completed? : " + completed);
            AchSystem?.PushStat(this); // push to the achievement server
            return completed;
        }

        [Button]
        public void SaveCheckProgress()
        {
            if (!Application.isPlaying || DSave.current == null)
            {
                Debug.Log("Checking Editor Save for achievement");
                SetProgress(saveCheck.Progress(GameManager.Get().saveData));
            }
            else
            {   
                //Debug.Log("Checking DSAVE current for achievement");
                SetProgress(saveCheck.Progress(DSave.current));
            }
               
        }
        
        [Button]
        void IncrementProgressBy1()
        {
            IncreaseProgress();
        }
        
        [Button]
        public void SetComplete()
        {
            SetProgress(goal);
        }
    
        [Button]
        public void LocalReset()
        {
            completed = false;
            progress = 0;
        }
    
        [Button]
        public void ServerReset()
        {
            LocalReset();
            AchSystem.ClearAchievement(this);
        }
    
    }    

}