using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using SpiderWeb;
using DUI;
using Diluvion.Ships;
using Sirenix.OdinInspector;
using Random = UnityEngine.Random;

namespace Diluvion
{

    public class Hull : MonoBehaviour, IHUDable, IDamageable
    {
        [TabGroup("Health")]
        [OnValueChanged("CheckMaxHealth")]
        public float maxHealth = 10;

        void CheckMaxHealth()
        {
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        }

        [TabGroup("Health")]
        [MinValue(0)]
        [OnValueChanged("CheckMaxHealth")]
        public float currentHealth = 10;

        [TabGroup("Health")] public bool invincible;

        [TabGroup("general")]
        public bool useCrushDepth = true;

        [TabGroup("general")]
        [ShowIf("useCrushDepth"), LabelText("crush depth")]
        public float testDepth = -300; // Official depth you can get to before you start taking hull damage

        [TabGroup("general")]
        public bool collisionDamage = true;
        [TabGroup("general")]
        public bool bubbleBleed = true;
        
        public event Action<GameObject> onKilled;

        public delegate void ImHit (float howmuch, GameObject byWhat);
        public ImHit imHit;
        public delegate void LostHealth (float remainingHealth, float remainingPercentage);
        public LostHealth lostHealth;

        [TabGroup("general")]
        public bool stopCollisionEffect = false;

        [TabGroup("general")]
        public Transform crippledModel;

        [TabGroup("general")]
        public Transform explodedModel;

        [TabGroup("Health")]
        public float defense = 1;   // greater number increases chance of blocking critical hits

        public delegate void DeathCall (Hull hullThatDied, string killer = "");
        public DeathCall myDeath;

        public delegate void Defeated ();
        public Defeated defeatedByPlayer;

        [TabGroup("general")]
        public GameObject sparksParticle;
        float _crushDepthTimer;
        bool _groanSound = true;
        float _recentDamage;
        bool _killed;
        bool _avoiding;
        bool _collisionVulnerable = true;            //if collision damamge is taken, there's a few second cooldown before it can be taken again
        float colDamageCooldown = 3;
        Vector3 _tempVel;

        SurfaceMaterial _surface;
        float crushDepthDmgChance = .3f;
        float crushDepthMinDelay = .5f;
        float crushDepthMaxDelay = 1;
        Rigidbody rb;

        List<DUIPanel> createdUI = new List<DUIPanel>();
        DUIPanel _damageOverlay;

        [TabGroup("Debug")]
        public float debugDamage = 5;
        [TabGroup("Debug")]
        public float critChance = 0.1f;
        [TabGroup("Debug")]
        public GameObject source;
        
        [TabGroup("Debug")]
        [Button]
        void DebugDamage()
        {
            Damage(debugDamage, critChance, source);
        }
        
        void Start ()
        {
            rb = GetComponent<Rigidbody>();
            if (!rb) rb = GetComponentInParent<Rigidbody>();
           
            //myDeath += Explode;
            _crushDepthTimer = 0;
            _surface = Resources.Load("ship_surface") as SurfaceMaterial;
            if (bubbleBleed)
            {
                if(!GetComponent<BubbleBleeder>())
                    gameObject.AddComponent<BubbleBleeder>();
            }
        }

        void OnSpawned ()
        {
            ResetHull();
        }

        void Update ()
        {
            //get current depth of ship
            float depth = transform.position.y;

            //damage if below crush depth
            if (_crushDepthTimer < crushDepthMaxDelay)
                _crushDepthTimer += Time.deltaTime;

            //Roll for crush depth damage
            if (depth < testDepth && useCrushDepth)
            {
                PlayDepthAlarmn();
                RollForCrushDepthDamage(depth);
            }
            else StopDepthAlarm();
        }


        public void CreateUI ()
        {
            // Add damage overlay
            _damageOverlay = UIManager.Create(UIManager.Get().damageOverlay);
            _damageOverlay.SetAlpha(0);
            createdUI.Add(_damageOverlay);

            // add the HP panel
            HPPanel hpPanel = UIManager.Create(UIManager.Get().HPPanel as HPPanel);
            hpPanel.InitHullPanel(this);
            createdUI.Add(hpPanel);
        }

        public void RemoveUI ()
        {
            foreach (DUIPanel p in createdUI) p.End();
            createdUI.Clear();
        }


        public bool Avoiding ()
        {
            return _avoiding;
        }

