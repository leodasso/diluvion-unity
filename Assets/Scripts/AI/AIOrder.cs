using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Diluvion.Ships;
using Diluvion.AI;
namespace Diluvion
{

    //Add to Trigger volume to change the orders of the list of shipspawners to aiMission
    public class AIOrder : MonoBehaviour
    {
        //public AIMission aiMission = AIMission.Idling;

      //  public List<Captain> aiToOrder = new List<Captain>();
        public List<AgentSpawner> aisToSpawn = new List<AgentSpawner>();


        public void Awake()
        {
            foreach (AgentSpawner ss in aisToSpawn) { }//For the list of ship spawners, get the captains from the spawned ships
              //  ss.spawnShips += AddCaptain;

        }

        void OnTriggerEnter(Collider other)//On Trigger enter, and its player, set the order of the liste of captains to the aiMission
        {
            if (other.GetComponent<Bridge>())
                if (other.GetComponent<Bridge>().IsPlayer()) { }
                    //SetOrder();
        }

        /*    //Sets the order
           public void SetOrder()
           {
              foreach (Captain c in aiToOrder)
                   c.SetMission(aiMission);
            }

        //Get captains from the spawned ships
          public void AddCaptain(Captain c)
            {
                if (c == null) return;
                aiToOrder.Add(c);
            }
        */
    }
}