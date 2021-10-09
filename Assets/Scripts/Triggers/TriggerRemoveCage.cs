using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using HeavyDutyInspector;
using Diluvion.Ships;



public class TriggerRemoveCage : Trigger
{
    public float forceOfCageRemoval = 15f;

    public TweakBaseMaterial tbm;
    [Button("Sets up tubes with the proper setup", "DebugSetUpTubes", true)]
    public bool setRigidbodies;
    [Button("Explodes the Rigidbodies in said tubes", "ExplodeFromThis", true)]
    public bool explodeBodies;
    [Button("ResetsTubes", "ResetAllRigidbodies", true)]
    public bool resetBodies;



    public bool debug;
    List<FillingTube> allFillingTubes = new List<FillingTube>();


    void Awake()
    {
        GetTubes();
        SetUpTubes(false);
    }

    public void Glow()
    {
        Debug.Log("Starting Tweak");
        if (tbm != null)
            tbm.StartTweaking();

    }

    public void GetTubes()
    {
        foreach (FillingTube f in FindObjectsOfType<FillingTube>())
        {
            allFillingTubes.Add(f);
        }       
    }

    public void DebugSetUpTubes()
    {
        SetUpTubes(debug);
    }




    public void SetUpTubes(bool d)
    {
        foreach(FillingTube tb in allFillingTubes)
        {
            tb.SetupLists(d);
        }
    }

    public void ResetAllRigidbodies()
    {
        foreach (FillingTube f in allFillingTubes)
            f.Reset();
    }   

    public override void TriggerAction(Bridge otherBridge)
    {
        //ExplodeFromTransform(otherBridge.transform);
    }

    public void ExplodeFromThis()
    {
        ExplodeFromTransform(transform);

    }

    public void ExplodeFromTransform(Transform t)
    {
        foreach (FillingTube f in allFillingTubes)
            f.AddExplosiveForceFrom(t, forceOfCageRemoval);
    }

}
