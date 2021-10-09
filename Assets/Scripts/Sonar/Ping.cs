using UnityEngine;
using System.Collections;
using PathologicalGames;
using SpiderWeb;
using Diluvion.Ships;
using System.Collections.Generic;

namespace Diluvion.Sonar
{
    public enum PingResult
    {
        Hail,
        Ping,
        SOS,       
        Invalid
    }

    /// <summary>
    /// The ping wave that extends out. Assumes that the visuals are a radius 1 unit sphere
    /// </summary>
    public class Ping : MonoBehaviour
    {
        public float charge = 1;
        public float amplitude;
        public GameObject pingedEffect;

        /// <summary>
        /// Has this ping contacted anything yet?
        /// </summary>
        bool _firstContact;

        PingResult pingType = PingResult.Ping;

        bool playerPing = false;
        Bridge linkedBridge;
        SonarModule sonarModule;
        Listener listener;
        float range = 1;
        float power;
        bool echoed;    // has it made the echo sound yet?
        Renderer r;
        Color pingColor;
        float remainingDist;

        List<QuestActor> _objectives = new List<QuestActor>();
        List<QuestActor> _pingedObjectives = new List<QuestActor>();
        
        void Awake()
        {
            r = GetComponent<Renderer>();
        }
        
        List<int> alreadyPinged = new List<int>();
        /// <summary>
        /// Initializes a ping to begin expanding.
        /// </summary>
        /// <param name="b">bridge of the ship that pinged</param>
        /// <param name="s">Linked sonar module</param>
        /// <param name="c">charge of the ping</param>
        public void InitPing(Bridge b, SonarModule s, float c, PingResult pType = PingResult.Invalid)
        {
            charge = c;
            linkedBridge = b;
            listener = linkedBridge.GetComponent<Listener>();
            listener.AddPing(this);
            power = s.pingPower;
            sonarModule = s;
            pingType = pType;
            range = 1;
            alreadyPinged.Clear();
            remainingDist = 0;
            if (GetComponent<Renderer>())
            {
                pingColor =  GetComponent<Renderer>().material.GetColor("_HighlightColor");
            }

            if (s.pingsQuestObjectives)
            {
                // Create list of objectives to check for
                _objectives = QuestManager.GetAllWaypoints();
            }

            pingColor.a = 1;
            SetColor(pingColor);
            transform.localScale = Vector3.one ;
        }

        /// <summary>
        /// Returns if the given sonar stats has been pinged by this instance. 
        /// </summary>
        /// <returns>This instances ping status of the given sonar stats. (a pingResult)</returns>
        public PingResult Pinged(int ss, float ssRange, bool inLos)
        {
            if (alreadyPinged.Contains(ss)) return PingResult.Invalid;
            if (ssRange > range) return PingResult.Invalid;
            if(!inLos)return PingResult.Invalid;

            alreadyPinged.Add(ss);

            if (!_firstContact)
            {
                if (sonarModule) sonarModule.Echo(gameObject);
                _firstContact = true;
            }
            
            return pingType;
        }

       
        void Update()
        {
            if (!linkedBridge || !sonarModule) return;

            float maxRange = Mathf.Clamp(sonarModule.pingRange * charge, 35, 600);

            // scale up
            if (range < maxRange)
                range += sonarModule.pingExpansionSpeed * Time.deltaTime;
            else
                Destroy(gameObject);
            transform.localScale = Vector3.one * range;

            listener.minRange = Mathf.Clamp(listener.minRange, range, 9999);

            if (r)
            {
                remainingDist = maxRange - range;

                float alpha = Mathf.Lerp(0, 1, remainingDist / maxRange);
                pingColor.a = alpha;
                SetColor( pingColor);
            }
            
            // Ping quest objectives
            if (sonarModule.pingsQuestObjectives)
            {
                foreach (var wp in _objectives)
                {
                    if (_pingedObjectives.Contains(wp)) continue;
                    if (Vector3.Distance(wp.transform.position, transform.position) > range) continue;
                    //if (listener.InLOS(wp.transform, 500))
                    //{
                        _pingedObjectives.Add(wp);
                        PingWaypoint(wp);
                    //}
                }
            }
        }

        void PingWaypoint(QuestActor waypoint)
        {
            GameObject effect = sonarModule.objective;
            if (waypoint == QuestManager.MainWaypoint()) effect = sonarModule.mainObjective;
            
            Destroy(Instantiate(effect, waypoint.transform.position, waypoint.transform.rotation), 7);
        }

        void SetColor(Color color)
        {
            if (!r) return;
            r.material.SetColor("_HighlightColor", color);
        }


        void OnDestroy()
        {
            listener.RemovePing(this);
        }
    }
}