using UnityEngine;
using System.Collections;
using Sirenix.OdinInspector;
using SpiderWeb;
using Diluvion.Ships;
using Diluvion.Sonar;
using Diluvion.AI;
namespace Diluvion
{

    public enum TurretState
    {
        InRange = 1,
        OutRange = 2,
        Gone = 3
    }

    public class AITurret : MonoBehaviour
    {
        public SonarStats target;

        [InfoBox("The range where the turret enters the 'InRange' state")]
        public float engageRange;

        public TurretState state = TurretState.Gone;

        [ShowIf("InRange"), InfoBox("Engaging Target inside EngageRange"),ReadOnly] public bool inRange;

        [ShowIf("OutRange"), InfoBox("Tracking target inside vision range"),ReadOnly] public bool outRange;

        [ShowIf("Gone"), InfoBox("Lost Target"),ReadOnly] public bool gone;

        public CaptainScriptableObject overridePersonality;
      
        bool init = false;
        int firingIterations = 1;
        Bridge myBridge;
        TurretState prevState;
        float chooseMissionSeconds = 3;
        bool initializedBehaviourLoop = false;
        AIWeapons aiWeapons;


        bool InRange => state == TurretState.InRange;
        bool OutRange => state == TurretState.OutRange;
        bool Gone => state == TurretState.Gone;
        
        void SortOutPersonality()
        {
           /* if (overridePersonality != null)
                personality = new CaptainPersonality(overridePersonality.personality);
            else
                personality = defaultPersonality;

            */
       //     AiWeapons().SetPersonality(personality);
        }

        public void OnEnable()
        {
            InitTurret();
            SortOutPersonality();
        }

        public void Start()
        {
            InitTurret();
            SortOutPersonality();
            //WorldControl.Get().worldReset += StopBehaviours;
        }

        public void InitTurret()
        {
            if (init) return;
            if (myBridge == null)
                if (GetComponent<Bridge>())
                    myBridge = GetComponent<Bridge>();
                else if (GetComponentInParent<Bridge>())
                    myBridge = GetComponentInParent<Bridge>();


          //  AiWeapons().InitAIWeapons(myBridge);
            SortOutPersonality();
            init = true;
        }

        /// <summary>
        /// Starts the behaviorus
        /// </summary>
        public void StartBehaviours()
        {
            state = TurretState.Gone;
            StartCoroutine(ChooseMission());
            StartCoroutine(DoMission());
            initializedBehaviourLoop = true;

        }
        void StopBehaviours()
        {
            StopAllCoroutines();
            //WorldControl.Get().worldReset -= StopBehaviours;

        }
        AIWeapons AiWeapons()
        {
            if (aiWeapons != null) return aiWeapons;
            aiWeapons = GetComponent<AIWeapons>();
            return aiWeapons;

        }

        //FireAtTheTarget
        public void FireAtTarget(SonarStats t)
        {
        //    AiWeapons().Fire(t);

        }
        //Sets the weapons to lead at the target
        public void AimAtTarget(SonarStats t)
        {
        //    AiWeapons().SetLeadAim(t);
        }

        //Idles the guns
        public void Idle()
        {
       //     AiWeapons().IdleWeapons();
        }

        //Loop for changing the missions
        IEnumerator ChooseMission()
        {
            if (target != null)
                if (Calc.WithinDistance(engageRange, transform, target.transform))
                    state = TurretState.InRange;
                else //TODO YAGNI Fudge the range a little with some sticky behaviorus
                {
                    state = TurretState.OutRange;
                }
            else
                state = TurretState.Gone;

            // Debug.Log("Chose state: " + state);
            yield return new WaitForSeconds(chooseMissionSeconds);
            StartCoroutine(ChooseMission());

        }

        //loop for running the mission
        IEnumerator DoMission()
        {
            switch (state)
            {
                case TurretState.InRange:
                    {
                        yield return Firing();
                        break;
                    }
                case TurretState.OutRange:
                    {
                        yield return Wary();
                        break;
                    }
                case TurretState.Gone:
                    {
                        yield return Peaceful();
                        break;
                    }
            }


            yield return StartCoroutine(DoMission());
        }



        /// <summary>
        /// Firing Behaviour
        /// </summary>
        /// <returns></returns>
        IEnumerator Firing()
        {
            //Enter State
            prevState = state;
            if (target == null) yield break;
            // if (searchLight != null) searchLight.LookAt(target);
            AimAtTarget(target);
            //Running State
            yield return new WaitForEndOfFrame();
            while (state == prevState)
            {
                //Debug.Log("Running firing behaviour.");
                if (target == null) break;
                FireAtTarget(target);
                yield return new WaitForEndOfFrame();
            }

            //Exit state
            yield break;
        }

        /// <summary>
        /// Guns still point but no firing
        /// </summary>
        /// <returns></returns>
        IEnumerator Wary()
        {
            //Enter State
            prevState = state;
            if (target == null) yield break;
            // if (searchLight != null) searchLight.LostTarget();
            //Running State
            AimAtTarget(target);
            yield return new WaitForEndOfFrame();
            while (state == prevState)
            {
                if (target == null) break;

                yield return new WaitForEndOfFrame();
            }

            //Exit state
            yield break;
        }

        //Guns reset to their original position
        IEnumerator Peaceful()
        {
            //Enter State
            prevState = state;
            yield return new WaitForEndOfFrame();
            //Running State
            while (state == prevState)
            {
                Idle();
                yield return new WaitForEndOfFrame();
            }

            //Exit state
            yield break;
        }


        public void OnTriggerEnter(Collider col)
        {
            Bridge otherBridge = col.GetComponent<Bridge>();

            if (otherBridge == null) return;
            if (!otherBridge.IsPlayer()) return;
            //Debug.Log("TRIGGERED");
            if (target != null) return;
            if (!col.GetComponent<SonarStats>()) return;
            target = col.GetComponent<SonarStats>();
            //if(!waitForCallback)
            StartBehaviours();

        }

        public void OnTriggerExit(Collider col)
        {
            Bridge otherBridge = col.GetComponent<Bridge>();

            if (otherBridge == null) return;
            if (otherBridge.GetComponent<SonarStats>() != target) return;
            target = null;
        }
    }
}