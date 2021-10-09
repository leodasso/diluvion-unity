using UnityEngine;
using System;
using Diluvion.Ships;
using Sirenix.OdinInspector;

namespace Diluvion
{

    public class LocalAtmosphere : MonoBehaviour
    {
        public Atmosphere myAtmosphere;

        [Range(0, 20)]
        public float transitionTime = 2;
        public float priority = 0;
        LeagueManager leagueManager;


        [Button]
        void Refresh ()
        {
            LeagueManager.Get().EnterLocalAtmosphere(this);
        }

        // Use this for initialization
        void Start()
        {

            gameObject.layer = LayerMask.NameToLayer("Tools");

            Collider col = GetComponent<Collider>();
            if (col) col.isTrigger = true;
            leagueManager = LeagueManager.Get();
        }


        void OnTriggerEnter(Collider other)
        {

            if (!leagueManager) return;

            Bridge otherBridge = other.GetComponent<Bridge>();
            if (otherBridge == null) return;

            //Check if the other bridge is the player
            if (otherBridge.IsPlayer())
            {
                //Change atmosphere
                leagueManager.EnterLocalAtmosphere(this);
            }
        }

        void OnTriggerExit(Collider other)
        {

            if (!leagueManager) return;

            Bridge otherBridge = other.GetComponent<Bridge>();
            if (otherBridge == null) return;

            //Check if the other bridge is the player
            if (otherBridge.IsPlayer())
            {
                //Change atmosphere
                leagueManager.ExitLocalAtmosphere(this);
            }
        }

        void OnDisable()
        {
            //Change atmosphere
            if (leagueManager == null) return;
            leagueManager.ExitLocalAtmosphere(this);
        }

        void OnDestroy()
        {
            //Change atmosphere
            if (leagueManager == null) return;
            leagueManager.ExitLocalAtmosphere(this);
        }
    }
}