using NodeCanvas.Framework;
using ParadoxNotion.Design;
using System.Collections.Generic;
using Diluvion.Ships;
using UnityEngine;

namespace Diluvion.AI{

	[Category("Diluvion")]
	[Description("Aims the input guns at a target")]
	public class AIAim : ActionTask<AIMono>
    {
        public BBParameter<Transform> target;
        public BBParameter<float> accuracy;
        public BBParameter<List<WeaponModule>> weaponsToFire;
        public BBParameter<List<WeaponSystem>> weaponSystems;

        string aimString = "";
        protected override string info
        {
            get
            {
                aimString = "Aiming with ";
                if (weaponsToFire.isNull||weaponsToFire.value.Count<1) return aimString += "EVERYTHING";
                foreach (WeaponModule ws in weaponsToFire.value)
                {
                    if (ws == null) continue;
                    aimString += ws.name;
                    if (weaponsToFire.value.Count > 1 && weaponsToFire.value.IndexOf(ws) != weaponsToFire.value.Count - 1)//if this is in a list with more than one weapon and its not the last one
                        aimString += ", ";                   
                }
                if (!target.isNull)
                    aimString += " at " + target.name;
                else
                    aimString += "at NOTHING";

                return aimString;
            }
        }

        protected override string OnInit()
        {
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
            
            //Debug.Log("Setting accuracy " + accuracy.value);
            foreach (WeaponSystem ws in weaponSystems.value)
            {
                ws.SetAccuracy(accuracy.value);
                if (ws.autoAimTarget != target.value)
                    if (AllowedToFire(ws))
                        ws.SetAutoAimTarget(target.value);
                     
            }


            EndAction(true);
		}
	}
}