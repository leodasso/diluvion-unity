using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Sirenix.OdinInspector;
using Diluvion.SaveLoad;
using SpiderWeb;

namespace Diluvion
{
    /// <summary>
    /// A type of interior switch that supports saving, and unity events
    /// </summary>
    public class InteriorLever : InteriorSwitch
    {
        [ToggleLeft]
        public bool saveValue;

        [ShowIf("saveValue"), Indent]
        public SwitchSave save;

        [ToggleLeft]
        public bool oneTimeOnly;

        public UnityEvent switchedOn;
        public UnityEvent playerSwitchedOn;
        [Space]
        public UnityEvent switchedOff;
        public UnityEvent playerSwitchedOff;

        bool switched = false;

        protected override void Start ()
        {
            base.Start();

            StartCoroutine(FindSavedValue());
        }

        public override void OnPointerEnter()
        {
            if (locked)
            {
                QuestManager.Tick();
                return;
            }

            if (oneTimeOnly && switched) return;
            SetSprite(spriteSet.hover);
        }

        IEnumerator FindSavedValue()
        {
            while (DSave.current == null) yield return null;

            // Load the save value
            if (saveValue)
            {
                if (SwitchSave.GetSavedObject(save.uniqueID) != null)
                {
                    Debug.Log("Switch " + name + " reading saved value...", gameObject);

                    switched = true;
                    on = SwitchSave.GetSavedValue(save.uniqueID, on);

                    Debug.Log("... on: " + on);

                    Switched();
                }
                else Debug.Log("Switch " + name + " couldn't find a saved value.");
            }
        }

        public override void OnRelease ()
        {
            base.OnRelease();

            if (oneTimeOnly && switched) return;
            
            on = !on;
            Switched(true);
        }

        /// <summary>
        /// Gets called immediately after the value is switched. Calls the unity events on this component
        /// </summary>
        /// <param name="fromPlayerAction">Was this switch from player action, or was it just loading, etc</param>
        void Switched(bool fromPlayerAction = false)
        {
            if (on)
            {
                if (fromPlayerAction) playerSwitchedOn.Invoke();
                switchedOn.Invoke();
            }
            else
            {
                if (fromPlayerAction) playerSwitchedOff.Invoke();
                switchedOff.Invoke();
            }
            
            if (fromPlayerAction) SpiderSound.MakeSound("Play_Lever_Pull", gameObject);

            // Save the switches new value
            if (fromPlayerAction && saveValue)
            {
                save.value = on;
                save.SaveValue();
            }

            switched = true;
            
            SetSprite(SwitchSprite());
        }
    }
}