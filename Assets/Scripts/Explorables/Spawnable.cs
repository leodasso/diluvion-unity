using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Diluvion;
using Sirenix.OdinInspector;
using Diluvion.Roll;
using SpiderWeb;
#if UNITY_EDITOR
using UnityEditor;
    #endif
public enum SpawnableSize
{
    Small = 1,
    Medium = 2,
    Large = 4,
    Huge = 8
}

namespace Diluvion
{

    /// <summary>
    /// Helper component that attaches to the spawnable prefab to keep track of reference to the scriptable object.
    /// <see cref="SpawnableEntry"/>
    /// </summary>
    public class Spawnable : MonoBehaviour
    {
        [TabGroup("References"), InlineEditor( Expanded = true), AssetsOnly]
        public SpawnableEntry entry;
        
        [TabGroup("References"), ReadOnly]
        public GameObject prefabRef;

        public PopResources currentResources = new PopResources(0,0);

        [TabGroup("BoundingValues")]
        public bool useBoundingBasePos;

        [TabGroup("BoundingValues")]
        public float width;
        
        [TabGroup("BoundingValues")]
        public float height;
        
        [TabGroup("BoundingValues")]
        public Vector3 localRoot;
        
        [TabGroup("BoundingValues")]
        public Vector3 localCenter;
        
        [TabGroup("BoundingValues")]
        public Vector3 offsetFromFloor = Vector3.zero;

        public bool ceiling;
        protected Bounds b;

        protected virtual string EntryAssetPath()
        {
            return "Assets/Prefabs/RollTableObjects";
        }
        
        #if UNITY_EDITOR
        
        [Button("Get Entry")]
        public void CreateDRollObject()
        {
            Debug.LogWarning("Note that ALL explorable entries must be in the correct path: " + EntryAssetPath());
        
            SpawnableEntry foundObj = ScriptableObjectUtility.GetAsset<SpawnableEntry>(EntryAssetPath(), name);

            if (foundObj)
            {
                Debug.Log("Spawnable entry for this object was found! No need to create a new one.");
                entry = foundObj;
                EditorUtility.SetDirty(this);
                return;
            }

            if (entry == null)
            {
                // Check the folder for the asset first
                entry = ScriptableObjectUtility.GetAsset<SpawnableEntry>(EntryAssetPath(), name);
                if (entry != null)
                {
                    EditorUtility.SetDirty(this);
                    return;
                }

                entry = CreateEntryAsset();
            }
            else
            {
                entry.prefab = gameObject;
            }
            
            EditorUtility.SetDirty(this);
            EditorUtility.SetDirty(entry);
        }

        protected virtual SpawnableEntry CreateEntryAsset()
        {
            return ScriptableObjectUtility.CreateSpawnableAsset<SpawnableEntry>(gameObject);
        }
        
        #endif

        public virtual void SetDanger(int newDanger)
        {
            danger = newDanger;
        }
        
        [SerializeField]
        protected int danger;
        public virtual int Danger()
        {
            if (danger != 0) return danger;
           
            return danger = currentResources.danger;
        }
        
        protected virtual void OnDrawGizmosSelected()
        {
            if (b != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireCube(localCenter + transform.position, new Vector3(width, height, width));
            }
        }
        
        #if UNITY_EDITOR
        
        [Button()]
        void PrepSpawnable()
        {
           // currentResources.techCost = 0;
            PrefabRef();
            GetBounds();
            //TechCost();
        }
     
        void PrefabRef()
        {
            if (!Application.isPlaying)
                if (prefabRef == null)
                    prefabRef = PrefabUtility.GetPrefabParent(gameObject) as GameObject;
        }

        List<Transform> children = new List<Transform>();

        [Button()]
        void MoveToOrientation()
        {
            children.Clear();
            //Deparent children
            for(int i=0; i<transform.childCount; i++)
            {
                Transform child = transform.GetChild(i);
                children.Add(child);               
            }

            foreach (Transform t in children)
                t.SetParent(null, true);
            transform.position = Root();
            foreach (Transform t in children)
                t.SetParent(transform, true);

            PrepSpawnable();
        }

        #endif

        /// <summary>
        /// Gets the bounds based on the children's colldiers, and stores the relevant data
        /// </summary>
        protected virtual void GetBounds()
        {
            b = new Bounds(transform.position, Vector3.zero);
            foreach (Collider c in GetComponentsInChildren<Collider>(true))
            {
                if (c.isTrigger) continue;
                b.Encapsulate(c.bounds);
            }

            height = b.size.y;
            width = Mathf.Max(b.extents.x, b.extents.z);
            localCenter =  b.center- transform.position;
            localRoot = localCenter;
        }

        /// <summary>
        /// Gets the spawnPoint of this spawnable
        /// </summary>
        public virtual Vector3 Root()
        {
            return BoundingCenter() + offsetFromFloor;
        }

        /// <summary>
        /// Local modifiable Root
        /// </summary> 
        protected virtual Vector3 BoundingCenter()
        {
            return transform.position + localRoot;
        }
      
        /// <summary>
        /// Fancy technical cost calculator for this spawnable if it has not already been set
        /// </summary>       
        /*public float TechCost()
        {
            if (currentResources.techCost != 0) return currentResources.techCost;

            float maxCost = 100;
           
            float halfPoint = 50;

            float value = GetComponentsInChildren<MeshRenderer>().Length;

            return currentResources.techCost = Mathf.RoundToInt(Calc.LogBase(value, halfPoint, maxCost));            
        }*/

        /// <summary>
        /// Safe Get for the Height of this spawnable
        /// </summary>
        public float Height()
        {
            if (height != 0) return height;
            GetBounds();
            return height;
        }

        /// <summary>
        /// Safe get for the width of this spawnable
        /// </summary>
        public float Width()
        {
            if (width != 0) return width;
            GetBounds();
            return width;
        }

        public virtual bool Engaged ()
        {
            return true;
        }
    }
}
