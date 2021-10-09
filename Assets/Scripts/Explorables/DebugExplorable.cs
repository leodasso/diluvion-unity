using UnityEngine;
using HeavyDutyInspector;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;
using System.Collections.Generic;

[ExecuteInEditMode]
public class DebugExplorable : MonoBehaviour
{
    public GameObject prefabRef;
    public Explorable instanceRef;

    [Button("Reset", "Reset", true)]
    public bool resetTest;

    public void InitDebugExplorable(Explorable instance)
    {
        instanceRef = instance;
        prefabRef = instance.prefabRef;
        //transform.SetParent(instance.transform);
    }

    public void Activate(bool acitve = true)
    {
        instanceRef.gameObject.SetActive(acitve);
    }

    //For resetting and hiding
    public void ResetAndDeactivate()
    {
        Reset();
        Activate(false);
    }
   
    //
    public void Reset()
    {
        if (!prefabRef) return;
        gameObject.isStatic = false;
        GameObject go = (GameObject)Instantiate(prefabRef, transform.position, transform.rotation);
        go.transform.SetParent(instanceRef.transform.parent);
        Explorable ex = go.GetComponent<Explorable>();
        //transform.SetParent(ex.transform);
        ex.debugEx = this;
        ex.prefabRef = prefabRef;
        if (!Application.isPlaying)
            DestroyImmediate(instanceRef.gameObject);
        else
            Destroy(instanceRef.gameObject);

        instanceRef = ex;
        gameObject.isStatic = true;
    }   
  

	
}
