using System.Collections;
using System.Collections.Generic;
using Quests;
using SpiderWeb;
using UnityEngine;

namespace DUI
{
	public class QuestUpdateLog : DUIPanel
	{
		public QuestUpdateElement elementPrefab;
		public Transform layoutGroup;
		public int itemsToShow = 4;
		[Tooltip("How many seconds will the element hang around for?")]
		public float showTime = 10;

		float _showTimer;

		static QuestUpdateLog _instance;

		static QuestUpdateLog Get()
		{
			if (_instance) return _instance;

			_instance = UIManager.Create(UIManager.Get().questUpdateLog as QuestUpdateLog);
			return _instance;
		}

		/// <summary>
		/// Shows an update for the given quest and objective.
		/// </summary>
		public static void ShowUpdate(DQuest quest, Objective obj)
		{
			Get().AddElement(quest, obj);
		}

		protected override void Update()
		{
			base.Update();
			if (_showTimer > 0)
			{
				_showTimer -= Time.deltaTime;
			}
			else
			{
				alpha = 0;
			}
		}

		void AddElement(DQuest quest, Objective obj)
		{
			alpha = 1;
			_showTimer = showTime;
			
			// Instantiate the new element and set it as the first sibling (so it appears at the top of the list
			QuestUpdateElement newElement = Instantiate(elementPrefab, layoutGroup);
			newElement.transform.SetAsFirstSibling();
			newElement.Init(quest, obj);
			
			SpiderSound.MakeSound("Play_MUS_New_Objective_Stinger", gameObject);

			// If there's too many elements showing, remove the last one.
			if (layoutGroup.transform.childCount > itemsToShow)
			{
				Destroy(layoutGroup.transform.GetChild(layoutGroup.transform.childCount - 1).gameObject);
			}
		}
	}
}