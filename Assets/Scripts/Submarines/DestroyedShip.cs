using UnityEngine;
using System.Collections.Generic;
using PathologicalGames;
using Sirenix.OdinInspector;
using Diluvion.Ships;
using Diluvion.Sonar;
using Diluvion.Achievements;

namespace Diluvion
{

    public class DestroyedShip : MonoBehaviour
    {

        public float lifetime = 60;
        float timer;
        public Rigidbody[] myParts;

        public bool autoExplode;
        public float autoExplodeForce = 20;
        public float autoExplodeTorque = 20;
        public bool explodeOnStart;

        Hull hull;

        public void Awake()
        {
            hull = GetComponent<Hull>();
            if (hull)    hull.myDeath += ResetPositions;
        }

        // Use this for initialization
        void Start()
        {
            if (explodeOnStart) Init(Vector3.zero);
        }

        void Update()
        {
            timer += Time.deltaTime;

            if (timer > lifetime)
            {
                if (hull)
                {
                    hull.SelfDestruct();
                }
                else
                {
                    Debug.Log("DESPAWNING FROM:" + this.name , this); Destroy(gameObject);
                }
            }
        }


        void SDC(Component comp)
        {
            if (comp == null) return;
            DestroyImmediate(comp);
        }

        [Button]
        public void AutoSetupButton()
        {
            SDC(GetComponent<Bridge>());
            //SDC(GetComponent<ShipInfo>());
            SDC(GetComponent<ShipAnimator>());
            SDC(GetComponent<ShipMover>());
            SDC(GetComponent<ShipControls>());
            SDC(GetComponent<Inventory>());
            if(GetComponent<SonarStats>())
                GetComponent<SonarStats>().targetable = false;
            //GetComponent<SonarStats>().info.displayName = "Crippled " + GetComponent<SonarStats>().info.displayName;
            SDC(GetComponent<AchievementTrigger>());
            SDC(GetComponentInChildren<Navigation>());

            List<TurretRotator> allRotators = new List<TurretRotator>(GetComponentsInChildren<TurretRotator>());
            foreach (TurretRotator tr in allRotators)
                DestroyImmediate(tr);

            List<Light> allLights = new List<Light>(GetComponentsInChildren<Light>());
            foreach (Light l in allLights)
                DestroyImmediate(l.gameObject);

            List<TrailRenderer> allTrails = new List<TrailRenderer>(GetComponentsInChildren<TrailRenderer>());
            foreach (TrailRenderer tr in allTrails)
                DestroyImmediate(tr.gameObject);


            List<Mount> allmounts = new List<Mount>(GetComponentsInChildren<Mount>());
            foreach (Mount m in allmounts)
                DestroyImmediate(m.gameObject);

            //Interior Stuff

            List<GameObject> oldStations = new List<GameObject>();
            foreach (CapsuleRotator c in GetComponentsInChildren<CapsuleRotator>())
            {
                if (c.GetComponent<HeartStation>()) continue;
                SDC(c.GetComponent<MeshRenderer>());
                SDC(c.GetComponent<MeshFilter>());
                oldStations.Add(c.gameObject);
            }
            List<GameObject> fakeStationprefab = new List<GameObject>();
            fakeStationprefab.Add(Instantiate(Resources.Load("Stripped_Bolt Station")) as GameObject);
            fakeStationprefab.Add(Instantiate(Resources.Load("Stripped_Helm Station")) as GameObject);
            fakeStationprefab.Add(Instantiate(Resources.Load("Stripped_Sonar Station")) as GameObject);
            fakeStationprefab.Add(Instantiate(Resources.Load("Stripped_Torpedo Station")) as GameObject);

            int fakeScount = 0;
            foreach (GameObject go in oldStations)
            {
                if (fakeScount > 3) break;
                go.transform.localScale = Vector3.one;
                SDC(go.GetComponent<CapsuleRotator>());
                fakeStationprefab[fakeScount].transform.SetParent(go.transform);
                fakeStationprefab[fakeScount].transform.localPosition = Vector3.zero;
                fakeScount++;
            }

            SDC(GetComponentInChildren<HeartStation>());
            SDC(GetComponentInChildren<CapsuleRotator>());
            
            //if(GetComponentInChildren<InteriorManager>())
            //    GetComponentInChildren<InteriorManager>().TintAll(Color.grey);

        }

        [Button]
        void GetAllRB()
        {
            myParts = GetComponentsInChildren<Rigidbody>();
            foreach (Rigidbody rb in myParts)
            {
                if (rb.GetComponent<ResetPosOnSpawn>()) continue;
                rb.gameObject.AddComponent<ResetPosOnSpawn>();
            }

            //Not on this object as this should be the one spawned in and gets placed
            if (GetComponent<ResetPosOnSpawn>())
                DestroyImmediate(GetComponent<ResetPosOnSpawn>());
        }

        public void Init(Vector3 inheritVelocity)
        {

            SpiderWeb.SpiderSound.MakeSound("Play_Large_Explosion_Clean", gameObject);
            foreach (Rigidbody rb in myParts)
            {

                if (rb == null) continue;
                if (rb != GetComponent<Rigidbody>())//dont unparent this object
                    rb.transform.SetParent(transform.parent);
                rb.AddForce(inheritVelocity, ForceMode.VelocityChange);

                //Destroy(rb.gameObject, 30);

                if (autoExplode)
                {
                    rb.AddExplosionForce(autoExplodeForce * 10, transform.position, autoExplodeForce);
                    rb.AddTorque(new Vector3(Random.Range(-autoExplodeTorque, autoExplodeTorque), Random.Range(-autoExplodeTorque, autoExplodeTorque), Random.Range(-autoExplodeTorque, autoExplodeTorque)));
                }
            }
        }

        public void ResetPositions(Hull hull, string bywhat)
        {
            foreach (Rigidbody rb in myParts)
            {
                if (rb == null) continue;
                if (rb.GetComponent<ResetPosOnSpawn>())
                {
                    rb.transform.SetParent(transform);
                    rb.GetComponent<ResetPosOnSpawn>().SetStartPos();
                }
            }
        }
    }
}