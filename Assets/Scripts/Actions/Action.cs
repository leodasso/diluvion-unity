using UnityEngine;
using System.Collections;
using Sirenix.OdinInspector;

namespace Diluvion
{
    public abstract class Action : ScriptableObject
    {
      
        public Object testObject;


        public abstract bool DoAction(Object o);

        public abstract override string ToString();

        /// <summary>
        /// If the given object is a game object, returns the game object. Otherwise returns null.
        /// </summary>
        protected GameObject GetGameObject(Object o)
        {
            Component c = o as Component;
            if (c) return c.gameObject;

            GameObject GO = o as GameObject;
            if (GO != null) return GO;

            Debug.Log(o.name + " is not a game object.");
            return null;
        }

        [Button]
        protected abstract void Test();

        /// <summary>
        /// Does this action give or start a quest?
        /// </summary>
        public virtual bool GivesQuest()
        {
            return false;
        }
    }
}
