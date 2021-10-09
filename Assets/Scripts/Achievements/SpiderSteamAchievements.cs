using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;
//using NodeCanvas.DialogueTrees;
using Steamworks;
using Sirenix.OdinInspector;


namespace Diluvion.Achievements
{
    

    
    public class SpiderSteamAchievements : MonoBehaviour, ISpiderAchievements
    {
        CGameID gameID;
        protected Callback<UserAchievementStored_t> m_UserAchievementStored;
        protected Callback<UserStatsStored_t> m_UserStatsStored;
        protected Callback<UserStatsReceived_t> m_UserStatsReceived;
    
        private bool m_bStoreStats;
        // Did we get the stats from Steam?
        private bool m_bRequestedStats;
        
        private bool m_bStatsValid;
        
        public event ReturnedAchievementInfo achievementChecked;

        public bool Initialized => SteamManager.Initialized;

        public IEnumerator InitAch()
        {
            while (!Initialized) yield return new WaitForSeconds(0.5f);
            gameID = new CGameID(SteamUtils.GetAppID());
            m_UserStatsReceived = Callback<UserStatsReceived_t>.Create(OnUserStatsReceived);
            m_UserAchievementStored = Callback<UserAchievementStored_t>.Create(OnAchievementStored);
            m_UserStatsStored = Callback<UserStatsStored_t>.Create(OnUserStatsStored);
            Debug.Log("STEAM INITIALIZED");
        }

        [Button]
        public void ReadAchievementProgress()
        {
            RefreshAchievements();
        }
        
        
    
        //Returns a copy list of the achievement templates
        public List<SpiderAchievement> AllAchievements()
        {
            return SpiderAchievementHandler.Get().AllAchievements();
        }
    
        public void RefreshAchievements()
        {
            m_bRequestedStats = false;
        }
    
        
        public void PushAchievement(SpiderAchievement ach)
        {
            if (!Initialized)
                return;
    
            Debug.Log("Pushing " + ach.name + " to steam");
            SteamUserStats.SetAchievement(ach.name);
            m_bStoreStats = true;
        }

        public void PushStat(SpiderAchievement ach)
        {

            if (!Initialized)
            {
                Debug.Log("STEAM Not Initialized!");
                return;
            }

            Debug.Log("Pushing " + ach.statID + " of value" + ach.progress +"/" +ach.goal + " to steam");
    
            //Debug.Log("Pushing " + ach.statID + " of value" + value +  " to steam");
            SteamUserStats.SetStat(ach.statID, ach.progress);
            m_bStoreStats = true;
        }
        
        public void ClearAchievement(SpiderAchievement ach)
        {
            
            if (!Initialized)
                return;
    
            Debug.Log("Clearing " + ach.statID);
            SteamUserStats.ClearAchievement(ach.name);
            m_bStoreStats = true;
        }
    
        public void ClearAchievements()
        {
            if (!Initialized)
                return;
            Debug.Log("Clearing " + AllAchievements().Count + "  achievements");
            foreach (SpiderAchievement sa in AllAchievements())
            {
                SteamUserStats.ClearAchievement(sa.name);
            }
            m_bStoreStats = true;
        }
    
    
        void Update()
        {
            if (!Initialized)
                return;
            if (!m_bRequestedStats)
            {                
                bool Ssuccess = SteamUserStats.RequestCurrentStats();
    
                // This function should only return false if we weren't logged in, and we already checked that.
                // But handle it being false again anyway, just ask again later.
                m_bRequestedStats = Ssuccess;
            }
    
            //If we have changed stats or changed Achievements we set these values to true, which streamlines the push and get calls from steam servers
            if (!m_bStatsValid) return;
    
            if (!m_bStoreStats) return;
    
            bool bSuccess = SteamUserStats.StoreStats();
            //if push was not successful, we wil try again on the next push
            m_bStoreStats = !bSuccess;
        }
    
        #region steam Callbacks
    
        void CheckAchievements()
        {
            string statString = "Steam Stats: \n";
            foreach(SpiderAchievement sa in AllAchievements())
            {
                m_bStatsValid = true;
                //Check if we have the stat
                bool ret = SteamUserStats.GetAchievement(sa.name, out sa.completed);
                if (ret)
                {
                    //We only get stats if the the achievement requires one
                    if (!string.IsNullOrEmpty(sa.statID))
                        if(SteamUserStats.GetStat(sa.statID, out sa.progress))
                            statString += "Stat: <color=yellow>" + sa.statID + "</color> = <color=green>" + sa.progress + "</color>.\n";
                }
                else
                {
                    Debug.LogWarning("SteamUserStats.GetAchievement failed for Achievement " + sa.name + "\nIs it registered in the Steam Partner site?");
                }
            }
//            Debug.Log(statString);
        }
    
        private void OnUserStatsReceived(UserStatsReceived_t pCallback)
        {
            if (!SteamManager.Initialized)
                return;
    
            // we may get callbacks for other games' stats arriving, ignore them
            if ((ulong)gameID == pCallback.m_nGameID)
            {
                if (EResult.k_EResultOK == pCallback.m_eResult)
                {
                    Debug.Log("Received stats and achievements from Steam\n");
                    CheckAchievements();
                    achievementChecked?.Invoke();
                }
                else
                {
                    Debug.Log("RequestStats - failed, " + pCallback.m_eResult);
                }
            }
        }
    
        private void OnUserStatsStored(UserStatsStored_t pCallback)
        {
            // we may get callbacks for other games' stats arriving, ignore them
            if ((ulong)gameID == pCallback.m_nGameID)
            {
                if (EResult.k_EResultOK == pCallback.m_eResult)
                {
                    Debug.Log("StoreStats - success");
                }
                else if (EResult.k_EResultInvalidParam == pCallback.m_eResult)
                {
                    // One or more stats we set broke a constraint. They've been reverted,
                    // and we should re-iterate the values now to keep in sync.
                    Debug.Log("StoreStats - some failed to validate");
                    // Fake up a callback here so that we re-load the values.
                    UserStatsReceived_t callback = new UserStatsReceived_t();
                    callback.m_eResult = EResult.k_EResultOK;
                    callback.m_nGameID = (ulong)gameID;
                    OnUserStatsReceived(callback);
                }
                else
                {
                    Debug.Log("StoreStats - failed, " + pCallback.m_eResult);
                }
            }
        }
    
    
        private void OnAchievementStored(UserAchievementStored_t pCallback)
        {
            // We may get callbacks for other games' stats arriving, ignore them
            if ((ulong)gameID == pCallback.m_nGameID)
            {
                if (0 == pCallback.m_nMaxProgress)
                {
                    Debug.Log("Achievement '" + pCallback.m_rgchAchievementName + "' unlocked!");
                }
                else
                {
                    Debug.Log("Achievement '" + pCallback.m_rgchAchievementName + "' progress callback, (" + pCallback.m_nCurProgress + "," + pCallback.m_nMaxProgress + ")");
                }
            }
        }
        #endregion
    }
}