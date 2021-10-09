using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class EscortTest : MonoBehaviour
{
    public Transform target;

    [SerializeField]
    private Vector3 targetPosition;

    
    [Button]
    void GetEscortPosition()
    {
        targetPosition = target.GetComponentInChildren<Escortee>().WorldEscortPoint(transform);
        Debug.DrawLine(transform.position, targetPosition, Color.red, 0.1f);
    }


}
