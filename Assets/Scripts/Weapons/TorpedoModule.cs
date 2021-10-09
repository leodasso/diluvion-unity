using UnityEngine;
using System.Collections;
using Diluvion;
using Diluvion.Sonar;
using FluffyUnderware.Curvy;
using NodeCanvas.BehaviourTrees;

namespace Diluvion.Ships
{

    /// <summary>
    /// A weapon module that allows for firing of torpedoes! Torpedoes can be charged up for more explosive power
    /// </summary>
    [CreateAssetMenu(fileName = "torpedo module", menuName = "Diluvion/subs/modules/torpedo")]
    public class TorpedoModule : WeaponModule
    {
        [Tooltip("How long it takes to charge up a torpedo with no modifications")]
        public float chargeTime = 3;

        [Tooltip("How long it takes to reload a torpedo with no modifications")]
        public float reloadTime;
       
        public override void UpdateSystem(WeaponSystem ws)
        {
            base.UpdateSystem(ws);
            
            UpdateTorpedoTubes(ws);
            
            // Find the next active tube
            if (ws.activeTube == null)
            {
                foreach (var tube in ws.allTorpedoTubeInstances)
                {
                    if (tube.ReadyToCalibrate())
                    {
                        ws.activeTube = tube;
                        break;
                    }
                }
            }
            
            // If not firing, no further action needed!
            if (!ws.firing) return;

            // Calibrate the active tube. If it's done calibrating, remove it from 'active tube' ref
            if (ws.activeTube != null)
            {
                ws.activeTube.CalibrationUpdate((ws.calibrationSpeed / chargeTime) * Time.deltaTime);
                if (ws.activeTube.calibrationProgress >= 1)
                    ws.activeTube = null;
            }

            // fire torpedoes once charge is complete for all tubes
            if (AllTubesCharged(ws)) FireReadyTorpedoes(ws);
        }

        bool AllTubesCharged(WeaponSystem ws)
        {
            foreach (TorpedoTube tube in ws.allTorpedoTubeInstances)
            {
                if (tube.tubeState != TorpedoTube.TubeState.Calibrating) return false;
                if (tube.calibrationProgress < .98f) return false;
            }

            return true;
        }


        /// <summary>
        /// Are there any torpedo tubes which have a torpedo ready to detonate?
        /// </summary>
        public bool DetonateReady(WeaponSystem ws)
        {
            foreach (var tube in ws.allTorpedoTubeInstances)
            {
                if (tube.DetonateReady()) return true;
            }

            return false;
        }


        bool _reloadedTube;
        void UpdateTorpedoTubes(WeaponSystem ws)
        {
            // Reloading can only happen on one tube per update, so this lets us know if we've already
            // performed reload stuff on this frame
            _reloadedTube = false;
            
            foreach (var tube in ws.allTorpedoTubeInstances)
            {
                // If the tube is already reloading, continue reloading
                if (tube.tubeState == TorpedoTube.TubeState.Reloading && !_reloadedTube)
                {
                    tube.ReloadUpdate(ws.reloadSpeed / reloadTime);
                    _reloadedTube = true;
                }

                // If the tube is empty, we have to try and fill it with another torpedo from the inventory
                if (tube.tubeState == TorpedoTube.TubeState.Empty && !_reloadedTube)
                {
                    if (TrySpendAmmo(ws))
                    {
                        tube.ReloadUpdate(ws.reloadSpeed / reloadTime);
                        _reloadedTube = true;
                    }
                }

                // Tell each tube if it's the weapon system's current active tube
                tube.isActiveTube = tube == ws.activeTube;
                
                // Have the tube update its spline
                tube.UpdateSpline(ws.GetSystemTarget(), ws.CleanAimPosition());
            }
        }

        /// <summary>
        /// Fire a torpedo, setting its target and charge amount appropriately.
        /// </summary>
        public override GameObject FireWeapon(Mount mount, WeaponSystem ws)
        {
            GameObject torp = base.FireWeapon(mount, ws);
            if (torp == null)return null;

            // set the torpedo's target
            SplineTorpedo splineTorpedo = torp.GetComponent<SplineTorpedo>();
            if (splineTorpedo == null) return null;
            return torp;
        }
        
        
        void FireReadyTorpedoes(WeaponSystem ws)
        {
            if (ws.inFiringSequence) return;
            ws.inFiringSequence = true;
            ws.StartCoroutine(FireTorpedoesSequence(ws));
        }

        IEnumerator FireTorpedoesSequence(WeaponSystem ws)
        {     
            Debug.Log("Beginning fire torpedo sequence. Weapon system in firing sequence: " + ws.inFiringSequence);
            
            foreach (var m in ws.mounts)
            {
                FireWeapon(m, ws);
                yield return new WaitForSeconds(.3f);
            }
            
            yield return new WaitForSeconds(1);

            ws.activeTube = null;
            ws.inFiringSequence = false;
            
            Debug.Log("Ending firing sequence.");
        }

        void DetonateTorpedoInstances(WeaponSystem ws)
        {
            foreach (var tube in ws.allTorpedoTubeInstances) 
                tube.Detonate();
        }
        

        protected override void OnInputDown(Bridge b)
        {
            var ws = RelatedWeaponSystem(b);
            
            // If there's any torpedoes ready to detonate, detonate them
            if (DetonateReady(ws))
            {
                Debug.Log("Torpedoes are ready to detonate!");
                // detonate the torpedoes
                DetonateTorpedoInstances(ws);
                return;
            }
            
            base.OnInputDown(b);
        }
        
        protected override void OnInputUp(Bridge b)
        {
            base.OnInputUp(b);
            //RelatedWeaponSystem(b).FireReadyTorpedoesSequence();
            FireReadyTorpedoes(RelatedWeaponSystem(b));
        }
        
        public override void AddToShip(Bridge bridge, ShipModuleSave data = null)
        {
            base.AddToShip(bridge, data);

            foreach (var tube in RelatedWeaponSystem(bridge).allTorpedoTubeInstances)
            {
                tube.ReloadFull();
            }
        }
    }
}