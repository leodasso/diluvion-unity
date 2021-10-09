using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IRewardable {

    /// <summary>
    /// Attempts to make a reward with the given value. Returns all the reward points not used.
    /// </summary>
    float MakeReward (float value);

    /// <summary>
    /// Priority for the population. Lowest number gets populated first
    /// </summary>
    float PopulatePriority();
    
    void DisableIfEmpty();

    void Disable ();
}