        public void Avoiding (bool avoid)
        {
            _avoiding = avoid;
        }

        
        /// <summary>
        /// Inserts the newCall at the front of the invokation list, rather than the end, will add normally if it sthe first item
        /// </summary>
        public void MethodBeforeDeath (DeathCall newCall)
        {
            if (myDeath == null)
            {
                Debug.Log("Adding " + newCall + " to deathCall");
                myDeath += newCall; 
                return;
            }
            
            System.Delegate[] delegates = myDeath.GetInvocationList();
            myDeath = null;
            Debug.Log("adding newCall to number " + delegates.Length+1 + " in the death list" );
            myDeath += newCall;
            foreach (DeathCall d in delegates)
                myDeath += d;
        }

        public void Invincibility (bool enabled)
        {
            invincible = enabled;
            useCrushDepth = !enabled;
        }

        /// <summary>
        /// Set collision damage enabled / disabled.
        /// </summary>
        public void CollisionDamage (bool enabled)
        {
            collisionDamage = enabled;
        }

        public void ResetHull ()
        {
            _crushDepthTimer = 0;
            currentHealth = maxHealth;
            _killed = false;
        }


        public void Impact (Vector3 impactDir, Vector3 impactPoint)
        {
            rb?.AddForceAtPosition(impactDir, impactPoint);
            if (!_surface) return;
            if (!_surface) return;
            _surface.NewImpact(impactDir, impactPoint).transform.parent = transform;
        }


        public void SelfDestruct ()
        {
            Die(gameObject);
        }

        /// <summary>
        /// Adds the given amount of HP.
        /// </summary>
        /// <returns>If HP is at max, returns false. Otherwise, true.</returns>
        public bool Repair (float HP)
        {
            if (currentHealth >= maxHealth) return false;
            
            // instantiate the glowy green repair particle
           Debug.Log("DESPAWNING FROM:" + this.name , this); Destroy(Instantiate(SubChassisGlobal.HealParticle(), transform.position, transform.rotation, transform), 5);

            StartCoroutine(DelayedRepair(1.7f, HP));

            return true;
        }

        IEnumerator DelayedRepair(float delayTime, float HP)
        {
            yield return new WaitForSeconds(delayTime);
            RemoveHealth(-HP);
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

            //TAudio heal
            SpiderSound.MakeSound("Play_Repaired", gameObject);
        }

        void OnDisable ()
        {
            if (!Application.isPlaying) return;
            StopCriticalHPSiren();
            SpiderSound.MakeSound("Stop_DepthAlarm", gameObject);
            SpiderSound.MakeSound("Stop_Depth_Creak", gameObject);

            //if (PlayerManager.PlayerShip() == gameObject) LitPeripheralMachine.Get().SetDefault();
        }


        /// <summary>
        /// Kills this hull, calling Explode and the myDeath delegate.
        /// </summary>
        /// <param name="killer">The object that killed this hull.</param>
        void Die (GameObject killer)
        {
            string killerName = "unknown";
            
            Debug.Log(name + " is dying");
            
            if (killer)
            {
                killerName = killer.name;
                
                Debug.Log("killed by: " + killerName);
                // play the death sound 
                DeathSound(killer);

                if (PlayerManager.PlayerShip())
                {
                    Debug.Log("PLayer ship?:" + killer.gameObject.name + " / " + PlayerManager.PlayerShip().name ); 
                    // run the 'defeated by player' delegate
                    if (killer == PlayerManager.PlayerShip())
                    {
                        defeatedByPlayer?.Invoke();
                    }
                }
            }
            
            Debug.Log("I got here bro, mydeath is null: " + myDeath == null);
            if(myDeath!=null)
                myDeath(this, killerName);
            onKilled?.Invoke(killer);
            Explode();
        }

        /// <summary>
        /// Does damage to the hull.
        /// </summary>
        /// <param name="amount">The amount of damage</param>
        /// <param name="critChance">the critical hit value of the damage</param>
        /// <param name="source">The source of the damage</param>
        public void Damage (float amount, float critical, GameObject source)
        {
            
            if (invincible) return;

            string sourceName = "[source]";
            if (source) sourceName = source.name;

            //send message to all components / children of this object that damage has been taken
            gameObject.BroadcastMessage("DamageTaken", SendMessageOptions.DontRequireReceiver);

            float totalDamage = amount;
            
            // Critical hit
            if (IsCritical(critical))
            {
                totalDamage *= GameManager.Mode().criticalHit;
                
                //Debug.Log("Hit is critical! New damage is " + totalDamage.ToString("00.0"));
                
                GameObject particle = Instantiate(CritHitParticle(), transform.position, transform.rotation);
                particle.transform.Rotate(new Vector3(Random.Range(-180, 180), Random.Range(-180, 180), Random.Range(-180, 180)));
                Destroy(particle, 2);
                
                SpiderSound.MakeSound("Play_Crit_Explosion", gameObject);
            }
            
            Debug.Log(name + " is damaged by " + sourceName + ". Total Damage: " + totalDamage.ToString("00.0"));

            imHit?.Invoke(totalDamage, source);
            RemoveHealth(totalDamage, source);

            //shake dat cam
            if (PlayerManager.PlayerShip() == gameObject)
            {
                OnPlayerAttacked(amount);
                SpiderSound.MakeSound("Play_Player_Damaged", gameObject);
            }
            else
            {
                SpiderSound.MakeSound("Play_WEA_Bolts_Impact_Destructable", gameObject);
            }
        }

