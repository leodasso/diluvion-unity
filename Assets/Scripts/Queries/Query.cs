using UnityEngine;
using System.Collections.Generic;
using HeavyDutyInspector;

namespace Queries
{
    public class Query : ScriptableObject
    {

        [Button("Test", "Test", true)]
        public bool hidden1;
        public Object testingObject;

        public virtual bool IsTrue(Object o) { return true; }
        public override string ToString() {
            return name;
        }

        protected virtual void Test(){   }

        /// <summary>
        /// If the given object is a game object, returns the game object. Otherwise returns null.
        /// </summary>
        protected GameObject GetGameObject(Object o)
        {
            if (o == null) return null;
            GameObject GO = o as GameObject;
            if (GO != null) return GO;

            //Debug.Log(o.name + " is not a game object.");
            return null;
        }

        public virtual List<StackedItem> ReferencedItems()
        {
            return null;
        }
    }
}