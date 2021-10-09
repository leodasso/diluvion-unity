using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AlignmentToPlayer
{
    Neutral,
    Friendly,
    Hostile
}

/// <summary>
/// For anything that can be aligned as hostile / friendly / neutral to the player. The results from this are only
/// relative to the player
/// </summary>
public interface IAlignable
{
    AlignmentToPlayer getAlignment();

    float SafeDistance();
}
