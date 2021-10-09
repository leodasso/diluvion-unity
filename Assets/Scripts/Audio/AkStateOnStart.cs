using UnityEngine;
using System.Collections;
using Sirenix.OdinInspector;

[RequireComponent(typeof(AkState))]
public class AkStateOnStart : MonoBehaviour {

    #region public fields
    public enum StartupCall { Awake, Start, Spawned, Enable};

    [Tooltip("Choose when we ask for a state switch")]
    public StartupCall startMethod = StartupCall.Start;

    [Tooltip("Clears the music list and sets the input song as the only one"), ToggleLeft]
    public bool resetMusicList;

    [Tooltip("Will delay the switch by delay seconds"), ToggleLeft]
    public bool timed;
    [ShowIf("timed")]
    public float delay = 1;
    [Tooltip("Will revert this state back to whatever it was before this came into play"), ToggleLeft]
    public bool revertStateOnDestroy = true;

    [Tooltip("If true, deleting this will stop any oneshot music."), ToggleLeft]
    public bool oneShotState;

    [Tooltip("Set the priority here.")]
    public AKStateCall myCall;
    #endregion

    #region private fields
    bool _alreadyRemoved;
    AkState _myState;
 
    #endregion

    AKStateCall MyCall
    {
        get
        {
            if (myCall != null) return myCall;
            _myState = GetComponent<AkState>();
            myCall.StateStats(_myState);
            return myCall;
        }
    }

    #region StartupCalls
    //Init
    void CacheCall()
    {
        _myState = GetComponent<AkState>();
        myCall.StateStats(_myState);
    }

    void OnEnable()
    {
        if (startMethod == StartupCall.Enable)
            SwitchEvent();
    }
    //Init choices
    void Awake()
    {      
        CacheCall();
        if (startMethod == StartupCall.Awake)
            SwitchEvent();
    }

	void Start () {
        if (startMethod == StartupCall.Start)
            SwitchEvent();
    }

    void OnSpawned()
    {
        if (startMethod == StartupCall.Spawned)
            SwitchEvent();
    }

    #endregion

    #region Methods
    //Sets the event in queue
    void SwitchEvent()
    {
        if(_alreadyRemoved)
            _alreadyRemoved = false;

        if (resetMusicList)
            AKMusic.Get().ResetMusicPlaylist();

        if (!timed)
            AKMusic.Get().AddState(MyCall, this);
        else
            StartCoroutine(DelayEventSwitch());
     
    }

    void RemoveEvent()
    {
        if (!Application.isPlaying) return;
        if (_alreadyRemoved) return;
        _alreadyRemoved = true;
        if (oneShotState)
            AKMusic.Get().RemoveOneShotState();
        
        AKMusic.Get().RemoveState(MyCall, this);
    }


    IEnumerator DelayEventSwitch()
    {
        yield return new WaitForSeconds(delay);       
        AKMusic.Get().AddState(MyCall, this);
      
    }
    #endregion

    #region endingCalls
    //Kill calls
    public void OnDestroy()
    {
        if(revertStateOnDestroy)
            RemoveEvent();
    }

    public void OnDisable()
    {
        if (revertStateOnDestroy)
            RemoveEvent();
    }

    public void OnDespawned()
    {
        if (revertStateOnDestroy)
            RemoveEvent();
    }

    #endregion
}



