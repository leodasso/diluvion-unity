using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using System.Collections.Generic;
using SpiderWeb;
using Queries;
using Quests;
using Diluvion;
using Diluvion.SaveLoad;
using Sirenix.OdinInspector;
#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "New conversation", menuName = "Diluvion/Characters/convo")]
public class Convo : ScriptableObject {

    #region activation
    [ToggleLeft, FoldoutGroup("Activation", false), Tooltip("Inactive convo won't ever appear in game.")]
    public bool active = true;      
    
    [ToggleLeft, FoldoutGroup("Activation")]
    [Tooltip("The conversation stays as an active button. Useful for merchants or anyone you talk to multiple times.")]
    public bool dontLog;

    [ToggleLeft, FoldoutGroup("Activation")]
    [Tooltip("As long as the character isn't on the player crew, this will remain a normal conversation. Once they're on the" +
             " player crew, it becomes a log.")]
    public bool dontLogUntilHired;
    
    [ToggleLeft, FoldoutGroup("Activation")]
    [Tooltip("This convo wont show up until manually added by an outside thing.")]
    public bool manualAdd;                              

    [ToggleLeft, FoldoutGroup("Activation"), LabelText("Text trigger only")]
    [Tooltip("When true, this convo can only appear once triggered")]
    public bool triggerOnly;                

    [ FoldoutGroup("Activation"), LabelText("Hashtag triggers")]
    public string textTriggers;             

    [ FoldoutGroup("Activation"), Space, AssetsOnly]
    public DQuest triggeringQuest;
    
    [ FoldoutGroup("Activation"), ShowIf("HasQuest")]
    public QuestStatus triggeringQuestStatus = QuestStatus.InProgress;

    [ FoldoutGroup("Activation"), ShowIf("ShowObjective"), AssetList(CustomFilterMethod = "PartOfQuest"), AssetsOnly]
    public Objective triggeringObjective;

    [ FoldoutGroup("Activation"), ShowIf("ShowObjectiveStatus")]
    public QuestStatus triggeringObjectiveStatus = QuestStatus.InProgress;
    
    [ FoldoutGroup("Activation"), Space, Tooltip("Lower numbers appear higher up in dialog window")]
    public float priority = 1;
    
    [FoldoutGroup("Activation"), AssetsOnly]
    public List<Convo> readTheseFirst = new List<Convo>();              // This convo won't appear until these are read first.

    [FoldoutGroup("Activation"), AssetsOnly]
    public List<Query> conversationQueries = new List<Query>();         // Must all be true before this convo will show up

    [FoldoutGroup("Activation"), ShowIf("HasQueries")]
    public bool invertQueries;
    
    #endregion
    
    #region text
    
    [FoldoutGroup("Text")]
    public bool showChatter = true;

    [FoldoutGroup("Text"), Tooltip("If true, the chatter will only show once per playthrough.")]
    public bool chatterShowsOnce;
    
    [Title("Title / Radio Chatter"), HideLabel]
    [FoldoutGroup("Text"), Multiline(2)]
    [Tooltip("This text is also what shows up in the radio chatter.")]
    public string titleText;          

    [FoldoutGroup("Text"), ReadOnly, Multiline(15), HideLabel]
    public string previewText;
    
    [FoldoutGroup("Text")]
    public bool omitFromLoc;


    [FoldoutGroup("Text"), ShowIf("showChatter")]
    public bool showFullConvoInRadio;

    [ShowIf("showChatter"), FoldoutGroup("Text")]
    public bool chatterEveryTime;

    [ShowIf("showChatter"), FoldoutGroup("Text")]
    [Tooltip("How long (seconds) the radio must wait between showing this chatter element.")]
    public float interval = 300;
    
    [FoldoutGroup("Text")]
    public string question;                                     // what is captain asking?

    [FoldoutGroup("Text")]
    public List<Speech> speeches = new List<Speech>();          // list of speeches
    
    #endregion

    #region onconvofinished
    [ FoldoutGroup("On convo finished"), AssetsOnly]
    public DQuest questToStart;
    
    [ FoldoutGroup("On convo finished"), AssetsOnly]
    public List<Action> postConvoActions = new List<Action>();

