using System.Collections;
using System.Collections.Generic;
using Diluvion;
using Quests;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using SpiderWeb;

namespace DUI
{

	/// <summary>
	/// The map waypoint component goes on an object that displays a waypoint on the zone map (not world map). It's instantiated in
	/// by DUIZoneMap for each quest actor that's visible to QuestManager.
	/// </summary>
	public class MapWaypoint : MonoBehaviour
	{

		[ReadOnly]
		public DQuest quest;

		[ReadOnly] 
		public DUIZoneMap mapParent;

		[ReadOnly]
		public QuestActor linkedWaypoint;
		
		[Space]
		public Image iconImage;
		public CanvasGroup activeWaypointDescription;
		public ClampToScreen clamper;
		public RectTransform questGuiParent;
		public GameObject questGuiPrefab;
		
		[Space]
		[Tooltip("The sprite to show when this is the active waypoint")]
		public Sprite activeSprite;
		
		[Tooltip("The sprite to show when this is an inactive waypoint")]
		public Sprite inactiveSprite;

		bool _displayingAsActive;

		GameObject _questGuiInstance;
		RectTransform _myRectTransform;

		void Start()
		{
			_myRectTransform = GetComponent<RectTransform>();

			// Destroy any previous quest GUI left in the prefab
			DUIQuest oldQuest = questGuiParent.GetComponentInChildren<DUIQuest>();
			if (oldQuest) Destroy(oldQuest.gameObject);
		}

		public void InitForWaypoint(DUIZoneMap zoneMap, QuestActor wp)
		{
			mapParent = zoneMap;
			quest = wp.quest;
			linkedWaypoint = wp;

			if (clamper)
			{
				clamper.clampToRect = true;
				
				// find the scroll rect ancestor in the hierarchy
				ScrollRect s = GetComponentInParent<ScrollRect>();
				if (s) clamper.clamperRect = s.GetComponent<RectTransform>();
			}
			
			ShowQuestGUI();
		}

		/// <summary>
		/// Sets the linked quest actor as Quest Manager's main waypoint.
		/// </summary>
		public void SetActive()
		{
			if (!linkedWaypoint) return;
			QuestManager.SetMainWaypoint(linkedWaypoint);
			SpiderSound.MakeSound("Play_Captains_Log_Select_Waypoint", gameObject);
		}

		/// <summary>
		/// Shows the full quest GUI for the linked quest actor's quest.
		/// </summary>
		void ShowQuestGUI()
		{
			if (!linkedWaypoint) return;
			if (!linkedWaypoint.quest) return;
			if (_questGuiInstance) return;

			// show the quest GUI
			_questGuiInstance = Instantiate(questGuiPrefab, questGuiParent);

			DUIQuest duiQuest = _questGuiInstance.GetComponent<DUIQuest>();
			
			duiQuest.Init(linkedWaypoint.quest);
			
			// Have the quest display only show my waypoint's specific objective
			if (linkedWaypoint.objective)
				duiQuest.ShowObjective(linkedWaypoint.objective);
			
			// If there's no objective specific to the waypoint, don't show any
			else if (linkedWaypoint.checkObjective == false)
				duiQuest.ClearObjectives();
		}


		// Update is called once per frame
		void Update()
		{
			/* In update, we want to reflect if this waypoint is the current main waypoint or not. We'll do this by setting
			the _displayingAsActive variable, and then using that to switch sprites between active / inactive. */
			
			if (!linkedWaypoint)
			{
				_displayingAsActive = false;
			}

			else
			{
				// Check if my linked waypoint is the active waypoint
				_displayingAsActive = QuestManager.MainWaypoint() == linkedWaypoint;
				
				// position the waypoint correctly on the map
				if (mapParent)
					_myRectTransform.anchoredPosition = mapParent.PositionOnMap(linkedWaypoint.transform);
			}


			// Display the correct sprite for active / inactive waypoint
			iconImage.sprite = _displayingAsActive ? activeSprite : inactiveSprite;
			
			// Display or hide the description
			activeWaypointDescription.alpha = _displayingAsActive ? 1 : 0;
		}
	}
}