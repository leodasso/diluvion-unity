using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SpiderWeb;
using DUI;
using Diluvion;
using Diluvion.SaveLoad;
using Diluvion.Ships;
using CharacterInfo = Diluvion.CharacterInfo;
using Sirenix.OdinInspector;

#if UNITY_EDITOR
using UnityEditor;
#endif

public enum StoryState {

	none,
	birchton1,
	birchton2,
	birchton3,
	birchton4,
	birchton5,
	birchtonDefault,
	tear1,
	tear2,
	tear3
}

public enum HireStatus {
	onPlayerCrew,
	offPlayerCrew,
	any
}

public enum LocationStatus {
	any,
	onPlayerShip,
	offPlayerShip
}



[Serializable]
public class Speech
{
    [DisplayAsString, HideLabel]
    public string locKey = "";

    [Tooltip("Action happens at end of speech, before next speech")]
    public Diluvion.Action action;

    [AssetsOnly]
    public CharacterInfo speaker;

    [Multiline(6), HideLabel]
	public string text = "Dialogue text here pls.";

	public CrewAnimationTool animTool;

    /// <summary>
    /// Returns a localized list of strings, split at the @ symbol. This is the ready-to-display text.
    /// </summary>
    public List<string> FormattedText()
    {
        //Make an array of the sub dialogue pieces
        string[] pendingArray = LocalizedText().Split("@"[0]);
        List<string> stringList = new List<string>();

        //Make a list of the bits of dialogue
        stringList.AddRange(pendingArray);
        stringList = StringOps.RemoveEmptyStrings(stringList);

        return stringList;
    }

	public string LocalizedText()
	{
		return Localization.GetFromLocLibrary(locKey, text);
	}
}



/// <summary>
/// Dialogue class contains anything that a given character will say.  It stores lists of 
/// dialogueBits, which contain the text and a float of the last time that given text was said.
/// </summary>
public class Dialogue : MonoBehaviour, IHUDable {

    public Gender gender = Gender.Male;
    public string locKey = "";
    public bool omitFromLoc;
    public bool lockLocKeys;

    /// <summary>
    /// All the possible conversations that can be had by this character
    /// </summary>
    [ValidateInput("ValidateConvos", "Errors with convos setup! See log for details."), InlineEditor(Expanded = false)]
    public List<Convo> conversations = new List<Convo>();

    bool ValidateConvos(List<Convo> inputConvos)
    {
        List<string> locKeys = new List<string>();
        bool valid = true;
        
        foreach (var c in inputConvos)
        {
            // Check for empty loc key
            if (string.IsNullOrEmpty(c.titleText))
            {
                Debug.LogError("Title text for " + c.name + " is empty! This needs to be something because it is used as a loc key.", c);
                valid = false;
                continue;
            }
            
            // Check for duplicate loc key
            if (locKeys.Contains(c.titleText))
            {
                Debug.LogError("Duplicate titles found, more than one convo has the title " + c.titleText, c);
                valid = false;
            }
            
            locKeys.Add(c.titleText);
        }

        return valid;
    }

    /// <summary>
    /// Conversations that were added manually, like say from a character placer.
    /// </summary>
    [ReadOnly]
    public List<Convo> manualAdded = new List<Convo>();

    InteriorManager myInterior;
	bool inPlayerShip = false;

    /// <summary>
    /// All the triggers that have been added to this dialog instance during play session.
    /// </summary>
    List<string> triggers = new List<string>();
    Character myCharacter;

    public void CreateUI()
    {
        if (Time.timeScale == 0) return;

        DialogBox newBox = UIManager.Create(UIManager.Get().dialoguePanel as DialogBox);
        if (!newBox) return;

        newBox.SetState(DialogBox.DialogState.List);

        newBox.Init(this);
    }

    public void RemoveUI()
    {
        Debug.Log("Removing UI for " + name);
        UIManager.Clear<DialogBox>();
    }

	// Use this for initialization
	void Start ()
    {
        QuestManager.Get().questTick += DialogueUpdate;
	}

    void OnDestroy()
    {
        if (QuestManager.Exists())
            QuestManager.Get().questTick -= DialogueUpdate;
    }

    #region localization


#if UNITY_EDITOR
    /// <summary>
    /// Applies i2 loc keys to each bit of text that will be shown.
    /// </summary>
    [ButtonGroup("loc")]
    public void SetKeys()
    {
        if ( lockLocKeys ) return;

        locKey = Localization.key_dialog + gameObject.name + "_" + gender.ToString() + "_";

        //Apply conversation loc keys
        foreach ( Convo convo in conversations )
        {
            string convoKeyName = convo.titleText;
            if (string.IsNullOrEmpty(convoKeyName)) convoKeyName = convo.name;
            convo.locKey = locKey + "_" + Localization.CleanKey(convoKeyName);

            int index = 0;
            foreach ( Speech speech in convo.speeches )
            {
                speech.locKey = convo.locKey + "_sp_" + index;
                if (speech.speaker != null)
                {
                    speech.locKey += "_" + speech.speaker.niceName + "(" + speech.speaker.gender.ToString() + ")";
                }
                index++;
            }

            EditorUtility.SetDirty(convo);
        }
    }

