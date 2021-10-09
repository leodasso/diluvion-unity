using UnityEngine;
using System.Collections;
using HeavyDutyInspector;

public class TextHelper : MonoBehaviour
{
    public string newLayer;
    public int newSortingOrder = 1;
    [Button("Set Sorting", "Set", true)] public bool hidden1;

    void Set()
    {
        GetComponent<Renderer>().sortingLayerName = newLayer;
        GetComponent<Renderer>().sortingOrder = newSortingOrder;
        //Debug.Log(GetComponent<Renderer>().sortingLayerName);
    }
}