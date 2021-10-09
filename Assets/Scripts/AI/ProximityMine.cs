using UnityEngine;
using System.Collections;
//using System.Collections.Generic;
//using System.Linq;
//using HeavyDutyInspector;
//using Diluvion.Ships;
//using Sirenix.OdinInspector;

namespace Diluvion
{

    public class ProximityMine : MonoBehaviour
    {
    /*

        //public float damageFactor = 100;
        float reactDistance = 30;

        [Comment("How close must another hull be before it explodes?")]
        public float explodeDistance = 10;
        [MinValue(1), OnValueChanged("AdjustTrigger")]
        public float detectionDistance = 20;
        
        [Space]
        public float warnDist = 1;
        public float armedDist = 2;
        [Space]
        public GameObject mineTop;
        public GameObject mineBottom;
        public SphereCollider detectionTrigger;
        public List<MeshRenderer> glowyMeshes = new List<MeshRenderer>();

        float warnAmount = 0;
        float mineTopInitY;
        float mineBottomInitY;
        List<Hull> invaders = new List<Hull>();
        Hull nearestInvader;
        bool armed = false;
        bool warning = false;
        bool boom = false;
        float boomTime = .5f;
        Light pointLight;

        Color initGlowColor;
        Color currentGlowColor;
        float glowIntensity = 1;

        void AdjustTrigger()
        {
            if (!detectionTrigger) return;

            detectionTrigger.isTrigger = true;
            detectionTrigger.radius = detectionDistance;
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, explodeDistance);
        }


        void Start()
        {

            detectionTrigger.gameObject.layer = LayerMask.NameToLayer("Tools");
            pointLight = GetComponentInChildren<Light>();

            invaders = new List<Hull>();

            mineTopInitY = mineTop.transform.localPosition.y;
            mineBottomInitY = mineBottom.transform.localPosition.y;
            reactDistance = detectionTrigger.radius * detectionTrigger.transform.lossyScale.x;

            initGlowColor = glowyMeshes[0].material.GetColor("_EmissionColor");
        }

        // Update is called once per frame
        void Update()
        {

            //find nearest invader
            if (invaders.Count > 0)
            {
                invaders = invaders.OrderBy(go => Vector3.SqrMagnitude(transform.position - go.transform.position)).ToList();
                nearestInvader = invaders[0];
            }
            else
            {
                nearestInvader = null;
                warning = false;
            }

            if (nearestInvader != null)
            {
                //get the distance from the invader to this
                float dist = Vector3.Distance(nearestInvader.transform.position, transform.position);

                //adjust so that the max distance is the explosion proximity
                float adjustedDist = dist - explodeDistance; // 3 - 5 = -2

                float adjustedTotalDist = reactDistance - explodeDistance; // 40 - 5 = 35

                //get the ratio 
                warnAmount = 1 - (adjustedDist / adjustedTotalDist); // 1 - (-2 / 35)

                if (dist < explodeDistance)
                {
                    armed = true;
                    Explode();
                }
            }
            else warnAmount = Mathf.Lerp(warnAmount, 0, Time.deltaTime);
            SetWarningVisuals(warnAmount);
        }

        void FixedUpdate()
        {
            // remove all missing invaders
            if (invaders.Count > 0)
                invaders.RemoveAll(x => (x == null || !x.gameObject.activeInHierarchy));
            //invaders = invaders.Where(x => (x != null && x.gameObject.activeInHierarchy)).ToList();
        }

        /// <summary>
        /// sets the visuals to show warning.
        /// </summary>
        /// <param name="amount">A value between 0 (no threat) and 1 (gonna explode)</param>
        void SetWarningVisuals(float amount)
        {

            //control glow color
            float glowIntensity = amount * 2;
            glowIntensity = Mathf.Clamp(glowIntensity, 0, 50);
            currentGlowColor = initGlowColor * glowIntensity;

            foreach (MeshRenderer renderer in glowyMeshes)
                renderer.material.SetColor("_EmissionColor", currentGlowColor);

            // Control light emission
            pointLight.intensity = amount * 6;

            ControlShell(mineTop, 1, mineTopInitY, amount);
            ControlShell(mineBottom, -1, mineBottomInitY, amount);
        }


        public void Explode()
        {
            //Destroy self via hull
            GetComponent<Explosive>().SelfDestruct();
        }


        void ControlShell(GameObject shell, int direction, float initY, float amount)
        {

            float dist = 0;

            if (!armed) dist = warnDist * Mathf.Clamp01(amount);
            else dist = armedDist;

            float localY = initY + dist * direction;
            Vector3 pos = shell.transform.localPosition;
            shell.transform.localPosition = new Vector3(pos.x, localY, pos.z);
        }

        void OnTriggerEnter(Collider other)
        {
            //We've encountered a ship		
            Hull theBody = other.GetComponent<Hull>();

            //Check for bridge
            Bridge theBrige = other.GetComponent<Bridge>();
            if (theBrige == null) return;

            if (theBody != null)
            {
                if (!theBody.GetComponent<ProximityMine>() && !theBody.GetComponent<Swarmer>())
                    invaders.Add(theBody);
            }
        }

        void OnTriggerExit(Collider other)
        {
            Hull h = other.GetComponent<Hull>();

            if (!h) return;
            if (invaders.Contains(h)) invaders.Remove(h);
        }

        public AlignmentToPlayer getAlignment()
        {
            return AlignmentToPlayer.Hostile;
        }

        public float SafeDistance()
        {
            return 15;
        }
        */
    }
}