    [FoldoutGroup("On convo finished"), DrawWithUnity] public UnityEvent onConvoComplete;
    
    #endregion


    [HideIf("omitFromLoc"), ReadOnly, PropertyOrder(599)]
    [Tooltip("Localization key. Used to generate keys for all parts of this conversation.")]
    public string locKey;               

    
    /// <summary>
    /// a dictionary to pair convos with the time (Time.unscaledTime) they were most recently shown
    /// </summary>
    static Dictionary<Convo, float> timesConvosShown = new Dictionary<Convo, float>();

    public static void ResetConvoTimes()
    {
        timesConvosShown.Clear();
    }
    
    bool PartOfQuest(Objective o)
    {
        if (!triggeringQuest) return false;
        return triggeringQuest.HasObjective(o);
    }

    bool HasQueries()
    {
        if (conversationQueries == null) return false;
        if (conversationQueries.Count < 1) return false;
        return true;
    }

    /// <summary>
    /// Has it been more than 'interval' seconds since the last time I've been shown?
    /// </summary>
    public bool IntervalOkay()
    {
        float lastTimeShown;
        
        // Check if this convo has been shown recently 
        if (!timesConvosShown.TryGetValue(this, out lastTimeShown))
            lastTimeShown = -999;
        
        // Check if the last time shown is greater than the allowed interval
        float timeSince = Time.unscaledTime - lastTimeShown;
        if (timeSince < interval) return false;


        // Update the memorized time last shown
        if (timesConvosShown.ContainsKey(this))
            timesConvosShown[this] = Time.unscaledTime;

        else
        {
            timesConvosShown.Add(this, Time.unscaledTime);
        }
        return true;
    }

    /// <summary>
    /// If this chatter has already been shown and is only supposed to show once, then return false. Otherwise true.
    /// </summary>
    public bool OkayToShowChatter()
    {
        return !chatterShowsOnce || !ChatterHasShown();
    }

    public bool ChatterHasShown()
    {
        if (DSave.current == null) return false;
        return DSave.current.ShownChatterKey(this);
    }

    public void SaveChatterShown()
    {
        DSave.current.AddChatterKey(this);
    }

    #if UNITY_EDITOR

    [ButtonGroup("Loc")]
    void ClearLocKey()
    {
        Undo.RecordObject(this, "clear loc key");
        locKey = "";
        EditorUtility.SetDirty(this);
    }
    
    [Button, FoldoutGroup("Text")]
    void RefreshPreview()
    {
        previewText = "";
        if (showChatter) previewText += "Chatter: " + LocalizedChatter() + "\n";
        previewText += "Player: " + LocalizedQuestion() + "\n";
        foreach (Speech s in speeches)
        {
            previewText += "\n";
            if (s.speaker == null) previewText += "Me: ";
            else previewText += s.speaker.GetLocalizedName() + ": ";
            previewText += s.LocalizedText();
        }
        
        EditorUtility.SetDirty(this);
    }
    #endif

    /// <summary>
    /// Returns true if this conversation will give a quest
    /// </summary>
    public bool GivesQuest()
    {
        if (questToStart != null) return true;

        foreach (Action a in postConvoActions)
        {
            if (a == null) continue;
            if (a.GivesQuest()) return true;
        }

        return false;
    }

    bool HasQuest()
    {
        if (triggeringQuest != null) return true;
        else return false;
    }

    bool ShowObjective()
    {
        if (!HasQuest()) return false;
        if (triggeringQuestStatus == QuestStatus.InProgress || triggeringQuestStatus == QuestStatus.NotFinished) return true;
        return false;
    }

    bool ShowObjectiveStatus()
    {
        if (!ShowObjective()) return false;
        if (triggeringObjective != null) return true;
        return false;
    }

    #region loc
    /// <summary>
    /// Pulls the terms for this conversation from the i2 source
    /// </summary>
    [ButtonGroup("Loc", 600)]
    public void PullTerms()
    {
        if (Application.isPlaying)
        {
            Debug.Log("Can't localize while playing.");
            return;
        }

        if (!active) return;
        if (omitFromLoc) return;

        if (showChatter) titleText = LocalizedChatter();
        if (NeedsQuestionText()) question = LocalizedQuestion();

        foreach (Speech speech in speeches)
        {
            speech.text = speech.LocalizedText();
        }
    }

