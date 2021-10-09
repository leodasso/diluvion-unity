using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Diluvion.Achievements
{
    
    public delegate void ReturnedAchievementInfo();
    public interface ISpiderAchievements
    {
        bool Initialized { get;}
        IEnumerator InitAch();
        List<SpiderAchievement> AllAchievements();
        void PushAchievement(SpiderAchievement ach);
        void PushStat(SpiderAchievement ach);
        void ClearAchievement(SpiderAchievement ach);
        void ClearAchievements();
        void RefreshAchievements();
        event ReturnedAchievementInfo achievementChecked;
    }
}