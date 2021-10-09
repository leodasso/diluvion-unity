using UnityEngine;
using System.Collections.Generic;
using Diluvion.Ships;
using Diluvion.AI;
using Sirenix.OdinInspector;
using Diluvion.Roll;
using Quests;
using Sirenix.Serialization;
#if UNITY_EDITOR
using UnityEditor;
#endif


namespace Diluvion
{

    public class SpawnPoint : Trigger
    {
        [TabGroup("General")] 
        public bool spawnOnStart;
        
        [TabGroup("General"), Tooltip("Allow more than one spawn in a lifetime.")] 
        public bool allowMultiSpawn = true;

        [TabGroup("General"), Tooltip("Respawn again once my spawn is destroyed")] 
        public bool respawn;

        [TabGroup("General"), ShowIf("respawn"), MinValue(1), Tooltip("How many seconds after being destroyed should I wait to respawn?")]
        public float respawnTime = 30;
        
        [TabGroup("General")]
        [ShowIf("NullSpawnParent"), Tooltip("Spawns will be placed into the parent I'm in.")]
        public bool sameParentAsThis;

        [TabGroup("General")]
        [HideIf("sameParentAsThis")]        
        public Transform spawnParent;

        float _respawnTimer;
        bool _respawning;
        
        [TabGroup("General"), Space]
        [OnValueChanged("SetPlaceHolder"), AssetsOnly]
        [Tooltip("Enter any kind of spawnable entry in here. Chassis, ShipEncounter, ShipBuild")]
        public SpawnableEntry prefabToSpawn;

        [TabGroup("General"), Indent()]
        public PopResources spawnResources = new PopResources(0, 300);

        [TabGroup("General"), Tooltip("Can the destruction of my spawn progress a quest? must select quest AND objective"), Space]
        public bool progressQuest;

        [TabGroup("General"), AssetsOnly, ShowIf("progressQuest"), Indent()] 
        public DQuest quest;

        [TabGroup("General"), ShowIf("progressQuest"), AssetsOnly, AssetList(CustomFilterMethod = "QuestHasObjective", AutoPopulate = false), Indent()] 
        public Objective objective;

        int _lifetimeSpawns;
        
        bool QuestHasObjective(Objective o)
        {
            if (!quest) return false;
            return quest.HasObjective(o);
        }

        [TabGroup("Override")]
        [InfoBox("Wrong Setting type for the prefabToSpawn", InfoMessageType.Error, "MisMatchSettings")]
        [OdinSerialize]
        public SpawnableSettings overrideSettings;

        public event System.Action onSpawnKilled;
        
        [ReadOnly, TabGroup("General")]
        public GameObject placeHolder;
        
        

        AIMono myAI;
        Material _placeHolderMaterial;
        
         #region inspectorCullers

        [Button, HideIf("NullPrefab")]
        void ShowSettings()
        {
            if (NullPrefab()) return;

            string explainString =  "Will spawn <color=yellow>" + prefabToSpawn.name+" </color>("+ prefabToSpawn.GetType().ToString() + ")";

            if(OSettings())
            {
                explainString += ", overwriting with spawn point Settings";
            }
            else if(IsShipBuild())
            {
                explainString += ", overwriting chassis with ShipBuild settings";
            }
            else if (IsChassis())
            {
                explainString += ", using default Chassis settings";
            }

            Debug.Log(explainString, gameObject);
        }


        bool NullSpawnParent()
        {
            return spawnParent == null;
        }      

        bool NullPrefab()
        {
           return prefabToSpawn== null;
        }
       
        public bool IsShipBuild()
        {        
            if (NullPrefab()) return false;
            return prefabToSpawn.GetType() == typeof(ShipBuild);
        }    
        
        public bool IsChassis()
        {           
            if (NullPrefab()) return false;
            return prefabToSpawn.GetType() == typeof(ShipEncounter) || prefabToSpawn.GetType() == typeof(SubChassis);
        }       

        public bool OSettings()
        {
            return overrideSettings != null;
        }

        public bool SpawningShip()
        {
            if (prefabToSpawn == null) return false;
            if (prefabToSpawn.GetType() == typeof(ShipEncounter)) return true;
            if (prefabToSpawn.GetType() == typeof(ShipBuild)) return true;
            if (prefabToSpawn.Prefab().GetComponent<Bridge>()) return true;
            return false;
        }

        public bool MisMatchSettings()
        {
            if (NullPrefab()) return false;
            if (overrideSettings == null) return false;
            if (SpawningShip())
            {
                if (overrideSettings.GetType() != typeof(ShipBuildSettings))
                {
                    return true;
                }
            }
            else
            {
                if (overrideSettings.GetType() != typeof(SpawnableSettings))
                {
                    return true;
                }
            }

            return false;
        }

