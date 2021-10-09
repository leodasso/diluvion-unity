using UnityEngine;
using System.Collections.Generic;
using Queries;
using Diluvion;
using Diluvion.Achievements;
using Diluvion.SaveLoad;
using DUI;
using Loot;
using Sirenix.OdinInspector;
using UnityEngine.Events;

namespace Quests
{
    [System.Serializable]
    public class ObjectiveContainer
    {
        public Objective objective;
        [Tooltip("If true, waits for all previous objectives to be complete before starting. Quest's sequential value overwrites this.")]
        public bool requirePrevious;
        public bool overrideName;
        [ShowIf("overrideName"), HideLabel]
        public string newName = "a great objective.";

        public ObjectiveContainer()
        {
            objective = null;
            overrideName = false;
        }

        public string LocKey()
        {
            return objective == null ? "_no objective" : objective.name;
        }
    }

    [CreateAssetMenu(fileName = "new quest", menuName = "Diluvion/Quests/Quest", order = 0)]
    public class DQuest : ScriptableObject
    {
        public bool debug;
        
        [OnValueChanged("RefreshLoc")]
        public string locKey;

        [Tooltip("If true, only one objective can be active at a time.")]
        public bool sequential; 
         
        public string title;

        [Multiline]
        public string description;

        [ShowInInspector, DisplayAsString]
        string locTitle;

        [ShowInInspector, DisplayAsString]
        string locDescr;

        [ToggleLeft]
        public bool showsQuestCompleteGui = true;
        
        [Tooltip("Start another quest when this one is complete?"), ToggleLeft]
        public bool startAnotherQuest;

        [ShowIf("startAnotherQuest"), AssetsOnly]
        public DQuest questToStart;

        [DrawWithUnity]
        public UnityEvent onQuestComplete;
        

        ///Prereqs for this quest to start
        public List<Query> prereqsToStart = new List<Query>();
        public List<ObjectiveContainer> objectiveCs = new List<ObjectiveContainer>();

        /// <summary>
        /// Has this quest been started in the current save file?
        /// </summary>
        public bool IsOfStatus(QuestStatus status)
        {
            if (DSave.current == null) return false;
            return DSave.current.IsQuestStatus(this, status);
        }

        /// <summary>
        /// Returns true if this quest is beyond the given status; i.e. if the status is 'in progress' 
        /// and the quest is complete, this will return true.
        /// </summary>
        public bool IsBeyondStatus(QuestStatus status)
        {
            if (status == QuestStatus.Complete && IsOfStatus(QuestStatus.Complete)) return true;

            if (status == QuestStatus.InProgress)
            {
                if (IsOfStatus(QuestStatus.InProgress) || IsOfStatus(QuestStatus.Complete)) return true;
            }

            if (status == QuestStatus.NotStarted)
            {
                if (IsOfStatus(QuestStatus.InProgress) || IsOfStatus(QuestStatus.Complete) ||
                    IsOfStatus(QuestStatus.NotStarted)) return true;
            }
            return false;
        }

        /// <summary>
        /// Starts this quest, adds to the save file.
        /// </summary>
        [Button()]
        public void StartQuest()
        {
            if (DSave.current == null) return;

            //Debug.Log("Starting quest " + ToString(), this);

            AddTick();

            // Check if there's already a save for this quest.
            if (DSave.current.HasQuest(this)) return;

            // If not, make one.
            DQuestSave newQuestSave = new DQuestSave(this);
            DSave.current.questSaves.Add(newQuestSave);

            if (!sequential)
            {
                foreach (ObjectiveContainer oc in objectiveCs)
                    oc.objective.ProgressObjective(this);
            }
            
            DUIQuestShower.ShowQuest(this);

            ProgressMade();
        }

        /// <summary>
        /// Add my check to the quest manager's tick.
        /// </summary>
        public void AddTick()
        {
            QuestManager.Get().questTick += CheckMyObjectives;
        }

