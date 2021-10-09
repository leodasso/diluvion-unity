using UnityEngine;
using Quests;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SpiderWeb;
using Diluvion.SaveLoad;
using Sirenix.OdinInspector;

namespace Diluvion
{

    ///This component goes on world object, and keeps track of all quests
    ///that are in progress or complete.  
    public class QuestManager : MonoBehaviour
    {
        public static QuestManager qm;
        public delegate void QuestTick ();
        public QuestTick questTick;

        public Ping mostRecentPing;


        /// <summary>
        /// The single, selected waypoint that the guiding fish will guide towards
        /// </summary>
        [ShowInInspector, ReadOnly]
        QuestActor displayedWaypoint;

        /// <summary>
        /// All waypoints that are currently active (i.e. displayed on the map)
        /// </summary>
        [ShowInInspector, ReadOnly]
        List<QuestActor> activeWaypoints = new List<QuestActor>();

        /// <summary>
        /// Have all the quests from the current save file been added to the tick?
        /// </summary>
        bool _savedQuestsAdded;


        IEnumerator Start()
        {
            yield return new WaitForSeconds(2);
            Tick();
        }

        /// <summary>
        /// Call this whenever a game is loaded to ensure that quest manager is in the scene and all quests
        /// relevant to the save file have had their checks added to the tick.
        /// </summary>
        public static void GameLoaded()
        {
            Get().AddQuestsToTick();
        }


        /// <summary>
        /// Adds any quests from save file to the tick
        /// </summary>
        void AddQuestsToTick ()
        {
            if (_savedQuestsAdded) return;
            
            if (DSave.current == null)
            {
                Debug.LogError("No save file, can't load quests! This play session will be broken.");
                return;
            }

            _savedQuestsAdded = true;

            foreach (DQuestSave qs in DSave.current.questSaves)
            {
                DQuest questObj = QuestsGlobal.GetQuest(qs.key);
                if (questObj == null) return;

                //Debug.Log("Got quest object " + questObj.name, questObj);
                questObj.AddTick();
            }
        }

        public static bool Exists()
        {
            return qm != null;
        }

        /// <summary>
        /// Returns the quest manager instance.
        /// </summary>
        public static QuestManager Get ()
        {
            if (qm != null) return qm;

            // If no qm instance exists, add the qm component to world control
            WorldControl wc = WorldControl.Get();
            DontDestroyOnLoad(wc.gameObject);
            qm = GO.MakeComponent<QuestManager>(wc.gameObject);
            return qm;
        }

        #region waypoints

        /// <summary>
        /// The main waypoint that navigation will point to.
        /// </summary>
        public static QuestActor MainWaypoint()
        {
            return Get().displayedWaypoint;
        }

        /// <summary>
        /// Returns a list of all available waypoints
        /// </summary>
        public static List<QuestActor> GetAllWaypoints ()
        {
            // Remove any null items from the list.
            Get().activeWaypoints = Get().activeWaypoints.Where(x => x != null).ToList();
            return Get().activeWaypoints;
        }

        /// <summary>
        /// Adds the given quest actor to list of available waypoints.
        /// </summary>
        public static void AddWaypoint (QuestActor wp)
        {
            if (Get().activeWaypoints.Contains(wp)) return;
            Get().activeWaypoints.Add(wp);
            
            // Set a GUI waypoint to the given quest actor
            DUI.WaypointHUD.CreateWaypoint(wp);
            
            if (!Get().displayedWaypoint) SetMainWaypoint(wp);
        }

        /// <summary>
        /// Removes the quest actor from the list of active waypoints, and 
        /// if it was the main waypoint, clears the main waypoint.
        /// </summary>
        public static void RemoveWaypoint(QuestActor wp)
        {
            Get().activeWaypoints.Remove(wp);
            
            DUI.WaypointHUD.RemoveWaypoint(wp);

            if (Get().displayedWaypoint == wp)
            {
                Get().displayedWaypoint = null;
                SetNextWaypointActive();
            }
        }

        /// <summary>
        /// If no waypoints are currently active, takes the next waypoint and sets it active
        /// </summary>
        static void SetNextWaypointActive()
        {
            // If there's already a main waypoint, take no action.
            if (Get().displayedWaypoint != null) return;
            if (Get().activeWaypoints.Count < 1) return;
            SetMainWaypoint(Get().activeWaypoints[0]);
        }

        /// <summary>
        /// Sets the given quest actor as the main waypoint
        /// </summary>
        public static void SetMainWaypoint(QuestActor wp)
        {
            Get().displayedWaypoint = wp;
        }

        #endregion

        /// <summary>
        /// This gets called whenever progress has been made in the game. i.e. crew hired, docking, quest
        /// completed, etc.
        /// </summary>
        public static void Tick ()
        {
            if (Get() == null) return;
            Get().LocalTick();
        }

        [Button]
        void LocalTick ()
        {
            //Debug.Log("Quest manager tick at " + Time.unscaledTime);
            if (questTick != null) questTick();
        }
    }
}