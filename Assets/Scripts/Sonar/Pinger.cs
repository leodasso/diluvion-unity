using System;
using UnityEngine;
using System.Collections;
using Diluvion.Ships;
using Sirenix.OdinInspector;
using SpiderWeb;

namespace Diluvion.Sonar
{
    /// <summary>
    /// Component tied to sonar module. Does the actual pinging. Note this is separate from the listener component, but uses the listener
    /// to process signatures found by ping.
    /// </summary>
    public class Pinger : MonoBehaviour
    {

        public float rangeMult = 1;
        public float chargeMult = 1;
        public float powerMult = 1;

        public float sosCancelTime = 1;
      
        float charge = .1f;

        public const float hailPercent = 0.2f;
        public const float sosPercent = 0.9f;

        public SonarModule sonar;

        bool charging;
        
        float cooldown;     // If this is greater than 0, can't ping



        void OnEnable()
        {
            charge = .1f;
        }
        
        // Update is called once per frame
        void Update()
        {
            if (cooldown > 0) cooldown -= Time.deltaTime;

            if (!sonar) return;
            if (charging)
            {
                charge += Time.deltaTime * chargeMult * sonar.chargeSpeed;
               
                if(charge > MaxCharge()+1 )
                {
                    SetCharging();
                    SpiderSound.MakeSound("Play_Sonar_Unable", gameObject);
                    SpiderSound.MakeSound("Stop_Sonar_Charge", gameObject);
                   // Debug.Log("Cancelled Ping held too long");
                }
            }
        }


        public bool ShowGUI()
        {
            return cooldown > 0 || charging;
        }

        public float MaxCharge()
        {
            return sonar.maxCharge * rangeMult;
        }

        public float NormalizedCharge()
        {
            return Mathf.Clamp01(charge / MaxCharge());
        }


        public bool BeginCharge()
        {
            if (!sonar || cooldown > 0 || !enabled)
            {
                SpiderSound.MakeSound("Play_Sonar_Unable", gameObject);
                return false;
            }

            charging = true;
            return true;
        }

        public void Ping(float overrideCharge = 0)
        {

            if(Math.Abs(overrideCharge) < .01f)
                if (!enabled || !sonar || !charging) { Debug.Log("Ping failed because: enabled: "+enabled+" sonar: "+sonar+" charging: "+charging); return; }
        
            // instantiate ping object
            Ping newPing = Instantiate(sonar.pingPrefab, transform.position, transform.rotation).GetComponent<Ping>();
            float normalized = NormalizedCharge();
            
            if (sonar.pingEffect)// Kill ping after 3 seconds
                Destroy(Instantiate(sonar.pingEffect, transform.position, transform.rotation), 3);
            
            if (overrideCharge > 0)
                normalized = overrideCharge;
            
            PingResult pType = PingType(normalized);
            newPing.InitPing(GetComponent<Bridge>(), sonar, normalized, pType);
            
           // Debug.Log("Pinged with " + normalized + " charge: " + " hail =<" + hailPercent + ", SOS =>"+ sosPercent +  pType.ToString());
            SetCharging();
        }

        public float HailAmount()
        {
            return MaxCharge() * hailPercent;
        }

        public float SOSAmount()
        {
            return MaxCharge() * sosPercent;
        }

        void SetCharging()
        {
            charging = false;
            charge = .1f;
            cooldown = 1;
        }

        //Get the ping type from the charge level
        PingResult PingType(float normalizedCharge)
        {
            /*
            if (normalizedCharge > sosPercent)
            {
               // Debug.Log("returning SOS");
                return PingResult.SOS;
            }
            */

            if (normalizedCharge < hailPercent)
            {
               // Debug.Log("returning Hail");
                return PingResult.Hail;
            }

          //  Debug.Log("returning Ping");
            return PingResult.Ping;

        }

    }
}