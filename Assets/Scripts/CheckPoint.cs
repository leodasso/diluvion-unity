using UnityEngine;
using System.Collections;
using HeavyDutyInspector;
using Diluvion.SaveLoad;
using Diluvion.Ships;

namespace Diluvion
{

    public class CheckPoint : MonoBehaviour
    {

        const float saveCooldown = 30;
        public static float lastTimeSaved;

        [Comment("Add a trigger collider to this component, and it will save state whenever the player enters the trigger. \n \n" +
            "Respawn position will default to this object's position if left blank.")]
        public Transform respawnPosition;

        [Comment("Animator on the checkpoint fish, so they can do a dance when saving. If left blank, will search children for animators.")]
        public Animator checkpointFish;

        static bool CanSave()
        {
           // Debug.Log("Last time saved: " + lastTimeSaved + " and current time: " + Time.unscaledTime);
            return (Time.unscaledTime - lastTimeSaved) > saveCooldown;
        }

        void Start()
        {
            gameObject.layer = LayerMask.NameToLayer("Tools");
            if (checkpointFish == null) checkpointFish = GetComponentInChildren<Animator>();
        }


        //Saving when player enters checkpoint
        void OnTriggerEnter(Collider other)
        {
            Save(other);
        }

        //And when player exits.   This way it saves when they leave a town, after they do all their fun stuff in the interior
        void OnTriggerExit(Collider other)
        {
            Save(other);
        }

        /// <summary>
        /// Returns the spawn position if available, otherwise returns the this position.
        /// </summary>
        public Transform SpawnPosition()
        {
            if (respawnPosition != null) return respawnPosition;
            return transform;
        }

        void Save(Collider other)
        {
            if (!CanSave()) return;
            Bridge otherBridge = other.GetComponent<Bridge>();
            if (otherBridge == null) return;
            if (!otherBridge.IsPlayer()) return;
            if (DSave.current == null) return;

            //DSave.current.SaveLocation(SpawnPosition());
            //DSave.Save();
            DSave.AutoSave();

            if (checkpointFish != null)
                checkpointFish.SetTrigger("save");
            
            lastTimeSaved = Time.unscaledTime;
        }
    }
}