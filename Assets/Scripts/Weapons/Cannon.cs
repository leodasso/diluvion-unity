using Diluvion.Ships;
using UnityEngine;
using Sirenix.OdinInspector;

namespace Diluvion
{
    /// <summary>
    /// Add this to a game object to give it the ability to fire an ammo. This will be attached to the actual model
    /// of the cannon. 
    /// <seealso cref="Munition"/>
    /// </summary>
    public class Cannon : MonoBehaviour
    {
        [Space] [Tooltip("The effects to be spawned in when firing.")] public GameObject FireFX;

        [Tooltip("The ammo that gets fired. This wont work properly if it doesn't have a rigidbody."), AssetsOnly]
        public GameObject ammoPrefab;

        [Space, Tooltip("The relative position that the munition will be spawned")] 
        public Vector3 exitPoint;
        [Tooltip("The relative direction of fire")]
        public Vector3 exitVector = new Vector3(0, 0, 1);
        
        [Space, Range(1, 25),OnValueChanged("CannonDanger",true)] 
        public int damage = 1;
        
        [OnValueChanged("CannonDanger",true)]
        [Tooltip("The max range the munition can travel before becoming a dud.")]
        public float maxRange = 40;
        
        [OnValueChanged("CannonDanger",true)]
        [Tooltip("The cooldown between shots")]
        public float cooldown = 5;
        
        [OnValueChanged("CannonDanger",true)]
        [Tooltip("The forward velocity that the munition is launched at")]
        public float shotSpeed = 10;
        
        [Range(0, 5),OnValueChanged("CannonDanger",true)]
        [Tooltip("The critical hit rate of the cannon")]
        public float critRate = .1f;
        
        [Range(0, 45), Tooltip("The angle of the spread cone. Wider spread means less accurate, more shotgun-y")] 
        public float shotSpread = 3;
        
        [Range(.05f, 30), Tooltip("The radius of the munition's hit detection")] 
        public float munitionRadius = .2f;
        
        [Space, Range(-200, 200), Tooltip("The spin velocity of the munition.")] 
        public float spin;
        
        [Tooltip("The recoil force applied to the attached rigidbody (if any).")]
        public float recoil = 1;

        [SerializeField]
        protected int danger;

        Rigidbody _parentRb;
        GameObject _source;
        protected Bridge _bridge;

        #region calculation factors
        
        [Button]
        public virtual int CannonDanger()
        {
            //Danger comprised of DPS, divided by a factor of recoil,  range and shotspeed;
            Debug.Log("dps: " + DamagePerSecond*2 + " * crit: " + CritDanger + " * range: "+ RangeDanger + " * speed: " + ShotSpeedDanger + " / recoil: " + RecoilDanger);
            return danger =  Mathf.RoundToInt( (DamagePerSecond*2 * CritDanger* RangeDanger * ShotSpeedDanger)/RecoilDanger);
        }
        
        //Edit these factors to change how much danger the weapon has
        public virtual float CritDanger => (1 + critRate); 
    
        public virtual float RecoilDanger => (1 + (recoil / 10));

        public virtual float RangeDanger => (1 + (maxRange / 10));
        
        public virtual float ShotSpeedDanger => (1 + (shotSpeed / 50));

        public virtual float DamagePerSecond => damage / cooldown;
        
        #endregion
        void Start()
        {
            _parentRb = GetComponentInParent<Rigidbody>();
            _bridge = GetComponentInParent<Bridge>();
        }

        public Vector3 ExitPos()
        {
            return transform.TransformPoint(exitPoint);
        }

        /// <summary>
        /// Tells the cannon what it's equipped to, so the munition knows to ignore that.
        /// </summary>
        public void SetWeaponSource(GameObject newSource)
        {
            _source = newSource;
        }

        /// <summary>
        /// Fires the given ammo, and returns the new ammo instance created.
        /// </summary>
        [Button]
        public virtual GameObject Fire()
        {
            // Spawn the ammunition
            Transform newShot = GameManager.Pool().Spawn(ammoPrefab, ExitPos(), Quaternion.LookRotation(ExitVector()));

            newShot.localScale = ammoPrefab.transform.localScale;

            // Spawn the effects
            if (FireFX)
            {
                Transform fx = GameManager.Pool().Spawn(FireFX, ExitPos(), Quaternion.LookRotation(ExitVector()));
                GameManager.Pool().Despawn(fx, 4, GameManager.Pool().transform);
            }

            
            // set a random rotation for the new shot
            Vector3 randomEuler = new Vector3(Random.Range(-shotSpread, shotSpread),
                Random.Range(-shotSpread, shotSpread), Random.Range(-shotSpread, shotSpread));

            newShot.Rotate(randomEuler);

            Rigidbody rb = newShot.GetComponent<Rigidbody>();
            if (rb) {
                rb.velocity = newShot.forward * shotSpeed;
                if (_parentRb) rb.velocity += _parentRb.velocity;

                rb.AddRelativeTorque(Vector3.forward * spin * 10000 * rb.mass);
                //Debug.Log("Shot's angular velocity: " + rb.angularVelocity);
            }

            Munition m = newShot.GetComponent<Munition>();
            if (m)
            {
                m.damage = damage;
                m.maxRange = maxRange;
                m.SetSource(Source());
                m.critRate = critRate;
                m.radius = munitionRadius;
            }

            // Recoil and shake
            if (_parentRb) _parentRb.AddForceAtPosition(-transform.forward * recoil * 50 , transform.position);
            OrbitCam.ShakeCam(recoil / 10, transform.position);

            if(GetComponent<AKTriggerCallback>())
                GetComponent<AKTriggerCallback>().Callback();
            
            //Debug.Log("Fired " + newShot.name + " from " + name);
            return newShot.gameObject;
        }

        /// <summary>
        /// The speed of the projectile, used for leading shots.
        /// </summary>
        public virtual float BulletSpeed()
        {
            return shotSpeed;
        }

        /// <summary>
        /// The controller of this cannon. If no controller, just returns the cannon itself.
        /// </summary>
        protected GameObject Source()
        {
            if (!_source) return gameObject;
            return _source;
        }

        protected Vector3 ExitVector()
        {
            return transform.rotation * exitVector;
        }

        protected bool IsPlayer()
        {
            if (!_bridge) return false;
            return _bridge.IsPlayer();
        }
    }
}