    [ButtonGroup("Loc")]
    public void AddTerms()
    {
        if (!active || omitFromLoc)
        {
            RemoveTerms();
            return;
        }

        // Chatter
        string chatterKey = locKey + Localization.key_Chatter;

        if (!showChatter)
            Localization.RemoveTermFromLib(chatterKey);
        else
            Localization.AddToKeyLib(chatterKey, titleText);

        // Question
        string questionKey = locKey + Localization.key_Question;
        if (string.IsNullOrEmpty(question) || speeches.Count < 1)
            Localization.RemoveTermFromLib(questionKey);
        else
            Localization.AddToKeyLib(questionKey, question);

        // speeches
        foreach (Speech speech in speeches)
            Localization.AddToKeyLib(speech.locKey, speech.text);
    }

    /// <summary>
    /// Removes terms from the i2 loc library.
    /// </summary>
    public void RemoveTerms()
    {
        // Chatter
        string chatterKey = locKey + Localization.key_Chatter;
        Localization.RemoveTermFromLib(chatterKey);

        // Question
        string questionKey = locKey + Localization.key_Question;
        Localization.RemoveTermFromLib(questionKey);

        // speeches
        foreach (Speech speech in speeches)
            Localization.RemoveTermFromLib(speech.locKey);
    }


    /// <summary>
    /// Verifies all the terms of this conversation with the i2 Loc library.
    /// </summary>
    [ButtonGroup("Loc")]
    public void TestTerms()
    {
        if (!active || omitFromLoc)
        {
            Debug.Log("Conversation " + locKey + " isn't active. Skipping.");
            return;
        }

        if (showChatter)
        {
            Debug.Log("Testing..." + locKey + Localization.key_Chatter);
            LocalizedChatter();
        }

        if (NeedsQuestionText())
        {
            Debug.Log("Testing..." + locKey + Localization.key_Question);
            LocalizedQuestion();
        }

        foreach (Speech sp in speeches)
        {
            Debug.Log("Testing..." + sp.locKey);
            sp.LocalizedText();
        }
    }

    /// <summary>
    /// Returns localized chatter / title
    /// </summary>
    public string LocalizedChatter()
    {
        return Localization.GetFromLocLibrary(locKey + Localization.key_Chatter, titleText);
    }

    /// <summary>
    /// Returns the localized player question
    /// </summary>
    public string LocalizedQuestion()
    {
        return Localization.GetFromLocLibrary(locKey + Localization.key_Question, question);
    }


    #endregion

    /// <summary>
    /// Returns true if there's chatter text.
    /// </summary>
    public bool ShowsChatter()
    {
        if (!showChatter) return false;
        return !string.IsNullOrEmpty(titleText);
    }

    /// <summary>
    /// Does this conversation have the given trigger?
    /// </summary>
    public bool HasTrigger(string trigger)
    {
        foreach (string s in MyTriggers())
            if (s == trigger) return true;

        return false;
    }

    /// <summary>
    /// Returns my triggers as a list of strings
    /// </summary>
    public List<string> MyTriggers()
    {
        List<string> returnList = new List<string>();
        returnList.AddRange(textTriggers.Split(' '));
        return returnList;
    }

    /// <summary>
    /// Checks if this conversation is valid for the given crew.
    /// </summary>
    public bool IsValid(Dialogue d)
    {
        if (!active) return false;

        foreach (Convo c in readTheseFirst)
            if (!c.BeenRead()) return false;

        if (!ValidQuestStatus()) return false;

        // Check for trigger only convos
        if (!TriggerMatch(d)) return false;

        if (!QueriesPassed(d)) return false;

        return true;
    }

    /// <summary>
    /// Checks all queries on this convo. If they all good, returns true.
    /// </summary>
    public bool QueriesPassed(Dialogue d)
    {
        if (!invertQueries)
        {
            foreach (Query q in conversationQueries)
                if (!q.IsTrue(d.MyCharacter())) return false;
        }else
        {
            foreach(Query q in conversationQueries)
                if (q.IsTrue(d.MyCharacter())) return false;
        }
        return true;
    }