        /// <summary>
        /// Starts the next objective that hasn't started yet. If there's already an objective in progress, does nothing.
        /// </summary>
        void StartNextObjective()
        {
            if (DSave.current == null) return;

            foreach (ObjectiveContainer oc in objectiveCs)
            {
                if (oc.objective.IsOfStatus(QuestStatus.InProgress, this)) return;
                if (oc.objective.IsOfStatus(QuestStatus.NotStarted, this))
                {
                    oc.objective.ProgressObjective(this);
                    return;
                }
            }
        }

        /// <summary>
        /// Logs the current status of this quest
        /// </summary>
        [Button]
        public void CheckStatus()
        {
            if (DSave.current == null)
            {
                Debug.Log("No save file! This quest can't save status.");
                return;
            }

            Debug.Log("Quest status for " + name + " Started: " + DSave.current.HasQuest(this) + 
            " Finished: " + DSave.current.IsQuestStatus(this, QuestStatus.Complete));
            foreach (ObjectiveContainer oc in objectiveCs)
            {
                Debug.Log("Objective " + oc.objective.name + " in progress: " + oc.objective.IsOfStatus(QuestStatus.InProgress, this) + " complete: " + oc.objective.IsComplete(this));
            }
        }
       

        /// <summary>
        /// Calls 'checkObjective' on each of my objectives to see if theyre complete.
        /// </summary>
        void CheckMyObjectives()
        {
            //Debug.Log("Quest " + name + " is checking objectives.");
            foreach (ObjectiveContainer oc in objectiveCs)
                oc.objective.CheckObjective(this);
        }

        /// <summary>
        /// Return a list of all objectives currently relavent. For sequential quests, only shows one objective at a time.
        /// </summary>
        public List<ObjectiveContainer> CurrentObjectives()
        {
            List<ObjectiveContainer> oList = new List<ObjectiveContainer>();

            foreach (ObjectiveContainer oc in objectiveCs)
            {
                // Don't add objectives which aren't in progress
                if (!oc.objective.IsOfStatus(QuestStatus.InProgress, this)) continue;

                // Don't add objectives which don't have a description
                if (!VisibleObjective(oc)) continue;

                // Add the objective to the list
                oList.Add(oc);

                // If sequential and we've reached the first objective that is in progress, stop.
                if (sequential && oc.objective.IsOfStatus(QuestStatus.InProgress, this)) break;
            
            }

            return oList;
        }


        public bool VisibleObjective(Objective o)
        {
            foreach (ObjectiveContainer oc in objectiveCs)
            {
                if (oc.objective == o)
                    return VisibleObjective(oc);
            }

            return false;
        }

        /// <summary>
        /// Does the given objective container have a description?
        /// </summary>
        bool VisibleObjective(ObjectiveContainer oc)
        {
            if (!oc.overrideName) return false;
            if (string.IsNullOrEmpty(SpiderWeb.Localization.GetFromLocLibrary(ObjectiveLocKey(oc), ""))) return false;
            return true;
        }

        /// <summary>
        /// Starts the next objective. If quest is complete, calls CompleteQuest.
        /// </summary>
        public void ProgressMade()
        {
            // If the quest is sequential, move to the next part of the sequence
            if (sequential) StartNextObjective();
            
            if (debug) Debug.Log(name + " checking all objectives for ones that require previous...");
            
            // Have objectives check their previous objectives
            foreach (var oc in objectiveCs)
            {
                
                // Check if it requires previous to be complete
                if (!oc.requirePrevious)
                {
                    if (debug) Debug.Log(oc.objective.name + " doesn't require previous. skipping...");
                    continue;
                }

                // Check if all the previous objectives have been complete
                if (!PreviousObjectivesComplete(oc))
                {
                    if (debug) Debug.Log(oc.objective.name + "'s previous objectives are not complete. Skipping...");
                    continue;
                }

                if (!oc.objective.IsOfStatus(QuestStatus.NotStarted, this))
                {
                    if (debug) Debug.Log(oc.objective.name + " is not of status ' notStarted', skipping...");
                    continue;
                }
                
                oc.objective.ProgressObjective(this);
                // Break, so that we only progress the first objective found.
                break;
            }

            if (IsComplete()) CompleteQuest();
        }

