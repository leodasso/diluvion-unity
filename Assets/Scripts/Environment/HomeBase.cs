using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SpiderWeb;
using Sirenix.OdinInspector;
using Diluvion;
using Diluvion.SaveLoad;
using Diluvion.Achievements;


/// <summary>
/// Spawns homebase appropriate to the saved level
/// </summary>
public class HomeBase : MonoBehaviour
{
    /// <summary>
    /// The level of the home base's appearance. This differs from the true level in save file- this level gets set after all the animation and
    /// such is complete, so dialogue related to home base can be more logical
    /// </summary>
    public static int cosmeticLevel;
    
    public SpiderAchievement achievement;
    Animator _anim;

    public void UpdateCosmeticLevel()
    {
        cosmeticLevel = HomeBaseLevel();
    }

    int HomeBaseLevel()
    {
        if (DSave.current == null) return 0;
        return DSave.current.homeBaseLevel;
    }

    void Start()
    {
        _anim = GetComponent<Animator>();
        StartCoroutine(SpeedySetup());
    }

    /// <summary>
    /// Speeds up the animator to get to the current level, then sets animator speed back to 1.
    /// </summary>
    IEnumerator SpeedySetup()
    {

        while (DSave.current == null)
        {
            yield return new WaitForSeconds(2);
        }
        
        _anim.speed = 100;
        _anim.SetInteger("Level", HomeBaseLevel());
        yield return new WaitForSeconds(2);
        _anim.speed = 1;
    }


    [Button]
    public void UpgradeToCurrentLevel()
    {
        UpgradeBaseCosmetics(HomeBaseLevel());
    }

    [Button]
    public void PlayConstructionAudio()
    {
        SpiderSound.MakeSound("Play_Construct_Base", gameObject);
    }


    /// <summary>
    /// Spawns the base of the given level.  If the player doesn't have a home base
    /// yet, then the base level will be 0 and nothing will happen.
    /// </summary>
    void UpgradeBaseCosmetics(int baseLevel)
    {
        //If base level is less than 1, then player hasn't gotten a home base yet.
        if (baseLevel < 1) return;
        achievement.SetProgress(baseLevel);
        _anim.SetInteger("Level", baseLevel);      
    }
}