        static GameObject _critHitParticle;
        static GameObject CritHitParticle()
        {
            if (_critHitParticle) return _critHitParticle;
            _critHitParticle = Resources.Load("effects/crit hit particle") as GameObject;
            return _critHitParticle;
        }


        /// <summary>
        /// The last time the player was notified that they're being attacked
        /// </summary>
        float _lastTimeNotified = -99;
        const float notifyCooldown = 10;
        
        /// <summary>
        /// All the actions for when a player ship is attacked
        /// </summary>
        void OnPlayerAttacked(float damageAmount)
        {
            float shakeAmt = Mathf.Clamp(damageAmount / 10, .5f, 2);
            OrbitCam.ShakeCam(shakeAmt, transform.position);

            // overlay
            if (_damageOverlay) _damageOverlay.SnapAlpha(1);
            
            // If the player's in interior, notify them that theyre being attacked
            // don't notify if the camera is in normal mode
            if (OrbitCam.CamMode() == CameraMode.Normal) return;
            
            // dont notify more often than the cooldown
            if (Time.unscaledTime - _lastTimeNotified < notifyCooldown) return;
            
            Notifier.DisplayNotification(PlayerManager.Get().underAttack.LocalizedText(), Color.red);
        }

        public float NormalizedHp()
        {
            return CurrentHPNormalized();
        }

        bool IsCritical (float critical)
        {
            float rnd = Random.Range(0, critical);
            return rnd > defense;
        }

        /// <summary>
        /// Removes health, and calls 'die' if there's no health left.
        /// </summary>
        /// <param name="source">The ship / creature doing the damage</param>
        void RemoveHealth (float amount, GameObject source = null)
        {
            if (_killed) return;
          
            currentHealth -= amount;
            if (lostHealth != null) lostHealth(CurrentHP(), CurrentHPNormalized());
            
            _recentDamage = 1;

            if (PlayerManager.PlayerShip() == gameObject) HPAudio();

            if (currentHealth <= 0) Die(source);
        }

        /// <summary>
        /// Simple damage/shake, TODO Add particle effects/bubbles
        /// </summary>
        /// <param name="percentageFloat"></param><param example = "0.5 = 50%" </param>
        public void PercentageDamage (float percentageFloat)// this ship has taken damage from environments
        {
            //Shake the camera
            if (PlayerManager.PlayerShip() == gameObject)
                OrbitCam.ShakeCam(.1f, transform.position);

            int damageAmt = Mathf.CeilToInt(maxHealth * percentageFloat);
            RemoveHealth(damageAmt);
        }

        #region audio

        #region HP Siren Sounds
        bool criticalHPSiren;
        void StartCriticalHPSiren ()
        {
            if (PlayerManager.PlayerShip() != gameObject) return;
            if (criticalHPSiren) return;
            criticalHPSiren = true;
            //LitPeripheralMachine.Get().SetLowHealthAlarm();
            SpiderSound.MakeSound("Play_Critical_Health", gameObject);
        }

        void StopCriticalHPSiren ()
        {
            if (PlayerManager.PlayerShip() != gameObject) return;
            if (!criticalHPSiren) return;
            //LitPeripheralMachine.Get().SetDefault();
            SpiderSound.MakeSound("Stop_Critical_Health", gameObject);
        }

        bool lowHPWarning;
        bool returnedToHighHealth = true;

        //Call to start low HP warning
        void LowHPwarning ()
        {
            if (PlayerManager.PlayerShip() != gameObject) return;
            if (lowHPWarning) return;
            if (!returnedToHighHealth) return;
            SpiderSound.MakeSound("Play_Low_Health", gameObject);
            StartCoroutine(LowHPWarningCD());
        }

        //Cooldown for low HP warning
        IEnumerator LowHPWarningCD ()
        {
            returnedToHighHealth = false;
            lowHPWarning = true;
            yield return new WaitForSeconds(10);
            lowHPWarning = false;
        }

        #endregion

