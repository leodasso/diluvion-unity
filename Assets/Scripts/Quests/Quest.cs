using UnityEngine;
using System;
using System.Collections.Generic;
using Quests;

public enum QuestConditionType {
	DestroyHull,			//subject destroyed target hull
	DockTo,					//subject docked to target
	UndockFrom,				//subject undocked form target
	TalkTo,					//subject talked to target
	hasItem,				//subject has the item on their inventory
	NavigateTo,				//subject navigated to 
	SonarPing,				//subject pinged (no target necessary)
	Looted,					//subject looted target inventory
	AcquiredItem,			//subject acquired target item
	Hired,					//subject hired target crew
	HasOnCrew,				//subject has target on crew
    Repaired,                //subject repaired target hull
	currentCrewQty,			// Number of current crew
	shipLevel,				// the current level of the ship
	otherQuestComplete,
	DestroySpawns,
    hasStation
}

public enum QuestAction {
	none,
	startQuest,
	completeQuest
}

public enum QuestStatus {
	Complete,
	InProgress,
	NotStarted,
    NotFinished,
	InProgOrComplete
}

[System.Serializable]
public class QuestSave
{
    public string locKey;
    public bool complete = false;
    public bool hidden = false;
    public List<bool> contitionStatus;

    public QuestSave()
    {
        locKey = "";
        complete = false;
        contitionStatus = new List<bool>();
    }

    public QuestSave(QuestSave qs)
    {

        locKey = qs.locKey;
        complete = qs.complete;
        hidden = qs.hidden;
        contitionStatus = new List<bool>(qs.contitionStatus);
    }
}