        #endregion

        protected override void Start()
        {
            if (!Application.isPlaying) return;
            
            base.Start();
            // Remove the placeholder on start
            RemovePlaceHolder();

            if (spawnOnStart) Spawn();
        }

        public override void TriggerAction(Bridge otherBridge)
        {
            base.TriggerAction(otherBridge);
            Spawn();
        }

#if UNITY_EDITOR
        void Update()
        {
            // respawning
            if (_respawning)
            {
                _respawnTimer -= Time.deltaTime;
                if (_respawnTimer < 0)
                {
                    _respawning = false;
                    Spawn();
                }
            }
            
            
            // placeholder selection - select this if the placeholder is selected
            if (!Application.isPlaying)
            {
                if (Selection.activeGameObject != null)
                    if (Selection.activeGameObject.GetComponentInParent<SpawnPoint>() == this)
                        Selection.activeGameObject = gameObject;
            }
        }
        #endif


        /// <summary>
        /// Gets the most relevant settings, the spawnpoint settings override the shipBuild settings
        /// </summary>
        /// <returns></returns>
        public SpawnableSettings CurrentSettings()
        {
            if (overrideSettings == null) return null;
            SpawnableSettings returnSettings = overrideSettings;
            if (SpawningShip())
            {                
                ShipBuild sb = prefabToSpawn as ShipBuild;
                if(sb!=null)                
                    returnSettings = sb.shipBuildSettings;

                returnSettings = overrideSettings as ShipBuildSettings;

            }
            return returnSettings;
        }

        /// <summary>
        /// A unity-event friendly version of the spawn function.
        /// </summary>
        [Button]
        public void SpawnObject()
        {
            Spawn();
        }
    
       
        /// <summary>
        /// Spawns the object that I'm set to spawn.
        /// </summary>
        public GameObject Spawn()
        {
            if (!prefabToSpawn)
            {
                Debug.Log(name + " has no prefab to spawn!", gameObject);
                return null;
            }

            if (_lifetimeSpawns > 0 && !allowMultiSpawn) return null;

            _lifetimeSpawns++;
            
            prefabToSpawn.Process(spawnResources);
            
            // Check which transform to use as a parent
            if (sameParentAsThis) spawnParent = transform.parent;

            // Get the start rotation of the spawn
            Quaternion startRotation = Quaternion.LookRotation(transform.forward, transform.up);
            
            // create an instance of the object to spawn
            GameObject spawnedInstance = prefabToSpawn.Create(transform.position, startRotation, spawnParent);

            // Check if the spawn is destructible. If it is, add SpawnKilled to its delegate so we know when our spawns have been killed.
            var damageable = spawnedInstance.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.onKilled += SpawnKilled;
            }

            return spawnedInstance;
        }

        /// <summary>
        /// Called when the spawn was killed by the onKilled delegate in idestructible. Progresses the objective, if 
        /// there is one. Note that killer may be null.
        /// </summary>
        void SpawnKilled(GameObject killer)
        {
            Debug.Log(name + " sees that my spawn " + prefabToSpawn + " has been killed.");
            onSpawnKilled?.Invoke();

            if (respawn)
            {
                _respawning = true;
                _respawnTimer = respawnTime;
            }
            
            if (!progressQuest) return;
            if (!quest || !objective) return;
            
            objective.ProgressObjective(quest);
        }

        
        #region placeholder
        protected void RemovePlaceHolder()
        {
            if (placeHolder == null) return;

            if (Application.isPlaying)
            {
                Destroy(placeHolder);
            }
            else
            {
                DestroyImmediate(placeHolder);
            }
        }

        void SetPlaceHolder()
        {
            if (placeHolder == null && prefabToSpawn!=null)
                SpawnPlaceholder();
            else
                RemovePlaceHolder();

            if (placeHolder == null && prefabToSpawn != null)
                SpawnPlaceholder();
            
            #if UNITY_EDITOR
            EditorUtility.SetDirty(this);
            #endif
        }