    /// <summary>
    /// Adds all terms to the i2 loc library.
    /// </summary>
    [ButtonGroup("loc")]
    public void AddTerms()
    {
        SetKeys();

        foreach ( Convo conv in conversations ) conv.AddTerms();
    }

#endif

    /// <summary>
    /// Pulls the terms from the i2 source
    /// </summary>
    public void PullFromSource() 
	{
		foreach (Convo convo in conversations) convo.PullTerms();
	}

    /// <summary>
    /// Verifies that each term is in the i2 term library. If it's not, will log an error. 
    /// </summary>
    public void TestTerms()
    {
        Debug.Log("Checking Loc Terms on: " + gameObject.name);
        foreach ( Convo convo in conversations ) convo.TestTerms();
    }

    #endregion

    public Character MyCharacter()
    {
        if (myCharacter) return myCharacter;
        myCharacter = GetComponentInParent<Character>();
        return myCharacter;
    }

    /// <summary>
    /// Returns true if at least one of the current conversation options gives a quest.
    /// This allows for showing an icon when a character has quest to give.
    /// </summary>
    public bool HasQuestToGive()
    {
        foreach (Convo c in GetNewDialog())
        {
            if (!c.IsValid(this)) continue;
            if (c.GivesQuest()) return true;
        }

        return false;
    }


    /// <summary>
    /// Dialogue update gets called on every quest tick. Checks through all dialogues to see
    /// if there's any new valid dialogues that need to add a chatter.
    /// </summary>
	void DialogueUpdate() {
		
        if (myInterior == null) myInterior = GetComponentInParent<InteriorManager>();
        if (myInterior == null) return;
        if (!OnPlayerCrew()) return;
        if (!myInterior.SubjectOfCamera()) return;

        // Check all conversations for chatter
        foreach (Convo convo in conversations)
        {
            if (convo == null) continue;
            if (convo.manualAdd) continue;
            ShowChatter(convo);
        }
	}

    /// <summary>
    /// Returns true if there's any conversations that the player can have with this instance currently.
    /// </summary>
    public bool HasDialogue()
    {
        if (GetLogDialog() == null || GetNewDialog() == null) return false;
        if ( GetNewDialog().Count + GetLogDialog().Count > 0 ) return true;
        return false;
    }

    /// <summary>
    /// Returns true if theres any NEW conversations that can be had with this character.
    /// </summary>
    /// <returns></returns>
    public bool HasNewDialogue()
    {
        return GetNewDialog().Count > 0;
    }

    List<Convo> _newConvosList = new List<Convo>();
    /// <summary>
    /// returns dialogues that are meant to be shown by dialogue box
    /// </summary>
    public List<Convo> GetNewDialog()
    {
        _newConvosList.Clear();

        // Check all my conversations to see which ones are new and valid.
        for (int i = 0; i < conversations.Count; i++)
        {
            if (conversations[i] == null) continue;
            if (!ValidForDialogBox(conversations[i])) continue;
            if (conversations[i].manualAdd) continue;

            _newConvosList.Add(conversations[i]);
        }

        // Show manually added dialogues
        foreach (Convo c in manualAdded)
            if (ValidForDialogBox(c)) _newConvosList.Add(c);

        // order by priority
        if (_newConvosList.Count > 1)
            _newConvosList = _newConvosList.OrderBy( go => go.priority).ToList();

        return _newConvosList;
    }

    /// <summary>
    /// Returns true if the given conversation is current and has never been read
    /// </summary>
    bool ValidForDialogBox(Convo c)
    {
        if (!c) return false;
        if (!c.IsValid(this)) return false;
        if (c.BeenRead(MyCharacter())) return false;
        if (!c.NeedsDialogBox()) return false;
        return true;
    }

    List<Convo> _returnList = new List<Convo>();
    
    /// <summary>
    /// Returns a full list of dialogue that's been previously read by the player (for this character).
    /// List is ordered chronologically by when player read the dialog in game.
    /// </summary>
    public List<Convo> GetLogDialog()
    {
        if (DSave.current == null) return null;
        _returnList.Clear();
        //List<Convo> returnList = new List<Convo>();

        // Check all my conversations to see which ones are new and valid.
        for (int i = 0; i < DSave.current.shownDialogKeys.Count; i++)
        {
            string key = DSave.current.shownDialogKeys[i];
            Convo c = ConvoFromSaveKey(key);
            if (c == null) continue;
            if (!c.QueriesPassed(this)) continue;
            
            _returnList.Add(c);
        }

        _returnList.Reverse();

        return _returnList;
    }

