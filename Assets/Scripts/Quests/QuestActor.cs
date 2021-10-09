using UnityEngine;
using System.Collections;
using Quests;
using Sirenix.OdinInspector;
using Diluvion.SaveLoad;

namespace Diluvion
{

    /// <summary>
    /// Receives a quest tick from QuestManager, and will take an action based on the status of my
    /// quest & objective.
    /// </summary>
    public abstract class QuestActor : MonoBehaviour
    {
        [Tooltip("Will show up on the map, but not the HUD. useful for waypoints to a general area"), ToggleLeft]
        public bool hideHud;
        
        [ReadOnly, HorizontalGroup("tick"), ToggleLeft]
        public bool tickAdded;
        
        [ReadOnly, Tooltip("The last time this object was updated by quest manager"), HorizontalGroup("tick"), HideLabel]
        public float lastTick = -99;
        
        [ReadOnly, ToggleLeft]
        public bool currentlyActive;

        [ToggleLeft]
        public bool checkQuest;
        
        [ShowIf("checkQuest"), AssetsOnly, HorizontalGroup(group: "quest", MarginLeft = 20), HideLabel]
        public DQuest quest;
        
        [ShowIf("checkQuest"), HorizontalGroup(group: "quest"), HideLabel]
        public QuestStatus questStatus = QuestStatus.InProgress;

        protected bool isActive;

        [ShowIf("checkQuest"), Space, ToggleLeft]
        public bool checkObjective;
        
        [ShowIf("ShowObjStatus"), AssetsOnly, HideLabel, HorizontalGroup(group:"obj"), AssetList(CustomFilterMethod = "PartOfQuest")]
        public Objective objective;
        
        bool PartOfQuest(Objective o)
        {
            if (!quest) return false;
            return quest.HasObjective(o);
        }
        

        [ShowIf("ShowObjStatus"), HideLabel, HorizontalGroup(group: "obj", MarginLeft = 20)]
        public QuestStatus objectiveStatus;

        [ToggleLeft]
        public bool debug;

        /// <summary>
        /// Has this ticked at least once? Required to properly set active / inactive
        /// </summary>
        bool _ticked;
        



        public bool ShowObjStatus()
        {
            return (checkQuest && checkObjective);
        }
        

        // Use this for initialization
        IEnumerator Start ()
        {

            while (!QuestManager.Exists())
            {
                yield return null;
            }

            TryAddTick();
        }

        void OnEnable()
        {
            TryAddTick();
        }

        void TryAddTick()
        {
            if (tickAdded) return;
            tickAdded = true;
            QuestManager.Get().questTick += Tick;
            QuestManager.Tick();
        }

        void OnDestroy ()
        {
            if (QuestManager.Exists())
                QuestManager.Get().questTick -= Tick;
        }

        /// <summary>
        /// Ticks with the questmanager.Tick()
        /// </summary>
        protected virtual void Tick ()
        {
            lastTick = Time.time;
            bool wasActive = isActive;
            isActive = Match();
            
            //if (debug) Debug.Log(name + " ticking at " + Time.time, gameObject);
            
            // First time tick checks
            if (!_ticked)
            {
                if (isActive) OnActivate();
                else OnDeactivate();

                _ticked = true;
                return;
            }

            if (!wasActive && isActive) OnActivate();
            if (wasActive && !isActive) OnDeactivate();
        }

        /// <summary>
        /// Sets this as the currently active navigation waypoint. There can only be one of these
        /// at a time!
        /// </summary>
        public virtual void SelectWaypoint ()
        { }

        protected virtual void OnActivate ()
        {
            if (debug) Debug.Log(gameObject.name + " activated from quest!");
            currentlyActive = true;
        }

        protected virtual void OnDeactivate ()
        {
            if (debug) Debug.Log(gameObject.name + " deactivated from quest.");
            currentlyActive = false;
        }

        /// <summary>
        /// Returns true if all the user set conditions match the game's current state.
        /// </summary>
        protected bool Match ()
        {
            if (DSave.current == null) return false;
            if (quest == null) return false;
            if (checkQuest)
                if (!DSave.current.IsQuestStatus(quest, questStatus)) return false;

            if (checkObjective && objective)
            {
                return objective.IsOfStatus(objectiveStatus, quest);
            }

            return true;
        }

        void Test ()
        {
            Debug.Log(ToString());
        }

        public override string ToString ()
        {
            string returnString = "";

            if (!checkQuest) return "No checks. Will never act.";

            if (checkQuest) returnString += "if quest " + quest.name + " is " + questStatus;
            if (checkObjective) returnString += " and " + objective.name + " is " + objectiveStatus;
            return returnString;
        }
    }
}