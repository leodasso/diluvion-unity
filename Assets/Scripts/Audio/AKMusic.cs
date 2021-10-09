using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using SpiderWeb;
using Diluvion;

[System.Serializable]
public class AKStateCall
{
    public int priority = 0;
    public List<Object> callers = new List<Object>();
    public string akGroup = "";
    public string akEvent = "";
    public uint akGroupInt;
    public uint akEventInt;

    public AKStateCall()
    {
        akGroup = "";
        akEvent = "";
    }

    public void AddCaller(Object caller)
    {
        if (!callers.Contains(caller))
            callers.Add(caller);
    }

    public void RemoveCaller(Object caller)
    {
        callers.Remove(caller);
    }

    public override string ToString()
    {
        return "StateCall Group: " + akGroup + " ("+akGroupInt+"), StateCall Event: " + akEvent + " (" + akEventInt +").";
    }
    
    public AKStateCall(string akg, string ake)
    {
        akGroup = akg;
        akEvent = ake;
    }
    
    
    public AKStateCall(string akg, string ake, int prio)
    {
        akGroup = akg;
        akEvent = ake;
        priority = prio;
    }


    public AKStateCall(uint akg, uint ake)
    {
        akGroupInt = akg;
        akEventInt = ake;
    }

    
    public AKStateCall(AkState state)
    {
        akGroupInt = (uint)state.groupID;
        akEventInt = (uint)state.valueID;
    }

    
    public void StateStats(AkState state)
    {
        //Debug.Log("Setting state stats to " + state.groupID + " and " + state.valueID);
        akGroupInt = (uint)state.groupID;
        akEventInt = (uint)state.valueID;
    }

    
    public bool HasSameStateCall(AKStateCall call)
    {
        if (call.akGroup != akGroup) return false;
        if (call.akGroupInt != akGroupInt) return false;
        if (call.akEvent != akEvent) return false;
        if (call.akEventInt != akEventInt) return false;
        //Debug.Log(call + " is equal to " + this);
        return true;
    }
}

public enum AdventureDifficulty
{
    Cleared,
    Neutral_Fjords,
    Neutral_Coast,
    Small,
    Medium,
    Heavy
}

public class AKMusic : MonoBehaviour
{
    public MusicSettingsObject musicSettings;

    [FoldoutGroup("Explorables"), OnValueChanged("SwitchExplorableIntensity")]
    public AdventureDifficulty explorableMusicSwitch;

    [FoldoutGroup("Player")]
    public AKStateCall mostRecentSet;
    [FoldoutGroup("Player")]
    public List<AKStateCall> currentEvents = new List<AKStateCall>();
    [FoldoutGroup("Player")]
    public AKStateCall oneShotSong;

    [Button]
    void TestAmbientMusic()
    {
        var ambient = new AKStateCall("Ingame_Music", "Fjords_Floatsam", 1);
        AddState(ambient, this);
    }

    void GetWwiseState()
    {
        uint stateID = 0;
        AkSoundEngine.GetState("OneShot", out stateID);
    }

    AKStateCall adventureStart;

    delegate void Callback();
 
    static AKMusic _instance;
    public static AKMusic Get()
    {
        if (_instance != null) return _instance;
        GameObject prefab = Resources.Load<GameObject>("akMusic");
        _instance = Instantiate(prefab).GetComponent<AKMusic>();
        DontDestroyOnLoad(_instance);
        return _instance;
    }

    public static bool Exists()
    {
        return _instance != null;
    }

    IEnumerator Start()
    {
        GameManager.LoadSoundBanks();
        adventureStart = new AKStateCall("Ingame_Music", "Explorable", musicSettings.explorablePriority);
        
        //DontDestroyOnLoad(gameObject);
        oneShotSong = null;
        yield return new WaitForEndOfFrame();
        SpiderSound.MakeSound("Play_Ingame_Music", gameObject);
    }

    void SwitchExplorableIntensity()
    {
        SpiderSound.SetSwitch("Adventure_Difficulty", explorableMusicSwitch.ToString(), gameObject);
    }

   
    [FoldoutGroup("Explorables"), Button]
    public void SetNeutralExporable()
    {
        if (GameManager.CurrentZone() == null)
        {
            Debug.LogError("Can't set adventure music, because not currently in a zone.");
            return;
        }
        SetAdventure(GameManager.CurrentZone().neutralExplorableTrack);
    }

    /// <summary>
    /// Sets the adventure if its not running, changes the switch if it is
    /// </summary>
    public void SetAdventure(AdventureDifficulty advDiff )
    {
        explorableMusicSwitch = advDiff;
        SwitchExplorableIntensity();
        AddState(adventureStart, this);
    }