    /// <summary>
    /// Is this quest valid based on the current save file quest status?
    /// </summary>
    bool ValidQuestStatus()
    {
        if (!triggeringQuest) return true;
        
        // Check if the triggering quest has started
        if (!triggeringQuest.IsOfStatus(triggeringQuestStatus)) return false;

        // Check if the triggering objective is true
        if (triggeringObjective)
        {
            if (!triggeringObjective.IsOfStatus(triggeringObjectiveStatus, triggeringQuest)) return false;
        }

        return true;
    }

    /// <summary>
    /// Checks if this convo would have likely been read in an older save file.
    /// </summary>
    public void CheckSavedStatus(string charName)
    {
        //Debug.Log("Checking " + name + "...", this);

        if (!triggeringQuest) {
            Debug.Log("<color=yellow>...It isn't triggered by any quests.</color>");
            return;
        }

        if (!triggeringQuest.IsBeyondStatus(triggeringQuestStatus)) {
            Debug.Log("<color=yellow>...Triggering quest " + triggeringQuest.name + " isn't beyond " + triggeringQuestStatus + ".</color>");
            return;
        }

        if (triggeringObjective)
        {
            if ( !triggeringObjective.IsBeyondStatus(triggeringObjectiveStatus, triggeringQuest))
            {
                Debug.Log("<color=yellow>...Triggering objective " + triggeringObjective.name + " isn't beyond " + triggeringObjectiveStatus + ".</color>");
                return;
            }
        }

        Debug.Log(name + " has likely been read. <color=green>Setting 'read' status in file.</color>");
        SaveAsRead(charName);
    }

    /// <summary>
    /// Checks if the given dialogue has any of my triggers. If I am not triggerOnly, always returns true.
    /// </summary>
    bool TriggerMatch(Dialogue d)
    {
        if (!triggerOnly) return true;
        foreach (string s in MyTriggers())
            if (d.HasTrigger(s)) return true;

        foreach (Convo c in d.manualAdded)
            if (c == this) return true;

        return false;
    }

    public bool NeedsDialogBox()
    {
        if (speeches.Count > 0) return true;
        return false;
    }

    /// <summary>
    /// Has this been read by the player?
    /// </summary>
     public bool BeenRead(Character forCrew)
    {
        if (dontLog) return false;

        if (dontLogUntilHired)
        {
            if (!PlayerManager.PlayerCrew()) return false;
            return PlayerManager.PlayerCrew().HasCrewmember(forCrew);
        }
        
        if (DSave.current == null) return false;
        return DSave.current.ShownDialogKey(name, forCrew);
    }

    /// <summary>
    /// Has this particular conversation taken place with anyone
    /// </summary>
    public bool BeenRead()
    {
        if (DSave.current == null) return false;
        return DSave.current.ShownDialogKey(name);
    }

    /// <summary>
    /// Call this when the dialog has been read. Will do the post-read actions as well. (i.e. start quest, open shop)
    /// </summary>
    public void SetAsRead(Character forCrew)
    {
        //Debug.Log("Player has read " + forCrew.name + "'s convo " + ToString());
        PostReadActions(forCrew);
        if (DSave.current == null) return;
        DSave.current.AddDialogKey(this, forCrew);
        QuestManager.Tick();
    }

    void SaveAsRead(string charName)
    {
        if (DSave.current == null) return;
        DSave.current.AddDialogKey(this, charName);
    }

    /// <summary>
    /// Stuff that happens once this conversation is complete.
    /// </summary>
    void PostReadActions(Character forCrew)
    {
        foreach (Action a in postConvoActions)
        {
            if (a == null) continue;
            a.DoAction(forCrew);
        }

        if (questToStart) questToStart.StartQuest();
        
        onConvoComplete.Invoke();
    }

    bool NeedsQuestionText()
    {
        if (!string.IsNullOrEmpty(question) && speeches.Count > 0) return true;
        return false;
    }

    public override string ToString()
    {
        string s = titleText + "\n";
        s += question + "\n";
        foreach (Speech speech in speeches)
            s += speech.text + "\n";

        return s;
    }
}