using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SpiderWeb;
using Sirenix.OdinInspector;
using HeavyDutyInspector;
using Diluvion.Ships;
using Diluvion.AI;

namespace Diluvion.Sonar
{

    public class SonarStats : MonoBehaviour
    {
        public LocTerm displayName;

        public delegate void PingCallback(GameObject bywo, PingResult why);
        public PingCallback pinged;
        public static List<SonarStats> allSonarStats = new List<SonarStats>();

        [AssetsOnly, AssetList(Path = "Prefabs/Sonar" )]
        public List<Signature> signatures = new List<Signature>();

        PseudoVelocity _velocity;

        public bool targetable = true;
        SonarStats statsIbelongTo;
        Bridge bridge;
        GameObject pingedEffect;
        AIMono _myAi;

        void OnEnable()
        {
            allSonarStats.Add(this);
        }

        void OnDisable()
        {
            allSonarStats.Remove(this);
        }

        /// <summary>
        /// Check if this SonarStatsBelongs to the input statsToCheck
        /// </summary>
        public bool BelongsTo(SonarStats statsToCheck)
        {
            if (statsIbelongTo == null) return false;
            if (statsToCheck == null) return false;
            if (statsIbelongTo != statsToCheck) return false;

            return true;
        }

        public int GetDanger()
        {
            if (!GetComponent<Bridge>()) return 0;
            return GetComponent<Bridge>().Danger();
        }
        
        AIMono MyAI()
        {
            if (_myAi != null) return _myAi;

            _myAi = GetComponent<AIMono>();
            if (_myAi == null)
                _myAi = GetComponentInParent<AIMono>();

            return _myAi;
        }

        public Bridge MyBridge()
        {
            if (bridge == null) bridge = GetComponent<Bridge>();
            if (bridge == null) bridge = GetComponentInParent<Bridge>();
            return bridge;
        }

        /// <summary>
        /// Called when this sonar stats is pinged. Calls the pinged delegate
        /// </summary>       
        public void Pinged(Listener byWho,PingResult why)
        {
            if (why == PingResult.Hail)
                Debug.Log(gameObject.name + " got a " + why + " by " + byWho.name);
            if (pinged != null)
                pinged(byWho.gameObject, why);
        }

        /// <summary>
        /// Sets the owner of the sonarStats
        /// </summary>
        public void SetOwner(Bridge b)
        {
            if (b == null) return;
            if (b.sonarSignature == null) return;
            statsIbelongTo = b.sonarSignature;
        }

        /// <summary>
        /// Adds the given signature to this sonar stats
        /// </summary>
        public void AddSignature(Signature s)
        {
            if (signatures.Contains(s)) return;
            signatures.Add(s);
        }

        public bool HasSignature(Signature s)
        {
            return signatures.Contains(s);
        }
        
        /// <summary>
        /// Do I have any of the signatures from the given list?
        /// </summary>
        public bool HasSignature(List<Signature> sigs)
        {
            foreach (Signature s in sigs)
                if (HasSignature(s)) return true;

            return false;
        }

        public bool HasFactionSignature(Signature s)
        {
            if (s == null) return false;
            if (!s.faction){ return false;}
            return signatures.Contains(s);
        }

        public bool HasFactionSignatures(List<Signature> ss)
        {
            foreach (Signature s in ss)
            {
                if (!HasFactionSignature(s)) continue;
                return true;
            }
            return false;
        }
        

        public Vector3 MyVelocity()
        {
            if (_velocity != null) return _velocity.velocity;
            
            if (GetComponent<PseudoVelocity>())
                _velocity = GetComponent<PseudoVelocity>();
            else
                _velocity = gameObject.AddComponent<PseudoVelocity>();

            return _velocity.velocity;
        }


        public bool HostileTorpedo()
        {
            /*
            muniToCheck = GetComponent<Munition>();
            if (muniToCheck)
                if (!muniToCheck.IsPlayerAmmo() && muniToCheck.myWeaponType == WeaponType.Torpedo) return true;
                */
            return false;
        }
        
        
       Munition muniToCheck;
        public bool IsHostileToPlayer(int level)
        {
            if (level <= 1) return false;         //Only level 2 signals past this point

            if (HostileTorpedo()) return true;
            if (GetComponent<ArkCreature>()) return true;
            if (!_myAi) return false;
            GameObject playerShip = PlayerManager.PlayerShip();
            if (!playerShip) return false;

          
            if (level <= 2) return false;//Only level 3 signals past this point   

            //we can make out if its a hostile sub even if its not attacking
            return false;
        }   
        

        //Spawns the effect of the player being pinged by other ships
        void SpawnPingedParticle(Vector3 direction, float power)
        {
            Quaternion pointParticle = Quaternion.LookRotation(-direction.normalized);
            //if (pingedEffect == null)
            //    pingedEffect = pingedEffect = bridge.sonar.sonarPingObj.pingedEffect;

            GameObject thisParticle = Instantiate(pingedEffect, transform.position, pointParticle) as GameObject;
            //WWISE TODO Set ping noise based on the incoming ping's power      

            thisParticle.transform.SetParent(transform);
            Destroy(thisParticle, 3);
        }

        List<Signature> orderedSigs = new List<Signature>();
        public List<Signature> OrderedSignatures()
        {
            orderedSigs = signatures.Where(x => x != null).ToList();
            orderedSigs = orderedSigs.OrderBy(x => x.revealStrengh).ToList();
            return orderedSigs;
        }
    }
}