    /// <summary>
    /// End Adventure
    /// </summary>
    [Button, FoldoutGroup("Explorables")]
    public void EndAdventure()
    {
        RemoveState(adventureStart, this, true);
    }


    #region States

    //ResetEvents
    public void ResetEvents()
    {
        currentEvents = new List<AKStateCall>();
    }
    
    //Adds a high-priority track that plays until RemoveOneSHotState is called (its outside the normal track adding/removal as only one can play at a time)
    public void AddOneShotState(AKStateCall skstate)
    {
        oneShotSong = skstate;
        SpiderSound.SetState(oneShotSong);
    }

    public void OneShotCallBack(object in_cookie, AkCallbackType in_type, object in_info)
    {
        Debug.Log("CalledBack Remove Oneshot" + in_info + " " + in_type + " " + in_cookie); 
        RemoveOneShotState();
    }

    [Button]
    public void RemoveOneShotState()
    {
        oneShotSong = null;
        SpiderSound.SetState("OneTime", "None");        
    }


    public bool AddState(AKStateCall akevent, Object newCaller)
    {
        if (akevent == null) return false;
     
        //Checks to see if any other of the same event have been added
        AKStateCall call = SameCallAsState(akevent);

        // Check if the call is already in the list
        if (call != null)
        {
            call.priority = akevent.priority;
            call.AddCaller(newCaller);
        }
        
        // If not, create a new one and add it to the list
        else
        {
            currentEvents.Add(akevent);
            akevent.AddCaller(newCaller);
        }

        SetLastState();
        return true;
    }
    
    //Adds Events 
    public void AddState(string eventGroup, string eventName, Object newCaller)
    {
        AddState(new AKStateCall(eventGroup, eventName), newCaller);
    }
    
    //Removes the currently playing event
    public void RemoveLastState()
    {
        int removeInt = currentEvents.Count - 1;
        if (currentEvents == null) return;
        if (removeInt > currentEvents.Count) return;
        RemoveState(GetCurrentEvent(), this);
    }

    [Button]
    public void ResetMusicPlaylist()
    {
        ResetEvents();
        SpiderSound.SetState("Ingame_Music", "None");
        SpiderSound.SetState("OneTime", "None");
    }


    /// <summary>
    /// Removes an event and sets the next one in line
    /// </summary>
    public void RemoveState(AKStateCall akevent, Object caller, bool removeAll = false)
    {
        if (akevent == null) return;
        if (currentEvents.Count < 1) return; // CAnt remove state if there are none
        
        AKStateCall call = SameCallAsState(akevent);
        
        if (call!=null)
        {
            call.RemoveCaller(caller);
            
            if (removeAll) call.callers.Clear();

            if (call.callers.Count < 1) currentEvents.Remove(call);

        }
        SetLastState();
    }

    public float lastStateSetTime = -99;

    
    /// <summary>
    /// Get next highest priority state, and sets spidersound to play that state.
    /// </summary>
    [Button]
    public void SetLastState()
    {      
        UpdateListOrder();
        mostRecentSet = GetCurrentEvent();
        lastStateSetTime = Time.unscaledTime;
        
        // If there's no music left, return to silence
        if (mostRecentSet == null)
        {
            SetSilence();
            return;
        }    
        SpiderSound.SetState(mostRecentSet);       
    }

    void SetSilence()
    {
        // TODO
    }


    public AKStateCall GetCurrentEvent()
    {
        if (currentEvents == null) return null;
        if (currentEvents.Count < 1) return null;
        return currentEvents.Last();
    }

    public bool SameStateCall(AKStateCall akevent)
    {
        if (GetCurrentEvent() == null) return false;
        if (!GetCurrentEvent().HasSameStateCall(akevent)) return false;
        return true;
    }

    public bool AlreadyHaveStateCall(AKStateCall akevent)
    {
        foreach (AKStateCall ak in currentEvents)
            if (ak.HasSameStateCall(akevent)) return true;
        return false;
    }

    public AKStateCall SameCallAsState(AKStateCall stateCall)
    {
        foreach (AKStateCall ak in currentEvents)
            if (ak.HasSameStateCall(stateCall)) return ak;
        return null;

    }

    //Makes sure we hit the correct priorityOrder
    public void OnRestart()
    {     
        currentEvents = new List<AKStateCall>();       
        RemoveOneShotState();
    }

    void OnDisable()
    {
        SpiderSound.MakeSound("Stop_Ingame_Music", gameObject);
    }

    /// <summary>
    /// Orders all the events by priority
    /// </summary>
    void UpdateListOrder()
    {
        currentEvents = currentEvents.OrderBy(asc =>asc.priority).ToList();
    }
    #endregion

}