        /// <summary>
        /// Returns the index of the next non-complete objective.
        /// </summary>
        int IndexOfNextObjective()
        {
            if (DSave.current == null) return 0;
            if (!DSave.current.HasQuest(this)) return 0;
            DQuestSave qs = DSave.current.GetQuest(this);

            for (int i = 0; i < objectiveCs.Count; i++)
            {
                if (!qs.ObjectiveComplete(this, objectiveCs[i].objective)) return i;
            }

            return objectiveCs.Count;
        }

        /// <summary>
        /// Returns true if all the objectives are complete.
        /// </summary>
        public bool IsComplete()
        {
            // Check the save file first to see if this quest has already been completed
            if (DSave.current != null)
                if (DSave.current.IsQuestStatus(this, QuestStatus.Complete)) return true;

            // Check the status of each objective
            foreach (ObjectiveContainer oc in objectiveCs)
            {
                if (!oc.objective.IsComplete(this)) return false;
            }
            return true;
        }

        /// <summary>
        /// Completes this quest, shows GUI
        /// </summary>
        [Button]
        public void CompleteQuest()
        {
            DSave.current.CompleteQuest(this);

            // show UI for a completed quest
            if (showsQuestCompleteGui) DUIQuestShower.ShowQuest(this);
            onQuestComplete.Invoke();
            if (startAnotherQuest) questToStart.StartQuest();
        }

        #region Localization

        void RefreshLoc()
        {
            locTitle = GetLocTitle();
            locDescr = GetLocDescription();
        }

        [ButtonGroup("loc")]
        public string GetLocTitle()
        {
            return SpiderWeb.Localization.GetFromLocLibrary("quest_" + locKey + "_title", "_" + title, false);
        }

        [ButtonGroup("loc")]
        public string GetLocDescription()
        {
            return SpiderWeb.Localization.GetFromLocLibrary("quest_" + locKey + "_body", "_" + description, false);
        }

        public bool HasObjectiveDescription(out string descr, Objective forObjective)
        {
            descr = "";
            
            if (!HasObjective(forObjective)) return false;
            if (ContainerForObjective(forObjective).overrideName == false) return false;

            descr = LocalizedObjective(forObjective);
            return true;
        }


        [ButtonGroup("loc")]
        public void LocObjectives ()
        {
            foreach (ObjectiveContainer oc in objectiveCs)
            {
                if (oc.overrideName)
                {
                    oc.newName = LocalizedObjective(oc);
                }
            }
        }

        /// <summary>
        /// Is the given objective part of this quest?
        /// </summary>
        public bool HasObjective(Objective o)
        {
            foreach (ObjectiveContainer oc in objectiveCs)
                if (oc.objective == o) return true;

            return false;
        }

        /// <summary>
        /// Are all objectives leading up to the given objective complete?
        /// </summary>
        public bool PreviousObjectivesComplete(ObjectiveContainer oc)
        {
            if (oc == null) return false;
            if (!objectiveCs.Contains(oc)) return false;

            foreach (ObjectiveContainer o in objectiveCs)
            {
                // Don't compare against self
                if (o == oc) continue;
                
                if (!o.objective.IsComplete(this))
                {
                    Debug.Log(o.objective.name + " is not complete yet.");
                    return false;
                }
            }

            return true;
        }

        [ButtonGroup("loc")]
        public void AddToKeyLib()
        {
            string titleKey = "quest_" + locKey + "_title";
            string descrKey = "quest_" + locKey + "_body";

            SpiderWeb.Localization.AddToKeyLib(titleKey, title);
            SpiderWeb.Localization.AddToKeyLib(descrKey, description);

            foreach (ObjectiveContainer c in objectiveCs)
            {
                if (!c.overrideName) continue;
                SpiderWeb.Localization.AddToKeyLib(ObjectiveLocKey(c), c.newName);
            }

            RefreshLoc();
        }

