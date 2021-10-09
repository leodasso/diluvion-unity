using UnityEngine;
using System.Collections;
using System;

namespace Diluvion.Actions
{
    [CreateAssetMenu(fileName = "check charts", menuName = "Diluvion/actions/check charts")]
    /// <summary>
    /// Opens up the cartographer's GUI panel and check's the player's landmark charts.
    /// </summary>
    public class CheckCharts : Action
    {
        public override bool DoAction(UnityEngine.Object o)
        {
            GameObject GO = GetGameObject(o);
            Cartographer carto = GO.GetComponent<Cartographer>();

            if (carto == null) return false;
            Inventory playerInv = PlayerManager.PlayerInventory();
            if (playerInv == null) return false;

            Debug.Log("Checking player's landmark charts!");

            carto.CheckCharts();
            return true;
        }

        protected override void Test()
        {
            Debug.Log(ToString());
            DoAction(testObject);
        }

        public override string ToString()
        {
            return "Check player's landmark charts.";
        }
    }
}
