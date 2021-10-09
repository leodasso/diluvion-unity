using UnityEngine;
using System.Collections;
using HeavyDutyInspector;
using Diluvion;

public class ShortestDistanceToMyPoints : MonoBehaviour
{
    public float maxDistance = 800;
    public float minDistance = 100;   
    public Transform target;
    [SerializeField]
    [ReorderableArray(false)]
    Transform[] distancePoints;
    public bool assumePlayerTarget = false;
    float distanceNormalized;


    /// <summary>
    /// Safe target getter with functionality to automatically set player if we want
    /// </summary>
    /// <returns></returns>
    public Transform Target()
    {
        if (target != null) return target;
        if (assumePlayerTarget)
            target = PlayerManager.PlayerTransform();     
        return target;
    }

    public Transform[] Points()
    {
        if (distancePoints == null) return null;
        if( distancePoints.Length<1)return null;
        return distancePoints;
    }

    /// <summary>
    /// Overload assumes player;
    /// </summary> 
    public float DistanceFromClosestPoint()
    {
        return DistanceFromClosestPoint(Target());
    }

    /// <summary>
    /// Returns the distance to the target from the closest Point in DistancePoints
    /// </summary> 
    public float DistanceFromClosestPoint(Transform tr)
    {
        target = tr;
        if (Target() == null) return maxDistance;

        Vector3 smallestVector = new Vector3(999, 999, 999);
        foreach (Transform t in distancePoints)
        {
            if (t == null) continue;
            Vector3 distanceVector = t.position - Target().position;
            if (distanceVector.sqrMagnitude < smallestVector.sqrMagnitude)
                smallestVector = distanceVector;
        }
        //Debug.DrawRay(target.transform.position, smallestVector, Color.green, 0.01f);

        float returnDistance = smallestVector.magnitude;
        return returnDistance;
    }

    /// <summary>
    /// Returns the normalized distance between min and max 
    /// </summary> 
    public float DistanceFromClosestPointNormalized()
    {
        distanceNormalized = Mathf.LerpUnclamped(0.0f, 1.0f, (Mathf.Clamp(DistanceFromClosestPoint(), minDistance, maxDistance) - minDistance) * 1.0f / (maxDistance - minDistance) * 1.0f);
        return distanceNormalized;
    }

    /// <summary>
    /// Returns the normalized distance between min and max 
    /// </summary> 
    public float DistanceFromClosestPointNormalized(float distance)
    {
        distanceNormalized = Mathf.LerpUnclamped(0.0f, 1.0f, (Mathf.Clamp(distance, minDistance, maxDistance) - minDistance) * 1.0f / (maxDistance - minDistance) * 1.0f);
        return distanceNormalized;
    }
}
