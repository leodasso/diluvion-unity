using NodeCanvas.Framework;
using ParadoxNotion.Design;
using System.Collections.Generic;
using Diluvion.Ships;
using UnityEngine;

namespace Diluvion.AI
{

	[Category("Diluvion")]
	[Description("Fires Selected guns that are in range")]
	public class AIFire : ActionTask<AIMono>
    {

        public BBParameter<List<WeaponModule>> weaponsToFire;
        public BBParameter<float> targetDistance;
        public BBParameter<float> reloadMultiplier;
        public BBParameter<List<WeaponSystem>> weaponSystems;
        public BBParameter<bool> fireMode;
        public BBParameter<float> aimPatience = 5;


        string firingString = "";
        protected override string info
        {
            get
            {
                if (fireMode.value)
                    firingString = "Turn Firing On";
                else
                    firingString = "Turn Firing Off";

                firingString += " for ";
                if (weaponsToFire.isNull||weaponsToFire.value.Count<1) return firingString += "EVERYTHING";
                foreach (WeaponModule ws in weaponsToFire.value)
                {
                    if (ws == null) continue;
                    firingString += ws.name;
                    if (weaponsToFire.value.Count > 1 && weaponsToFire.value.IndexOf(ws) != weaponsToFire.value.Count - 1)//if this is in a list with more than one weapon and its not the last one
                        firingString += ", ";
                    else
                        firingString += ".";
                }               

                return firingString;
            }
        }

        protected override string OnInit(){
			return null;
		}

        //Checks to see if this behaviour should fire the weapon, an empty input list = defaults to firing everything
        bool AllowedToFire(WeaponSystem ws)
        {
            if (weaponsToFire.isNull) return true;
            if (weaponsToFire.value.Count < 1) return true;
            return weaponsToFire.value.Contains(ws.module);
        }

        protected override void OnExecute()
        {
            if (weaponSystems.isNull) { EndAction(false); return; }
            
           // Debug.Log("Setting reloadValue " + reloadMultiplier.value);
            foreach (WeaponSystem ws in weaponSystems.value)
                if (AllowedToFire(ws))
                {
                    //If waverAmount drops below my patience value i will fire, otherwise i will wait
                    if (fireMode.value)
                    {
                        if(ws.Range()>targetDistance.value)
                            ws.FireOn(reloadMultiplier.value);
                        else
                            ws.FireOff();
                    }
                    else
                        ws.FireOff();
                }         

			EndAction(true);
		}
	}
}