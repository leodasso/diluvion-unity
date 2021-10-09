using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Diluvion.Roll
{
    /// <summary>
    /// Parent class for Random encounter types (ships, creatures, etc)
    /// </summary>    
    public abstract class RandomEncounter : SpawnableEntry, IRoller
    {      


        public virtual bool RollQuery(Entry checkedObject)
        {
            SpawnableEntry se = checkedObject as SpawnableEntry;
            if (se == null) return false;

            if (!se.CanAfford(resourceCost))
                return false;

            return checkedObject.AllTagsTrue(this);
        }

        List<Tag> tempTags = new List<Tag>();
        /// <summary>
        /// Returns the working tags, can be manipulated
        /// </summary>
        public List<Tag> RollingTags
        {
            get
            {
                if (tempTags.Count < 1)
                {
                    tempTags = tags;
                }
                return tempTags;
            }
            set { tempTags = value; }
        }

        /// <summary>
        /// Safe add tags makes sure no overlap is happening in the tag list
        /// </summary>
        public void CombineTagList(List<Tag> ts)
        {
            RollingTags = new List<Tag>(tags);
            foreach (Tag t in ts)
            {
                if (!tags.Contains(t))
                    RollingTags.Add(t);
            }
        }
    }
}
