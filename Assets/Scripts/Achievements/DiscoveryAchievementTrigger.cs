using UnityEngine;
using System.Collections;
using Diluvion.Ships;
using Diluvion.Achievements;

public class DiscoveryAchievementTrigger : AchievementTrigger
{

    void OnTriggerEnter(Collider other)
    {
        if (!other.GetComponent<Bridge>()) return;
        if (!other.GetComponent<Bridge>().IsPlayer()) return;
        UpdateAch();
    }

}
