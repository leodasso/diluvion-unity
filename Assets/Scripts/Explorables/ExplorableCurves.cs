using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

/// <summary>
/// Asset to store the curve values for danger / value for explorables
/// </summary>
[CreateAssetMenu(fileName = "new explorable curves", menuName = "Diluvion/Explorable curves")]
public class ExplorableCurves : ScriptableObject
{

    [InlineButton("LogDanger")]
    public AnimationCurve danger;
    
    [InlineButton("LogValue")]
    public AnimationCurve value;

    [Tooltip("X axis is the normalized danger of the room (what percentage of the whole explorable's danger the room has) and" +
             " Y axis is the chance of hazard spawning.")]
    public AnimationCurve hazardChance;

    [Tooltip("X axis is the normalized gold value of the room, and Y axis is the chance of it spawning rewards.")]
    public AnimationCurve rewardChance;

    float CurveSum(AnimationCurve curve)
    {
        return CurveTools.IntegrateCurve(curve, 0, 1, 150);
    }

    /// <summary>
    /// Returns the total danger value. This is also the area under the danger curve between x=0 and x=1
    /// </summary>
    float DangerCurveSum()
    {
        return CurveSum(danger);
    }
    
    /// <summary>
    /// Returns the total gold value. This is also the area under the gold value curve between x=0 and x=1
    /// </summary>
    float GoldValueCurveSum()
    {
        return CurveSum(value);
    }


    float ValueOfRoom(int roomIndex, int totalRooms, AnimationCurve curve)
    {
        float segmentSize = 1.0f / totalRooms;
        float start = roomIndex * segmentSize;

        return CurveTools.IntegrateCurve(curve, start, start + segmentSize, 50);
    }

    /// <summary>
    /// Between 0 and 1, returns how much of the area of the total curve the given room's segment represents.
    /// </summary>
    /// <param name="totalRooms">total number of rooms in the chain</param>
    float NormalizedValueOfRoom(int roomIndex, int totalRooms, AnimationCurve curve)
    {
        return ValueOfRoom(roomIndex, totalRooms, curve) / CurveSum(curve);
    }

    /// <summary>
    /// Returns the gold value of the room at the given index.
    /// </summary>
    /// <param name="totalRooms">The number of rooms in the room chain.</param>
    public float GoldValueOfRoom(int roomIndex, int totalRooms)
    {
        return ValueOfRoom(roomIndex, totalRooms, value);
    }

    /// <summary>
    /// Finds the area on the curve relating to the given room. Then returns that area's percentage of the whole  curve,
    /// as a value between 0 and 1.
    /// </summary>
    public float GoldValueOfRoomNormalized(int roomIndex, int totalRooms)
    {
        return NormalizedValueOfRoom(roomIndex, totalRooms, value);
    }
    
    /// <summary>
    /// Finds the area on the curve relating to the given room. Then returns that area's percentage of the whole  curve,
    /// as a value between 0 and 1.
    /// </summary>
    public float DangerOfRoomNormalized(int roomIndex, int totalRooms)
    {
        return NormalizedValueOfRoom(roomIndex, totalRooms, danger);
    }

    /// <summary>
    /// Returns the danger value of the room at the given index.
    /// </summary>
    /// <param name="totalRooms">The number of rooms in the room chain.</param>
    public float DangerOfRoom(int roomIndex, int totalRooms)
    {
        return ValueOfRoom(roomIndex, totalRooms, danger);
    }


    void LogDanger()
    {
        LogCurve(danger, "danger curve");
    }

    void LogValue()
    {
        LogCurve(value, "value curve");
    }

    void LogCurve(AnimationCurve curve, string curveName)
    {
        long calcTime;
        float curveArea = CurveTools.IntegrateCurve(curve, 0, 1, 500, out calcTime);
        Debug.Log(curveName + " has a total derivative of " + curveArea + " calculated in " + calcTime + " ticks.");
    }
}
