using UnityEngine;
using System.Collections.Generic;
using Diluvion.SaveLoad;
using Diluvion;

namespace Quests
{
    public abstract class Objective : ScriptableObject
    {
        /// <summary>
        /// Checks the objective, an if it's met (relative to the given quest) will then call SetComplete.
        /// </summary>
        public abstract void CheckObjective(DQuest forQuest);

        /// <summary>
        /// Progresses the objective by 1 unit. If this is a quest with multiple units (i.e. destroy 5 ships) it will increase that by one.
        /// </summary>
        public virtual void ProgressObjective(DQuest forQuest) 
        {
            if (DSave.current == null) return;

            if (IsOfStatus(QuestStatus.Complete, forQuest))
            {
                if (forQuest.debug) Debug.Log("Objective " + name + " is already complete for " + forQuest.name);
                return;
            }

            // Check if the quest is part of the save file
            if (!forQuest.IsOfStatus(QuestStatus.InProgress))
            {
                if (forQuest.debug) Debug.Log("Quest " + forQuest.name + " is not currently in progress. Can't progress.");
                return;
            }
            
            // Check if the previous objectives are complete if this objective requires previous to be complete in the given quest.
            ObjectiveContainer oc = forQuest.ContainerForObjective(this);
            if (oc != null)
            {
                if (oc.requirePrevious)
                {
                    // If the previous objectives aren't complete, return.
                    if (!forQuest.PreviousObjectivesComplete(oc))
                    {
                        if (forQuest.debug) Debug.Log("Previous objectives in " + forQuest.name + " aren't compplete.");
                        return;
                    }
                }
            }

            if (forQuest.debug) Debug.Log("Progressing objective " + name + " " + forQuest.name);

            // If this objective is just now starting, notify that a waypoint has been added.
            if (IsOfStatus(QuestStatus.NotStarted, forQuest))
            {
                //Debug.Log("Waypoint added for " + name, this);
                // Show 'quest updated' gui, but not if it's just for adding an objective
                DUI.QuestUpdateLog.ShowUpdate(forQuest, this);
            }
            else
            {
                
            }

            DSave.current.SaveObjectiveProgress(forQuest, this);
            
            if (forQuest.debug) Debug.Log(forQuest.name + " objective " + name + " is complete: " + IsComplete(forQuest));

            forQuest.ProgressMade();

            QuestManager.Tick();
        }

        /// <summary>
        /// Returns any items that may be used by this objective
        /// </summary>
        public virtual List<StackedItem> ReferencedItems()
        {
            return null;
        }

        public bool IsComplete(DQuest forQuest)
        {
            if (DSave.current == null) return false;
            return DSave.current.IsObjectiveComplete(forQuest, this);
        }

        /// <summary>
        /// Checks if this objective as part of the given quest is the given status.
        /// </summary>
        public bool IsOfStatus(QuestStatus status, DQuest forQuest)
        {
            if (DSave.current == null) return false;

            if (!DSave.current.HasQuest(forQuest))
            {
                return status == QuestStatus.NotFinished || status == QuestStatus.NotStarted;
            }

            DQuestSave qSave = DSave.current.GetQuest(forQuest);
            if (status == QuestStatus.Complete)
            {
                if (qSave.complete) return true;
                if (qSave.ObjectiveComplete(forQuest, this)) return true;
                return false;
            }

            if (status == QuestStatus.InProgress && qSave.ObjectiveInProgress(forQuest, this))
                return true;

            if (status == QuestStatus.NotFinished && !qSave.ObjectiveComplete(forQuest, this))
                return true;

            if (status == QuestStatus.NotStarted && !qSave.ObjectiveComplete(forQuest, this) &&
                !qSave.ObjectiveInProgress(forQuest, this)) return true;

            if (status == QuestStatus.InProgOrComplete)
            {
                if (qSave.complete) return true;
                if (qSave.ObjectiveInProgress(forQuest, this)) return true;
                if (qSave.ObjectiveComplete(forQuest, this)) return true;
            }

            return false;
        }


        public bool IsBeyondStatus(QuestStatus status, DQuest forQuest)
        {
            if (DSave.current == null) return false;

            DQuestSave qSave = DSave.current.GetQuest(forQuest);
            if (qSave.complete) return true;
            if (qSave.ObjectiveComplete(forQuest, this)) return true;

            if (status == QuestStatus.InProgress || status == QuestStatus.NotFinished )
                if (qSave.ObjectiveInProgress(forQuest, this)) return true;

            if (status == QuestStatus.NotStarted) return true;

            return false;
        }

        /// <summary>
        /// Returns a new objective save created from the properties of this objective.
        /// </summary>
        public virtual ObjectiveSave CreateSave()
        {
            ObjectiveSave newSave = new ObjectiveSave();
            newSave.GetObjectiveProperties(this);
            return newSave;
        }

        public abstract GameObject CreateGUI(string overrideObjectiveName);
    }

    [System.Serializable]
    public class ObjectiveSave
    {
        public string name;
        public bool complete = false;
        public int progress = 0;

        public virtual void GetObjectiveProperties(Objective o)
        {
            name = o.name;
        }

        /// <summary>
        /// Call this when progress is made ont he quest
        /// </summary>
        public virtual void Progress()
        {
            complete = true;
            QuestManager.Tick();
        }
    }
}
