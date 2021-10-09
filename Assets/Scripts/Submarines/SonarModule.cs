using UnityEngine;
using SpiderWeb;
using Diluvion.Sonar;
using DUI;
using Sirenix.OdinInspector;

namespace Diluvion.Ships
{

    [CreateAssetMenu(fileName = "sonar module", menuName = "Diluvion/subs/modules/sonar")]
    public class SonarModule : ShipModule
    {

        public GameObject pingPrefab;
        
        [Tooltip("Prefab to instantiate when this sends out a ping")]
        public GameObject pingEffect;

        [Space] 
        [ToggleLeft, Tooltip("instantiate effects on things pinged by this module? Typically you'll only want this for player sonar")]
        public bool createsPingedEffects;
        
        [ShowIf("createsPingedEffects")]
        public GameObject friendlyEffect;
        [ShowIf("createsPingedEffects")]
        public GameObject neutralEffect;
        [ShowIf("createsPingedEffects")]
        public GameObject hostileEffect;

        [ToggleLeft]
        public bool pingsQuestObjectives;
        [ShowIf("pingsQuestObjectives")]
        public GameObject mainObjective;
        [ShowIf("pingsQuestObjectives")]
        public GameObject objective;
        
        
        [ToggleLeft]
        public bool playsEchoSound;
        
        [Space]
        
        public float pingRange = 200;
        public float chargeSpeed = 1;
        public float maxCharge = 3;
        public float pingPower = 5;
        public float pingExpansionSpeed = 10; // units per second


        public override void AddToShip(Bridge bridge, ShipModuleSave data = null)
        {
            if (bridge.shipModules.Contains(this)) return;
            
            base.AddToShip(bridge);
            Pinger p = bridge.gameObject.AddComponent<Pinger>();
            p.enabled = false;
            p.sonar = this;
            
            bridge.AddSonarListener(this);
        }


        public override DUIPanel CreateUI(Bridge b)
        {
            DUISonarPanel returnPanel = base.CreateUI(b) as DUISonarPanel;
            if (returnPanel == null)
            {
                Debug.LogError("Wrong or missing UI object on " + this.name , this);
                return null;
            }
            returnPanel.SonarPanelSetup(b.MyPinger, b.GetComponent<Listener>());
            return returnPanel;

        }

        public override void RemoveFromShip(Bridge bridge)
        {
            base.RemoveFromShip(bridge);
            Pinger pinger = bridge.gameObject.GetComponent<Pinger>();
            if (pinger) Destroy(pinger);
        }

        public override bool Enable(Bridge bridge)
        {
            if (!base.Enable(bridge)) return false;
            SetEnabled(bridge, true);
            return true;
        }

        public override bool Disable(Bridge bridge, float disabledTime = 0)
        {
            if (!base.Disable(bridge, disabledTime)) return false;
            SetEnabled(bridge, false);
            return true;
        }

        void SetEnabled(Bridge b, bool enabled)
        {
            Pinger p = b.GetComponent<Pinger>();
            if (p) p.enabled = enabled;
        }

        /// <summary>
        /// Creates an effect for an object being pinged by this sonar module
        /// </summary>
        /// <param name="pinged"></param>
        public void CreatePingedEffect(GameObject pinged)
        {
            if (!createsPingedEffects) return;

            AlignmentToPlayer a = AlignmentToPlayer.Neutral;
            
            // Search for alignment interfaces on the object
            var alignmentInterface = pinged.GetComponent<IAlignable>();
            if (alignmentInterface != null)
            {
                a = alignmentInterface.getAlignment();
            }
            
            // instantiate effects object for stuff that gets pinged
            GameObject effects = EffectObject(a);

            if (effects == null) return;

            Destroy(Instantiate(effects, pinged.transform.position, pinged.transform.rotation), 3);
        }

        /// <summary>
        /// Called when a sonar ping from this module touches its first contact
        /// </summary>
        public void Echo(GameObject ping)
        {
            if (!playsEchoSound) return;
            SpiderSound.MakeSound("Play_Sonar_Echo", ping);
        }
        
        
        
        /// <summary>
        /// Returns the appropriate effects object for the given ping result
        /// </summary>
        GameObject EffectObject(AlignmentToPlayer alignment)
        {
            switch (alignment)
            {
                case AlignmentToPlayer.Friendly: return friendlyEffect;
                case AlignmentToPlayer.Hostile: return hostileEffect;
                case AlignmentToPlayer.Neutral: return neutralEffect;
                    
                default: return null;
            }
        } 
        

        protected override void OnInputDown(Bridge b)
        {
            base.OnInputDown(b);
            bool canPing = b.MyPinger.BeginCharge();

            if (!canPing) SpiderSound.MakeSound("Play_Sonar_Unable", b.gameObject);
            else SpiderSound.MakeSound("Play_Sonar_Charge", b.gameObject);
        }

        protected override void OnInputUp(Bridge b)
        {
            base.OnInputUp(b);
            SpiderSound.MakeSound("Stop_Sonar_Charge", b.gameObject);
            b.MyPinger.Ping();
        }

        /// <summary>
        /// Sets the Pinger component on the given bridge as enabled or disabled.
        /// </summary>
        public override bool EnabledForBridge(Bridge b)
        {
            if (!base.EnabledForBridge(b)) return false;
            if (b.GetComponent<Pinger>() == null) return false;
            return true;
        }
    }
}