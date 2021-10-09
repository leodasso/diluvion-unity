using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Diluvion;
using UnityEngine;

public class Escortee : MonoBehaviour
{
    
    
    
    [SerializeField]
    List<Transform> _escortPoints = new List<Transform>();
    
    private List<Transform> EscortPoints
    {
        get
        {
            if (_escortPoints != null && _escortPoints.Count > 0) return _escortPoints;
            _escortPoints = new List<Transform>(GetComponentsInChildren<Transform>(false));
            _escortPoints.Remove(transform);
            return _escortPoints;
        }
    }


    [SerializeField] private bool showPoints = false;

    
    private void OnDrawGizmos()
    {
#if UNITY_EDITOR
       // Handles.ArrowHandleCap(11,transform.forward, transform.rotation,2,EventType.Ignore);
        Gizmos.color = Color.green;
        foreach (Transform t in EscortPoints)
        {
            if (t == null) continue;
            if (t == transform) continue;
            Gizmos.DrawLine(transform.position,t.position);
            UnityEditor.Handles.ArrowHandleCap(0,t.position, transform.rotation,2,EventType.Repaint);
        }
#endif
    }


    private Dictionary<Transform, Transform> EscortSlots = new Dictionary<Transform, Transform>();

    /// <summary>
    /// Local Coordinates of an escort point paired to the input escorter
    /// </summary>
    /// <param name="escorter"></param>
    /// <returns></returns>
    public Vector3 LocalEscortPoint(Transform escorter)
    {
        Transform point = EscortPointTransform(escorter);
        if (point == null) return Vector3.zero;
        return point.localPosition;
    }

    /// <summary>
    /// World coordinates for a escort point paired to the input escorter
    /// </summary>
    /// <param name="escorter"></param>
    /// <returns></returns>
    public Vector3 WorldEscortPoint(Transform escorter)
    {
        Transform point = EscortPointTransform(escorter);
        if (point == null) return transform.position;
        return point.position;
    }

    /// <summary>
    /// A free escort point paired to the input escorter
    /// </summary>
    /// <param name="escorter"></param>
    /// <returns></returns>
    public Transform EscortPointTransform(Transform escorter)
    {
        if (escorter == null) return null;
        
        if (EscortSlots.ContainsKey(escorter))
            return EscortSlots[escorter];
        else
        {
            Transform freePoint = FreeEscortPoint(escorter);
            if (freePoint== null) return null;
            Hull h = escorter.GetComponent<Hull>();
            if(h)
                h.onKilled += RemoveMe;
            EscortSlots.Add(escorter,freePoint);
            
            return freePoint;
        }
    }

    //Checks for a free escort point
    Transform FreeEscortPoint(Transform escorter)
    {
        List<Transform> sortedEscortPoints = EscortPoints.OrderBy(t => Vector3.Distance(t.position,escorter.position)).ToList();
        foreach (Transform t in sortedEscortPoints)
        {
            if (t == transform) continue;
            if (EscortSlots.ContainsValue(t)) continue;
            return t;
        }
        return null;
    }

    //Callback for escort point
    void RemoveMe(GameObject escorter)
    {
        if (escorter != null) return;
        Hull h = escorter.GetComponent<Hull>();
        if(h)
            h.onKilled -= RemoveMe;

        EscortSlots.Remove(escorter.transform);
    }
    
    
    
    

}
