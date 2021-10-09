using System;
using System.Collections.Generic;
using Diluvion;

public enum AnimationType
{
    none,
    whenDisplayed
}

[Serializable]
public class CrewAnimationTool
{

    public AnimationType animationType = AnimationType.none;
    public string animTag = "none";
    public float animTime = 0;
    public bool setAsDefault = false;

    public void ApplyAnimation(Character subject)
    {
        subject.SetAnimation(animTag, animTime, setAsDefault);
    }
}

[Serializable]
public class CrewSave
{
    public string crewName;
    public List<string> generatedName = new List<string>();
    public List<float> stats;
    public Gender gender = Gender.Male;
    public VoiceType voiceType = VoiceType.Somber;
    public string stationName = "";
    public string controllerName = "";		// sprite controller
    public string dialogueName = "";
    public bool currentlyOnShip = false;
    public bool guest = false;
    public bool officer = false;
    public string spriteObj;
    public int cost = 0;
    public string perkName = ""; // defunct
}