        [Button]
        void SpawnPlaceholder()
        {
            //destroy all previous children of the spawner
            if (placeHolder != null) RemovePlaceHolder();
                
            if (prefabToSpawn.Prefab() == null) return;
            
            
            int sonarLayer = LayerMask.NameToLayer("Sonar");
            int interiorLayer = LayerMask.NameToLayer("Interior");
            _placeHolderMaterial = Resources.Load("placeholder") as Material;

            GameObject newPlaceholder = new GameObject(prefabToSpawn.name + " placeholder");
            newPlaceholder.transform.rotation = prefabToSpawn.Prefab().transform.rotation;
            newPlaceholder.transform.localScale = prefabToSpawn.Prefab().transform.localScale;


            //get all the mesh filters yo
            foreach (MeshFilter mf in prefabToSpawn.Prefab().GetComponentsInChildren<MeshFilter>(true))
            {
                // Omit capsules, masks, mounts, and sonar
                if (mf.name.Contains("capsule") 
                    || mf.name.Contains("mask")
                    || mf.gameObject.layer == sonarLayer
                    || mf.gameObject.layer == interiorLayer
                    || mf.name.Contains("decal")
                    || mf.name.Contains("emblem")
                    || mf.GetComponent<Mount>() != null) continue;

                GameObject newGO = null;
                if (parents.ContainsKey(mf.name))
                    newGO = parents[mf.name].gameObject;
                else
                {
                    newGO = new GameObject(mf.name);
                    parents.Add(mf.name, newGO.transform);
                }

                newGO.transform.SetParent(SetupParentHiearchy(mf.transform, prefabToSpawn.Prefab().transform, newPlaceholder.transform),true);
                newGO.transform.localScale = mf.gameObject.transform.localScale;
                newGO.transform.localPosition = mf.transform.localPosition;
                newGO.transform.localEulerAngles = mf.transform.localEulerAngles;

                if (newGO.GetComponent<MeshFilter>()) continue;
                MeshFilter newMF = newGO.AddComponent<MeshFilter>();
                //add the meshfilter to placeholder
                if (newMF!=null&&mf.sharedMesh != null)                            
                    newMF.sharedMesh = mf.sharedMesh;

                if (newGO.GetComponent<MeshRenderer>()) continue;
                MeshRenderer newMR = newGO.AddComponent<MeshRenderer>();
                if (newMR!=null&&_placeHolderMaterial != null)
                    newMR.sharedMaterial = _placeHolderMaterial;
            }

            //get all the mesh filters yo
            foreach (SkinnedMeshRenderer mf in prefabToSpawn.Prefab().GetComponentsInChildren<SkinnedMeshRenderer>(true))
            {
                // Omit capsules, masks, mounts, and sonar
                if (mf.name.Contains("capsule")
                    || mf.name.Contains("mask")
                    || mf.gameObject.layer == sonarLayer
                    || mf.gameObject.layer == interiorLayer
                    || mf.name.Contains("decal")
                    || mf.GetComponent<Mount>() != null) continue;

                GameObject newGO = null;
                if (parents.ContainsKey(mf.name))
                    newGO = parents[mf.name].gameObject;
                else
                {
                    newGO = new GameObject(mf.name);
                    parents.Add(mf.name, newGO.transform);
                }

                newGO.transform.SetParent(SetupParentHiearchy(mf.transform, prefabToSpawn.Prefab().transform, newPlaceholder.transform), true);
                newGO.transform.localScale = mf.gameObject.transform.localScale;
                newGO.transform.localPosition = mf.transform.localPosition;
                newGO.transform.localEulerAngles = mf.transform.localEulerAngles;


                //add the meshfilter to placeholder
                if (mf.sharedMesh != null)
                {
                    MeshFilter newMF = newGO.AddComponent<MeshFilter>();
                    newMF.sharedMesh = mf.sharedMesh;
                }

                MeshRenderer newMR = newGO.AddComponent<MeshRenderer>();
                newMR.sharedMaterial = _placeHolderMaterial;
            }

            newPlaceholder.transform.parent = transform;
            newPlaceholder.transform.localPosition = Vector3.zero;
            newPlaceholder.transform.rotation = transform.rotation;
            parents.Clear();
            placeHolder = newPlaceholder;
            
#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif
        }

        /// <summary>
        /// Sets up the proper hierarchy for the input transform in relation to the top parent
        /// </summary>    
        /// <returns>the proper parent for the input transform</returns>      
        Dictionary<string, Transform> parents = new Dictionary<string, Transform>();
        Transform SetupParentHiearchy(Transform input, Transform topParent, Transform topInstance)
        {
            if (input == null)  return topInstance;
            if (input.parent == null) return topInstance;
            if (input.parent == topParent) return topInstance;
        
            Transform inputParent = input.parent;
            Transform newParent = null;
            if (parents.ContainsKey(inputParent.name))
                newParent = parents[inputParent.name];
            else
            {
                newParent = new GameObject(inputParent.name).transform;
                parents.Add(inputParent.name, newParent);
            }

            Transform hiearchyParent = SetupParentHiearchy(inputParent, topParent, topInstance);
            newParent.SetParent(hiearchyParent, true);
            newParent.localScale = inputParent.localScale;       
            newParent.localPosition = inputParent.localPosition;
            newParent.localRotation = inputParent.localRotation;
            return newParent;              
        }

        
        #endregion
    }
}