    Convo ConvoFromSaveKey(string saveKey)
    {
        //Debug.Log("Compare " + saveKey + " & " + myCrew.NonLocalizedName() + saveKey.Contains(myCrew.NonLocalizedName()));
        // Don't check keys that aren't related to this character
        if (!saveKey.Contains(MyCharacter().NonLocalizedName())) return null;

        string convoName = DiluvionSaveData.DialogTrimmedKey(saveKey);
        for (int j = 0; j < conversations.Count; j++)
        {
            if (conversations [j] == null) continue;
            if (conversations[j].name == convoName) return conversations[j];
        }
        return null;
    }

    #region dialogue pushing

    /// <summary>
    /// Finds all conversations that match the given trigger 'tag', and pushes a random one of them.
    /// Also adds the tag to list of triggers.
    /// </summary>
    public void PassHashtag(string tag)
    {
        if (!triggers.Contains(tag)) triggers.Add(tag);
        PushRandomDialogue(TaggedDialog(tag));
    }

    /// <summary>
    /// If the given conversation is contained in this dialogue, show the chatter & activate the given conversation.
    /// </summary>
    /// <param name="c">Convo to show.</param>
    public void ShowConversation(Convo c)
    {
        // If it's not a manual add convo, and I don't have it in my thing, then return.
        if (!conversations.Contains(c) && !c.manualAdd) return;
        
        // If we get here, it means my dialog list contains this convo. Add it to the manualAdded list so it can be displayed.
        if (!manualAdded.Contains(c)) manualAdded.Add(c);
        ShowChatter(c);
    }

    /// <summary>
    /// For each convo in the list, checks if it's part of this dialog, and if so, activates it.
    /// </summary>
    public void ShowConversation(List<Convo> convos)
    {
        foreach (Convo c in convos) ShowConversation(c);
    }

    /// <summary>
    /// Returns a list of all dialogue that matches at least one of the triggers in this dialogue.
    /// </summary>
    List <Convo> TaggedDialog(string tag)
    {
        List<Convo> dialogWithTag = new List<Convo>();
        for (int i = 0; i < conversations.Count; i++)
        {
            if (conversations[i] == null) continue;
            if (!conversations[i].IsValid(this)) continue;
            if (conversations[i].HasTrigger(tag)) dialogWithTag.Add(conversations[i]);
        }
        return dialogWithTag;
    }

    ///Pushes a random dialogue bit from the given list.
    void PushRandomDialogue(List<Convo> taggedConvo)
    {
        if (taggedConvo.Count < 1) return;

		//get max index, find a random index 
		int max = taggedConvo.Count;
		int randomIndex = UnityEngine.Random.Range(0, max);

		Convo chosen = taggedConvo[randomIndex];

        ShowChatter(chosen);
	}

    /// <summary>
    /// Checks if convo is valid. If so, checks if it can show chatter, and if true, will show chatter.
    /// </summary>
    void ShowChatter(Convo dialogToAdd)
    {
        // Check if the dialog is valid, given current game conditions
        if (!dialogToAdd.IsValid(this)) return;
        if (dialogToAdd.BeenRead(MyCharacter()) && !dialogToAdd.chatterEveryTime) return;
        //if ()
        if (dialogToAdd.ShowsChatter()) DUIChatterBox.AddChatter(dialogToAdd, this);
    }

	#endregion


    public bool HasTrigger(string trigger)
    {
        return triggers.Contains(trigger);
    }

    /// <summary>
    /// Returns true if the character this dialog is for is currently on the player ship.
    /// For sailors just checks directly, but for premade characters, it checks if any instance of that character
    /// is on the ship.
    /// </summary>
	public bool OnPlayerCrew() {

        if (!MyCharacter()) return false;
        if (PlayerManager.PlayerCrew() == null) return false;

        Sailor s = MyCharacter() as Sailor;
        if (s)
        {
            CrewManager cm = GetComponentInParent<CrewManager>();
            if (cm == PlayerManager.PlayerCrew()) return true;
        }

        return PlayerManager.PlayerCrew().HasCrewmember(MyCharacter());
	}


    /// <summary>
    /// Creates a specific instance of the dialogue prefab
    /// </summary>
    public Dialogue NewDialogInstance(Character ch)
    {
        GameObject dialogueInstance = Instantiate(gameObject, ch.transform.position, Quaternion.identity) as GameObject;
        dialogueInstance.transform.parent = ch.transform;
        dialogueInstance.name = ch.name + " dialogue instance";

        Dialogue dScript = dialogueInstance.GetComponent<Dialogue>();
        return dScript;
    }
}