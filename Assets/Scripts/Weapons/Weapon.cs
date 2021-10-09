/*
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using PathologicalGames;
using HeavyDutyInspector;
using System.Linq;
using SpiderWeb;
using Diluvion.Ships;

namespace Diluvion
{

    [System.Serializable]
    public class WeaponStats
    {
        [Comment("Green stats will be applied to ammo when clicking 'set ammo'")]
        [Background(ColorEnum.Green)]
        public int damage = 2;
        public int burstSize = 1;

        [Background(ColorEnum.Green)]
        public float shotSpeed = 10;
        public float shotSpread = 3;

        [Background(ColorEnum.Green)]
        public float range = 100;
        public string sfx = "bolt";
        public float volume = 2;
        public int dangerRating = 1;
        public float cooldown = 1;

        public WeaponStats()
        {
        }
    }

    public class Weapon : ShipPart
    {
        public int damage = 1;
        public string fireSound = "";
        public float cooldown = 1;

        [Button("SetAmmo", "SetMunitionStats", true)]
        public bool setAmmoStats;

        [SerializeField]
        public WeaponStats stats;

        public Munition ammo;
        public Loot.DItem ammoItem;
        public bool dontShootOwnShip;
        public float shakeAmt = 1;
        public float recoilForce = 45;
        public GameObject fireParticle;
        public bool ignoreInheritedForce;
        public Vector3 torpedoInstability;
        public Loot.DItemWeapon item;

        public enum TorpedoFails { EarlyDestruct, SpinOut };

        float burstTime = 0.1f;
        float realNoise;
        Transform shot;
        bool proximityUpgrade = false;
        float proximityRange = 5;
        protected Mount parentMount;
        Bridge myBridge;
        List<Hull> myHulls = new List<Hull>();
        float cooldownTimer = 0;
        LayerMask hitMask;

        #region initialize


        public virtual void Start()
        {
            ParentMount();
            hitMask = Calc.GunsafeLayer();
        }

        Mount ParentMount()
        {
            if (parentMount != null) return parentMount;
            parentMount = GetComponentInParent<Mount>();
            return parentMount;
        }

        GameObject FireParticle()
        {
            if (fireParticle != null) return fireParticle;
            fireParticle = Resources.Load("effects/bolt_fire_effect") as GameObject;
            return fireParticle;
        }


        public virtual void OnSpawned()
        { }

        #endregion

        public float NormalizedCooldown()
        {
            return 1 - cooldownTimer / stats.cooldown;
        }


        Bridge MyBridge()
        {
            if (myBridge) return myBridge;
            myBridge = FindBridge();
            return myBridge;
        }

        void Update()
        {

            if (cooldownTimer > 0)
                cooldownTimer -= Time.deltaTime;
        }

        Transform firePart;
        public void FireFX()
        {
            //shake the camera
            OrbitCam.Get().Shake(shakeAmt, transform.position);

            if (GetComponent<AKTriggerCallback>())
                GetComponent<AKTriggerCallback>().Callback();

            //Spawn fire particles
            firePart = GameManager.Pool().Spawn(FireParticle().transform, transform.position + transform.forward, transform.rotation);
            GameManager.Pool().Despawn(firePart, 2);
        }

        //Caching get method for getting all the hulls this weapon should ignore
        List<Hull> GetIgnoreHulls()
        {
            if (myHulls.Count > 0) return myHulls;
            if (MyBridge() == null) return null;
            myHulls = MyBridge().GetAllHulls();
            return myHulls;
        }

        #region FireActual Scope Variables
        /// <summary>
        /// This is minor, but saves a few .1ms per bullet, which happens a lot
        /// declaring them outside scope prevents construction in some cases, which also saves space in the GC
        /// </summary>
        Munition shotMunition;
        Rigidbody shotRB;
        Rigidbody parentRB;
        Targeting shotTarget;
        Vector3 fireSpreadVector;
        Vector3 inaccurateForward;
        #endregion

        protected void Shoot()
        {
        }

        //Function that fires the munition with the inherited force from the ship and the accuracy(0.1-2)
        void FireActual(Vector3 inheritedForce, float cooldown, float torpCharge = 1)
        {

            // Debug.DrawRay(transform.position, transform.up * TotalInaccuracy(), Color.green, 5);

            FireFX();

            // Find the parent rigidbody, if there is one
            if (parentRB == null) parentRB = GetComponentInParent<Rigidbody>();

            // Add force to rg
            if (parentRB)
                parentRB.AddForceAtPosition(transform.TransformDirection(Vector3.forward) * -recoilForce, transform.position);

            //spawn the ammo
            shot = GameManager.Pool().Spawn(ammo.transform, transform.position, transform.rotation);
            shotMunition = shot.GetComponent<Munition>();
            shotRB = shot.GetComponent<Rigidbody>();
            shotTarget = shot.GetComponent<Targeting>();

            //shotMunition.SetOwner(MyBridge());
            //shotMunition.LearnHull(GetIgnoreHulls(), MyBridge().IsPlayer());

            // Shotspread is the least amount of distance the shot can deviate, 
            // higher accuracy brings it down to the shotspread
            //fireSpreadVector = Random.insideUnitCircle * GetParentWeaponRoom().CurrentAccuracySize();
            shot.transform.rotation = transform.rotation;

            //Torpedo behaviors   
            /*
            if (myWeaponType == WeaponType.Torpedo)
            {
                //launch direction, not neccesarily the same as the travel direction
                Vector3 launchDir = transform.TransformDirection(ParentMount().launchDir.normalized * 10);

                // Build the inaccuracy vector 
                inaccurateForward = launchDir;

                // Set velocity and torque
                shotRB.velocity = (stats.shotSpeed * inaccurateForward.normalized) + inheritedForce;

                // Set damage based on charge amount
                float min = .5f;
                float max = 1;
                shotMunition.damageMultiplier = Mathf.Lerp(min, max, torpCharge);

                if (parentRB)
                    parentRB.AddForceAtPosition(transform.TransformDirection(Vector3.forward) * -recoilForce * 15 * torpCharge, transform.position);

                //if the weapon room is tracking a target
                /* TODO
                if (ParentMount())
                    if (ParentMount().myWeaponRoom)
                        if (ParentMount().myWeaponRoom.targetObj != null)//feed target info into the torpedo			
                            shotTarget.target = ParentMount().myWeaponRoom.targetObj;
                            

            }
            else
            //behavior for all non-torpedos 
            {
                //  Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward * stats.range), Color.red, 5);
                // Debug.DrawRay(transform.position,inaccurateForward.normalized*5, Color.cyan, 5);

                // Build the inaccuracy vector (THIS PUNISHES LOW RANGE WEAPONS)
                inaccurateForward = transform.TransformDirection(Vector3.forward * stats.range + fireSpreadVector);

                shotRB.velocity = inaccurateForward.normalized * stats.shotSpeed + inheritedForce;
            }
    **

            if (shotTarget != null)
            {
                if (shotTarget.enabled)
                {

                    shotTarget.chargeAmount = torpCharge;

                    // Set torpedo success chance based on shot gamble
                    float lowGambleChance = 1;
                    float highGambleChance = 2;//parentMount.myWeaponRoom.TorpedoSuccessChance();

                    shotTarget.successChance = Mathf.Lerp(lowGambleChance, highGambleChance, torpCharge);
                    shotTarget.StartCoroutine("Arming");
                }
            }

            shotRB.angularVelocity = Vector3.zero;
            cooldownTimer = cooldown;
        }


        //Sets burst and adjusts the fire rate accordingly
        public void SetBurst(int burstSize, bool adding)
        {
            if (adding)
            {
                stats.burstSize = burstSize;
                burstTime = 0.1f;
            }
            else
            {
                stats.burstSize = 1;
                burstTime = 0.1f;
            }
        }

        //Burst Ienumerator
        IEnumerator BurstShot(Vector3 iForce, float cooldown, float chargeAmt = 1)
        {
            for (int i = 0; i < stats.burstSize; i++)
            {
                FireActual(iForce, cooldown, chargeAmt);
                yield return new WaitForSeconds(burstTime);
            }
        }

        Vector3 shotForce;
        public virtual bool Fire(Vector3 inheritedForce, float cooldown, float chargeAmt = 1)
        {
            if (!gameObject.activeInHierarchy) return false;

            if (dontShootOwnShip && AmShootingOwnShip()) return false;

            if (ignoreInheritedForce) shotForce = Vector3.zero;
            else shotForce = inheritedForce;

            //Cooldown Check
            if (CanFire())
            {
                StartCoroutine(BurstShot(shotForce, cooldown, chargeAmt));
                return true;
            }
            else
                return false;
        }

        protected bool IsThisPartOfMyShip(Collider col)
        {

            if (MyBridge() == null) return false;
            if (col == null) return false;

            Bridge br = col.gameObject.GetComponentInParent<Bridge>();

            if (br == MyBridge()) return true;
            else return false;
        }


        /// <summary>
        /// Raycasts out in fire path to see if I'll hit my own ship when firing. 
        /// Returns true if aiming at my own ship.
        /// </summary>
        protected bool AmShootingOwnShip()
        {
            //Create a ray where I'm pointing
            Ray impactRay = new Ray(transform.position, transform.forward * 200);

            // Create an ordered list of all hits
            RaycastHit[] hits = Physics.RaycastAll(impactRay, 200, hitMask.value);
            List<RaycastHit> sortedHits = hits.OrderBy(h => h.distance).ToList();

            // Remove triggers
            foreach (RaycastHit h in hits)
                if (h.collider.isTrigger) sortedHits.Remove(h);

            // Get the first thing the ray is hitting
            Collider firstCol = null;
            foreach (RaycastHit h in sortedHits)
            {

                if (h.collider != null)
                {
                    firstCol = h.collider;
                    break;
                }
            }

            if (firstCol == null) return false;

            if (IsThisPartOfMyShip(firstCol)) return true;
            else return false;
        }

        bool CanFire()
        {
            if (cooldownTimer > 0) return false;
            else return true;
        }
    }
}
*/