        /*
        public void SetHealthMusicSwitch ()
        {
            if (currentHealth < maxHealth / 3)
                AKMusic.Get().SwitchHealth(Combat_Health.Low_Health_Loop);

            else AKMusic.Get().SwitchHealth(Combat_Health.Normal);
        }
        */

        void HPAudio ()
        {
            if (currentHealth < maxHealth / 2)
                LowHPwarning();
            else returnedToHighHealth = true;

            //SetHealthMusicSwitch();

            if (CurrentHPNormalized() < .15f && currentHealth > 0)
                StartCriticalHPSiren();

            else StopCriticalHPSiren();
        }

        bool _playingSiren;
        void PlayDepthAlarmn ()
        {
            if (PlayerManager.PlayerShip() != gameObject) return;
            if (_playingSiren) return;
            _playingSiren = true;
            //LitPeripheralMachine.Get().SetCrushDepth();
            Debug.Log("Playing depth alarm!");
            SpiderSound.MakeSound("Play_Depth_Creak", gameObject);
            SpiderSound.MakeSound("Play_DepthAlarm", gameObject);

        }

        //Stop
        void StopDepthAlarm ()
        {
            if (PlayerManager.PlayerShip() != gameObject) return;
            if (!_playingSiren) return;
            _playingSiren = false;
            //TODO Remove only lowDepth
            //LitPeripheralMachine.Get().SetDefault();
            SpiderSound.MakeSound("Stop_Depth_Creak", gameObject);
            SpiderSound.MakeSound("Stop_DepthAlarm", gameObject);
        }

        public void DeathSound (GameObject source)
        {
            //Bridge sourceBridge
            //if (!source.GetComponent<Bridge>()) return;
            //if (!source.GetComponent<Bridge>().IsPlayer()) return;

            if (source != PlayerManager.PlayerShip()) return;
            if (!GetComponent<Bridge>()) return;
            if (!GetComponent<ArkCreature>()) return;
            SpiderSound.MakeGlobalSound("Trigger_EnemyDestroyed");
        }

        #endregion

        #region Crush Depth

        bool CrushDepthTimer (float depth)
        {
            float crushDepthLerp = (depth - testDepth) / testDepth;
            float crushDepthDelay = Mathf.Lerp(crushDepthMaxDelay, crushDepthMinDelay, crushDepthLerp);

            //TODO WWISE Callback for when creak is done for a BANG
            float groanDelay = 1;

            if (_crushDepthTimer > (crushDepthDelay - groanDelay) && !_groanSound)
                _groanSound = true;

            if (_crushDepthTimer > crushDepthDelay)
            {
                _crushDepthTimer = 0;
                _groanSound = false;
                return true;
            }
            return false;
        }

        public void RollForCrushDepthDamage (float depth)
        {
            //If the crushdepth flips the wrong coin you take damage
            //TODO Start creaks when you get close to the timing
            if (CrushDepthTimer(depth))
            {
                //Only take damage if you're 10 m below crush depth
                if (depth < testDepth - 5)
                {
                    if (Random.value < crushDepthDmgChance)
                    {
                        if (PlayerManager.PlayerShip() != gameObject)
                            SpiderSound.MakeSound("Play_Depth_Crack", gameObject);

                        PercentageDamage(0.5f);
                    }
                }
            }
        }
        #endregion

        #region HP setting and getting

        //return the current HP of this hull
        public float CurrentHP () { return currentHealth; }
        public float CurrentHPPercent () { return (currentHealth / maxHealth * 100f); }
        public float CurrentHPNormalized () { return (currentHealth / maxHealth); }

        /// <summary>
        /// Sets the new max & current HP to the given value.
        /// </summary>
        public void OverrideHP (float newHP)
        {
            maxHealth = newHP;
            currentHealth = newHP;
        }

        /// <summary>
        /// Set the current HP, clamped to max HP.
        /// </summary>
        public void SetCurrentHP (float newHP)
        {
            currentHealth = Mathf.Clamp(newHP, 0, maxHealth);
        }

        /// <summary>
        /// Set the HP as a percentage of max HP
        /// </summary>
        public void SetPercentageHP (float percentage)
        {
            percentage = Mathf.Clamp01(percentage);
            currentHealth = maxHealth * percentage;
        }
        #endregion


        bool scraping;
        float scrapeTime = 0;
        /// <summary>
        /// For minor collisions.  Visually indicates collision, but doesn't do damage
        /// </summary>
        public void Scrape (Vector3 point, float collisionVel, bool terrain)
        {
            if (stopCollisionEffect) return;

            if (terrain)
            {
                scrapeTime = 0.3f;
                if (!scraping)
                {
                    SpiderSound.MakeSound("Play_ShipScrape", gameObject);
                    StartCoroutine(ScrapeTimer());
                    scraping = true;
                }
            }
        }


