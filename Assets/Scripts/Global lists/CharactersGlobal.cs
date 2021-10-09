using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Diluvion;
using CharacterInfo = Diluvion.CharacterInfo;
using Sirenix.OdinInspector;

[System.Serializable]
public class PoseHolder
{
    public Poses pose;
    public Sprite sprite;
}

[CreateAssetMenu(fileName = "characters global", menuName = "Diluvion/global lists/characters")]
public class CharactersGlobal : GlobalList {

    [TabGroup("Main")]
    public Sailor sailorTemplate;

    [TabGroup("Main")]
    public List<CharacterInfo> allChars = new List<CharacterInfo>();

    [TabGroup("Main")]
    public List<Appearance> allAppearances = new List<Appearance>();

    [TabGroup("Sailor Gen")]
    public List<Dialogue> sailorDialogue = new List<Dialogue>();

    [TabGroup("Sailor Gen")]
    public List<RuntimeAnimatorController> sailorAnims = new List<RuntimeAnimatorController>();

    [TabGroup("Sailor Gen")]
    public List<string> maleNames = new List<string>();

    [TabGroup("Sailor Gen")]
    public List<string> femaleName = new List<string>();

    [TabGroup("Sailor Gen")]
    public List<string> lastNames = new List<string>();

    [TabGroup("Sailor Gen")]
    public List<Appearance> sailorAppearances = new List<Appearance>();

    [TabGroup("Sailor Gen")]
    public List<PoseHolder> poses = new List<PoseHolder>();

    public static CharactersGlobal charactersGlobal;

    public static string namePrefix = "name_full_";
    public static string maleNamePrefix = "name_m_";
    public static string femaleNamePrefix = "name_f_";
    public static string lastNamePrefix = "name_last_";

    const string resourceName = "characters global";

    public static Sailor SailorTemplate()
    {
        return Get().sailorTemplate;
    }

    /// <summary>
    /// Returns the mannequin sprite for the given pose
    /// </summary>
    public static Sprite SpriteForPose(Poses pose)
    {
        foreach (PoseHolder p in Get().poses)
        {
            if (p.pose == pose) return p.sprite;
        }

        return null;
    }

    public static CharactersGlobal Get()
    {
        if (charactersGlobal != null) return charactersGlobal;
        charactersGlobal = Resources.Load(resourcesPrefix + resourceName) as CharactersGlobal;
        return charactersGlobal;
    }

    
    public override void FindAll()
    {
       
        ConfirmObjectExistence(Get(), (resourcesPrefix + resourceName));

        allChars = LoadObjects<CharacterInfo>("Assets/Prefabs/Characters");
        allAppearances = LoadObjects<Appearance>("Assets/Prefabs/Appearances");
        Debug.Log("loading all characters.");
#if UNITY_EDITOR
        SetDirty(this);
#endif
    }

    protected override void TestAll()
    {
        base.TestAll();
        TestAllObjects(allChars, new GetObjectDelegate(GetCharacter));
        TestAllObjects(sailorDialogue, new GetObjectDelegate(GetDialogue));
        TestAllObjects(sailorAnims, new GetObjectDelegate(GetAnimController));
        TestAllObjects(allAppearances, new GetObjectDelegate(GetAppearance));
    }
   

    public static CharacterInfo GetCharacter(string nameKey)
    {
        return GetObject(nameKey, Get().allChars) as CharacterInfo;
    }

    public static Dialogue GetDialogue(string nameKey)
    {
       return GetObject(nameKey, Get().sailorDialogue) as Dialogue;
    }

    public static Dialogue RandomDialogue()
    {
        int index = Random.Range(0, Get().sailorDialogue.Count);
        return Get().sailorDialogue[index];
    }

    public static Appearance GetAppearance(string nameKey)
    {
        return GetObject(nameKey, Get().allAppearances) as Appearance;
    }

    /// <summary>
    /// Returns an appearance that uses the given animation controller. 
    /// </summary>
    public static Appearance AppearanceForController(string controllerName)
    {
        foreach (Appearance a in Get().allAppearances)
        {
            if (!a) continue;
            if (!a.animController) continue;
            if (a.animController.name == controllerName) return a;
        }
        return null;
    }

    /// <summary>
    /// Returns a random appearance meant for sailor generation using the given gender.
    /// </summary>
    public static Appearance RandomAppearance(Gender gender)
    {
        List<Appearance> list = Get().sailorAppearances.Where(x => x != null).ToList();
        
        list = list.Where(x => x.gender == gender).ToList();

        int index = Random.Range(0, list.Count);
        return list[index];
    }

    public static RuntimeAnimatorController GetAnimController(string nameKey)
    {
        return GetObject(nameKey, Get().sailorAnims) as RuntimeAnimatorController;
    }

    /// <summary>
    /// Creates a random name, and returns a list with first name, last name.  These are actually loc keys
    /// so this function can still be used in localized versions.
    /// </summary>
    public static List<string> RandomName(Gender gender)
    {
        List<string> firstNames = new List<string>();
        if (gender == Gender.Male) firstNames.AddRange(Get().maleNames);
        if (gender == Gender.Female) firstNames.AddRange(Get().femaleName);

        List<string> lastNames = new List<string>();
        lastNames.AddRange(Get().lastNames);

        //pick a random first name
        string firstName = firstNames[UnityEngine.Random.Range(0, firstNames.Count)];

        //pick a random last name
        string lastName = lastNames[UnityEngine.Random.Range(0, lastNames.Count)];

        List<string> myName = new List<string>();
        myName.Add(firstName);
        myName.Add(lastName);

        if (myName.Count < 2) Debug.LogError("Full name wasn't generated!");

        return myName;
    }
}