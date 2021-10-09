using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Galaxy;
using Galaxy.Api;

namespace Diluvion.Achievements
{
    

    /// <summary>
    /// Achievement translator from spider to GOG
    /// </summary>
    public class SpiderGogAchievements : MonoBehaviour, ISpiderAchievements
    {
    
        public event ReturnedAchievementInfo achievementChecked;
    
        bool updateToServer = false;
        GalaxyManager gmanager;
        GalaxyManager GM()
        {
            if (gmanager != null) return gmanager;
            gmanager = FindObjectOfType<GalaxyManager>();
            if (gmanager==null)
                gmanager = gameObject.AddComponent<GalaxyManager>();
            return gmanager;
        }

        public bool Initialized => GM().IsSignedIn;

        public IEnumerator InitAch()
        {
            while(!Initialized)
                yield return new WaitForSeconds(0.5f);
            Debug.Log("GALAXY INITIALIZED");
        }  
    
        /// <summary>
        /// Pushes Stats to Server at the end of the frame
        /// </summary>
        void Update()
        {
            if (!Initialized) return;
            if (!updateToServer) return;
    
            RefreshAchievements(); // THIS WAS MISSING JACOB YOU FUCK -Jacob
        }
    
        public List<SpiderAchievement> AllAchievements()
        {
            return SpiderAchievementHandler.Get().AllAchievements();
        }
    
        /// <summary>
        /// Saves the achievement as complete to the local Galaxymanager and pushes it to the server
        /// </summary>
        /// <param name="ach"></param>
        public void PushAchievement(SpiderAchievement ach)
        {
            if (!Initialized) return;
            if (GM().GetAchievement(ach.name)) return;
            ach.SetComplete();
            GM().SetAchievement(ach.name);
            updateToServer = true;
          //  RefreshAchievements(); 
        }
    
        /// <summary>
        /// Processes and pushes the stat to the local GalaxyManager
        /// </summary>
        /// <param name="ach"></param>
        /// <param name="value"></param>
        public void PushStat(SpiderAchievement ach)
        {
           // Debug.Log(GM().IsSignedIn + " Signed IN?");
            if (!Initialized) return;
            if (GM().GetAchievement(ach.name)) return;
            //Debug.Log("GOG is signed in");
           // if (GM().GetAchievement(ach.id)) return;
           // Debug.Log("Achievement not completed");
            int currentInt = ach.progress;
            Debug.Log("Pushing GOG stat: " + ach.statID + " value + : " + currentInt);
            if (currentInt >= ach.goal)
            {
                PushAchievement(ach);
            }
            else
            {
                SaveStatInt(currentInt, ach.statID);
            }
        }
    
        //Saves the stat to the manager and calls the manager to update the stats to server
        public void SaveStatInt(int currentint, string statId)
        {
            GM().SetStatInt(currentint, statId);
            updateToServer = true;
            //   RefreshAchievements(); // TODO if this refreshes too often, use the update loop instead
        }
    
        public void ClearAchievement(SpiderAchievement ach)
        {
            if (!Initialized) return;
            GM().ClearAchievement(ach.name);
        }
    
    
        public void ClearAchievements()
        {
            if (!Initialized) return;
            GM().ResetStatsAndAchievements();
        }
    
        public void RefreshAchievements()
        {
            GM().StoreStatsAndAchievements();
            updateToServer = false;
          
        }
    }
}