        IEnumerator ScrapeTimer ()
        {
            while (scrapeTime > 0)
            {
                scrapeTime -= Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }

            scraping = false;
            SpiderSound.MakeSound("Stop_ShipScrape", gameObject);
        }



        public bool debugCollision;
        
        void OnCollisionEnter (Collision other)
        {
            if (debugCollision) Debug.Log("Collided with " + other.collider.name, other.collider);
            
            if (!gameObject.activeInHierarchy) return;

            //velocity indicator that takes into account the direction of the collision
            ContactPoint contact;
            if (other.contacts.Length > 0)
                contact = other.contacts [0];

            else contact = new ContactPoint();

            _tempVel = Vector3.Scale(contact.normal, other.relativeVelocity);
            float collisionVelocity = _tempVel.magnitude;

            // Camera shake
            OrbitCam.ShakeCam(collisionVelocity / 20, contact.point);
            
            // Check for impactable surface to create effects
            Impactable i = GO.ComponentInParentOrSelf<Impactable>(other.collider.gameObject);
            if (i != null) i.Impact(other.relativeVelocity, other.contacts[0].point);
            
            foreach (WeaponSystem ws in GetComponents<WeaponSystem>())
                ws.AddWaver(collisionVelocity, null);

            //if the velocity is minor, just do a scrape.  (visuals but no damage)
            if (collisionVelocity > 2)
            {
                if (other.gameObject.layer == LayerMask.NameToLayer("Terrain"))
                    Scrape(contact.point, collisionVelocity, true);
                else
                    Scrape(contact.point, collisionVelocity, false);
            }

            //if it's an excessive velocity, do damage based on the velocity of the impact
            if (collisionVelocity > 5 && collisionDamage && _collisionVulnerable)
            {
                _collisionVulnerable = false;

                //clamp collision damage
                float colDamage = Mathf.Clamp((collisionVelocity / 2), 0, 2);
                //apply collision damage
                Damage(colDamage, 1, other.gameObject);

                if (!gameObject.activeInHierarchy) return;
                StartCoroutine(CollisionCooldown());
            }
        }


        IEnumerator CollisionCooldown ()
        {
            _collisionVulnerable = false;
            yield return new WaitForSeconds(colDamageCooldown);
            _collisionVulnerable = true;
        }

        


        Transform thisExplosionModel;
        
        
        
        /// <summary>
        /// Explode this instance, creates sound FX, and spawns in the exploded model (if available)
        /// </summary>
        void Explode ()
        {
            QuestManager.Tick();

            StopCriticalHPSiren();

            RemoveUI();

            //If the target was overkilled, blow it up
            if (currentHealth < -4)
            {
                if (explodedModel != null)
                    thisExplosionModel = explodedModel;
                else
                    thisExplosionModel = crippledModel;
            }
            else
            {
                if (crippledModel != null)
                    thisExplosionModel = crippledModel;
                else
                    thisExplosionModel = explodedModel;
            }

            GameObject explosionParticle = null;

            Bridge b = GetComponent<Bridge>();
            
            // If this is a ship, add the destruction particle.
            if (b)
            {
                explosionParticle = Instantiate(SubChassisGlobal.DestructionParticle(), transform.position,
                    transform.rotation);
                Destroy(explosionParticle, 10);
            }

            if (thisExplosionModel != null)
            {
                Transform expModel = GameManager.Pool().Spawn(thisExplosionModel, transform.position, transform.rotation);
                
                if (explosionParticle) explosionParticle.transform.parent = expModel;

                Rigidbody myRB = GetComponent<Rigidbody>();

                if (myRB)
                {
                    foreach (Rigidbody rb in expModel.GetComponentsInChildren<Rigidbody>())
                        rb.velocity = myRB.velocity;

                    DestroyedShip destroyedModel = expModel.GetComponent<DestroyedShip>();
                    if (destroyedModel)
                        if (myRB)
                            destroyedModel.Init(myRB.velocity);
                }

                // Check for skin on a destroyed model
                Skin destroyedSkin = expModel.GetComponent<Skin>();

                // apply materials to the destroyed model
                if (destroyedSkin)
                {
                    // apply the bridge's skin
                    if (b) destroyedSkin.ApplySkin(b.chassis.skin.destroyed);

                    else if (GetComponent<Skin>())
                        destroyedSkin.ApplySkin(GetComponent<Skin>().skin);
                }
            }
            
            GameManager.Despawn(gameObject);

            _killed = true;
        }
    }
}