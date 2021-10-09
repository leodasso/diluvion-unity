using UnityEngine;
using System.Collections;
using Sirenix.OdinInspector;
using System.Collections.Generic;



namespace Diluvion.Achievements
{
    
    
    public enum PublishingTo { Steam, iStore, GOG, NONE };
    public class SpiderAchievementHandler : MonoBehaviour
    {
    
      
        public PublishingTo publishingTo = PublishingTo.Steam;
        List<SpiderAchievement> allAchievements = null;
    
        static SpiderAchievementHandler handler = null;
        
        [SerializeField]
        ISpiderAchievements achievementSystem = null;
        public ReturnedAchievementInfo checkedAchievement;
    
        public static SpiderAchievementHandler Get()
        {
            if (handler == null) handler = FindObjectOfType<SpiderAchievementHandler>();
            if (handler == null) handler = new GameObject("AchievementManager").AddComponent<SpiderAchievementHandler>();
       
            return handler;
        }
    
        public List<SpiderAchievement> AllAchievements()
        {
            if (allAchievements == null) allAchievements = new List<SpiderAchievement>();
            if (allAchievements.Count > 0) return allAchievements;
            foreach (SpiderAchievement sao in Resources.LoadAll<SpiderAchievement>("AchievementObjects/"))
            {
                allAchievements.Add(sao);
            }
            return allAchievements;
        }
        
        public ISpiderAchievements CurrentAchievementSystem
        {
            get
            {
                if (achievementSystem != null) return achievementSystem;
                SetAchievementSystem();
                return achievementSystem;
            }
        }

        public bool Initialized()
        {
            return CurrentAchievementSystem.Initialized;
        }
        
       // Use this for initialization
        void Awake()
        {
            DontDestroyOnLoad(gameObject);
            StartCoroutine(CurrentAchievementSystem?.InitAch());
        }
        
    
        void SetAchievementSystem()
        {
            publishingTo = GameManager.BuildSettings().platform;
            switch (publishingTo)
            {
                case PublishingTo.Steam:
                {
                    SetupForSteam();
                    break;
                }
                case PublishingTo.iStore:
                {
                    SetupForMac();
                    break;
                }
                case PublishingTo.GOG:
                {
                    SetupForGOG();
                    break;
                }
                default:
                {
                    return;
                }
            }
        
            if (achievementSystem == null) return;
            achievementSystem.achievementChecked += CheckedAchievement;
        }
    
      
        
        
        public void CheckedAchievement()
        {
            checkedAchievement?.Invoke();
        }
    
    
        public void RefreshAchievements()
        {
            CurrentAchievementSystem.RefreshAchievements();
    
        }
    
        void SetupForSteam()
        {     
            achievementSystem = gameObject.AddComponent<SpiderSteamAchievements>();
        }
    
        void SetupForMac()
        {
            Debug.LogError("I STORE NOT IMPLEMENTED");
        }
    
        void SetupForGOG()
        {
            achievementSystem = gameObject.AddComponent<SpiderGogAchievements>();
        }
    
        public SpiderAchievement GetAchievement(string achName)
        {
            foreach (SpiderAchievement sa in AllAchievements())
            {
                if (sa.name == achName)
                    return sa;
            }
            
            return null;
        }
    
    
        void PushAchievement(SpiderAchievement ach)
        {
            if (ach == null) return;
            if (achievementSystem == null) return;
            achievementSystem.PushAchievement(ach);//Will this steam achievement stay in scope until its recieved?
        }
    
    
        void PushStat(SpiderAchievement ach)
        {
            achievementSystem?.PushStat(ach);
        }
    
    
        public void ClearAchievements()
        {
            achievementSystem.ClearAchievements();
        }
    
    
        public void ClearAchievement(SpiderAchievement ach)
        {
            achievementSystem.ClearAchievement(ach);
    
        }
    
        #region AchivementCheckers
    
    
    
        //String name overload for achievmeent templates
        public void SetAchievement(string achName, int amount)
        {
            SpiderAchievement savedAch = GetAchievement(achName);
            if (savedAch == null) return;
      
            savedAch.SetProgress(amount);
        }
        #endregion
    }

}
