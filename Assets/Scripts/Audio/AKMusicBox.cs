using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using SpiderWeb;
using Diluvion.Ships;

[System.Serializable]
public struct RTPCPair
{
    public string RTPCToSet;
    public float RTPCValue;
}


[RequireComponent(typeof(AkState))]
public class AKMusicBox : Trigger
{
    [HideIf("dontChangeState")]
    public AkState eventToCall;
    [HideIf("dontChangeState")]
    public AKStateCall eventToTrigger;

    [ToggleLeft]
    public bool dontChangeState;
    [Tooltip("Option to set global RTPC to make things more intense"), ToggleLeft]
    public bool setRtpc;

    [Tooltip("Option to go back an RTPC level to tone things down"), ToggleLeft]
    public bool revertRTPC;

    [ShowIf("setRtpc")]
    public List<RTPCPair> rtpcsToSet;
    
    [ToggleLeft]
    public bool oneShot;
    [ToggleLeft]
    public bool onlyRemoveOnDisable;

    object cookie;


    //CurrentState
    public AkState CurrentState()
    {
        if (eventToCall != null) return eventToCall;
        eventToCall = GetComponent<AkState>();
        return eventToCall;
    }

    public void Awake()
    {
        eventToTrigger.StateStats(CurrentState());
    }

    public override void TriggerAction(Bridge otherBridge)
    {
        base.TriggerAction(otherBridge);
        StartCoroutine(ChangeState(true));
    }

    public override void TriggerExitAction(Bridge otherBridge)
    {
        Debug.Log(otherBridge.name + " exited " + name, gameObject);
        base.TriggerExitAction(otherBridge);

        if (!oneShot)
            StartCoroutine(ChangeState(false));
    }

    IEnumerator ChangeState(bool add)
    {
        yield return new WaitForSeconds(0.2f);

        if (setRtpc && add)
        {
            foreach (RTPCPair rtp in rtpcsToSet)
                SpiderSound.TweakRTPC(rtp.RTPCToSet, rtp.RTPCValue, null);
        }

        if (dontChangeState) yield break;
        if (CurrentState() == null) yield break;

        // Removing state
        if (!add)
        {
            if (!onlyRemoveOnDisable) RemoveState();
            yield break;
        }

        // Adding one shot
        if (oneShot)
        {
            PlayOneShot();
        }
        
        // Adding state
        else
        {
            if (!AKMusic.Get().AddState(eventToTrigger, this))
                Debug.LogError(name + " couldn't add state to music!", gameObject);
        }  
    }

    [Button]
    void PlayOneShot()
    {
        AKMusic.Get().AddOneShotState(eventToTrigger);
        AkSoundEngine.PostEvent("Play_OneShot", gameObject, (uint) AkCallbackType.AK_MusicSyncExit,
            AKMusic.Get().OneShotCallBack, cookie);
        TurnOffAllColliders();
    }

    [Button]
    void RemoveOneshot()
    {
        AKMusic.Get().RemoveOneShotState();
    }
    
    void RemoveState()
    {
        if (dontChangeState) return; 
       
        AKMusic.Get().RemoveState(eventToTrigger, this);
        
    }


    void OnDisable()
    {
        RemoveState();
    } 


    void TurnOffAllColliders()
    {
        if (GetComponent<Collider>())
            GetComponent<Collider>().enabled = false;
        foreach (Collider c in GetComponentsInChildren<Collider>())
            c.enabled = false;
    }
}