        string ObjectiveLocKey(ObjectiveContainer oc)
        {
            return "quest_" + locKey + "_" + oc.LocKey();
        }

        public ObjectiveContainer ContainerForObjective(Objective obj)
        {
            foreach (var oc in objectiveCs)
            {
                if (oc.objective == obj) return oc;
            }
            return null;
        }

        public string LocalizedObjective(Objective objective)
        {
            if (!HasObjective(objective))return "[none]";

            return LocalizedObjective(ContainerForObjective(objective));
        }

        public string LocalizedObjective(ObjectiveContainer oc)
        {
            string key = ObjectiveLocKey(oc);
            return SpiderWeb.Localization.GetFromLocLibrary(key, "[" + oc.newName + "]");
        }
        #endregion
    }


    [System.Serializable]
    public class DQuestSave
    {
        public string key;
        public bool complete;
        public List<ObjectiveSave> objectives = new List<ObjectiveSave>();

        public DQuestSave(){}

        public DQuestSave(DQuest quest)
        {
            key = quest.name;
        }

        /// <summary>
        /// Constructor for converting old questsave into new
        /// </summary>
        public DQuestSave(QuestSave qs)
        {
            Debug.Log("converting old questsave '" + qs.locKey + "' to new...");
            string questObjName = QuestsGlobal.Get().NameForLocKey(qs.locKey);

            DQuest questObj = QuestsGlobal.GetQuest(questObjName);
            if (questObj == null)
            {
                Debug.Log("Quest couldn't be found!! " + qs.locKey);
                return;
            }

            Debug.Log("<color=green>Success!</color>", questObj);

            key = questObj.name;
            complete = qs.complete;
            for (int i = 0; i < questObj.objectiveCs.Count; i++)
            {
                //Create a new objective save file.
                ObjectiveSave os = questObj.objectiveCs[i].objective.CreateSave();

                // check if it's complete in the old save file
                if (qs.contitionStatus.Count > i)
                    os.complete = qs.contitionStatus[i];

                objectives.Add(os);
            }
        }
        
        /// <summary>
        /// Progresses the objective in the save file.
        /// </summary>
        public void ProgressObjective(Objective o)
        {
            if (!HasObjectiveSaved(o))
            {
                ObjectiveSave os = o.CreateSave();
                objectives.Add(os);
                return;
            }

            GetObjective(o).Progress();
        }

        /// <summary>
        /// Returns true of the objective has started but hasn't finished.
        /// </summary>
        public bool ObjectiveInProgress(DQuest q, Objective o)
        {
            if (!ObjectiveMatch(q, o)) return false;
            foreach (ObjectiveSave os in objectives)
                if (os.name == o.name && !os.complete) return true;

            return false;
        }

        /// <summary>
        /// Returns true of the objective has been completed.
        /// </summary>
        public bool ObjectiveComplete(DQuest q, Objective o)
        {
            if (!ObjectiveMatch(q, o)) return false;
            foreach (ObjectiveSave os in objectives)
                if (os.name == o.name && os.complete) return true;

            return false;
        }

        bool HasObjectiveSaved(Objective o)
        {
            foreach (ObjectiveSave os in objectives)
                if (os.name == o.name) return true;

            return false;
        }

        ObjectiveSave GetObjective(Objective o)
        {
            foreach (ObjectiveSave os in objectives)
                if (os.name == o.name) return os;

            return null;
        }

        /// <summary>
        /// Do the given quest and objective belong with this quest save?
        /// </summary>
        bool ObjectiveMatch(DQuest q, Objective o)
        {
            if (q.name != key) return false;
            if (!q.HasObjective(o)) return false;
            
            return true;
        }
    }
}