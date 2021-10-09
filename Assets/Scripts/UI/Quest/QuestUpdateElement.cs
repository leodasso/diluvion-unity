using System.Collections;
using System.Collections.Generic;
using Quests;
using TMPro;
using UnityEngine;

public class QuestUpdateElement : MonoBehaviour
{

	public TextMeshProUGUI textPanel;
	public DQuest quest;
	public Objective objective;

	public void Init(DQuest newQuest, Objective newObj)
	{
		quest = newQuest;
		objective = newObj;

		string objectiveText = quest.GetLocTitle();

		string newText;

		if (quest.HasObjectiveDescription(out newText, objective))
		{
			objectiveText += " - " + newText;
		}
		
		string formattedText = string.Format(textPanel.text, objectiveText);
		textPanel.text = formattedText;
	}
}
