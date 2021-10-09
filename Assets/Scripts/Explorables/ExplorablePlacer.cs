using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Diluvion;
using SpiderWeb;
using Diluvion.Roll;
//using DUI;
using Sirenix.OdinInspector;


[System.Serializable]
public class ExplorablePlacer : MonoBehaviour, ITransformRoller
{
    [BoxGroup("Local settings")]
    [Tooltip("Longest time until this despawns")]
    public float destroyTime = 60;

    [BoxGroup("Local settings")]
    [Tooltip("Percentage of the radius (longest side) we want to use to scatter")]
    public float scatterRadius = 1;

    [BoxGroup("Local settings")]
    [Tooltip("Size of this placer.")]
    public SpawnableSize size;

    [BoxGroup("Local settings"), SerializeField]
    [Tooltip("Specific tags of this placer.")]
    List<Tag> _tags = new List<Tag>();
    
    [FoldoutGroup("Debug Values"), ReadOnly]
    public bool playerIsInRange;
    
    [FoldoutGroup("Debug Values"), ReadOnly]
    public bool attemptedSpawnThisSession;

    [FoldoutGroup("Debug Values"), ReadOnly]
    public float lastTimeUpdated = -99;

    [FoldoutGroup("Debug Values")]
    [Tooltip("Currently Used table to roll against, will check its parent if this is null")]
    public Table spawnTable;
    
    /// <summary>
    /// Resources used for testing. These don't get used up, but are used for spawning in the editor.
    /// </summary>
    //[FoldoutGroup("Debug Values", false), SerializeField, Title("Test Roll Resources"), DisableInPlayMode]  
    //PopResources _testResources = new PopResources(0,0);

    /// <summary>
    /// The actual current working resources. These get depleted when stuff is spawned.
    /// </summary>
    [FoldoutGroup("Debug Values"), Title("Current Rolling Resources"), SerializeField]
    [Tooltip("The resources given to this placer to populate with. Doesn't deplete.")]
    public PopResources myRollResources = new PopResources(0,0);
    
    /// <summary>
    /// The resources currently available for population
    /// </summary>
    PopResources _currentRollRes = new PopResources();
    
    [FoldoutGroup("Debug Values"), SerializeField, DisableInPlayMode]
    List<GameObject> _spawnedObjects = new List<GameObject>();
    [FoldoutGroup("Debug Values"), SerializeField]
    List<GameObject> _staticObjects = new List<GameObject>();

    /// <summary>
    /// Player must be within this distance to the instance for it to activate.
    /// </summary>
    const float DistanceToActivate = 120;

    /// <summary>
    /// Query working value for population
    /// </summary>
    int _currentPopulationBudget = 0;
    int _myIndex;
    float _lastTimeSpawnAttempted = 0;
    float _time;
    bool _visiting;
    bool _actedInUpdate;
    ResourceZone _resourceZone;
    
    /// <summary>
    /// Safe get function for the most relevant dock table, last resort being the TableHolders's
    /// </summary>
    Table GetTable<T> () where T : Entry
    {
        if (spawnTable != null && spawnTable.ContainsRollType(typeof(T))) return spawnTable;

        spawnTable = GetComponentInParent<TableHolder>().Table(typeof(T));
        if (!spawnTable) Debug.Log("Could not find a table containing type: " + typeof(T));
        return spawnTable;
    }
    
    #region range activation

    public static int activeIndex = 0;
    static List<ExplorablePlacer> _allPlacers = new List<ExplorablePlacer>();

    /// <summary>
    /// Gets the closest placer to input transform.
    /// </summary>
    /// <param name="transformToCheck"> The transform to check against.</param>
    /// <param name="closestAmount">Closest x hits to return.</param>
    /// <returns>Closest x Explorableplacers to input transform.</returns>
    public static List<ExplorablePlacer> ClosestPlacersTo(Transform transformToCheck, int closestAmount = 1)
    {    
        List<ExplorablePlacer> sortList  = new List<ExplorablePlacer>(_allPlacers);

        sortList.OrderBy(e => Vector3.Distance(e.transform.position, transformToCheck.position)).ToList();
        List<ExplorablePlacer> takeList  =sortList.Take(closestAmount).ToList();
        return takeList;
    }

    void Start ()
    {
        _resourceZone = GetComponentInParent<ResourceZone>();
        if (!_resourceZone)
        {
            //Debug.LogError("No resource zone could be found for " + name, gameObject);
            enabled = false;
        }
        
        gameObject.layer = LayerMask.NameToLayer("Tools");
        if (GetComponent<Renderer>()) GetComponent<Renderer>().enabled = false;
    }


    void OnEnable()
    {
        RegisterPlacer();
    }

    void OnDisable()
    {
        UnRegisterPlacer();
    }
    

    void Update()
    {

        if (!playerIsInRange && IsPlayerInRange(DistanceToActivate))
        {
            PlayerEnteredRadius();
            playerIsInRange = true;
            return;
        }

        if (playerIsInRange && !IsPlayerInRange(DistanceToActivate))
        {
            PlayerLeftRadius();
            playerIsInRange = false;
        }
    }


    void PlayerEnteredRadius()
    {
        // Roll for population
        float spawnRoll = Random.Range(0, 1);
        if (spawnRoll > _resourceZone.placerChance) return;

        Populate(_resourceZone.resources);
    }

    void PlayerLeftRadius()
    {
        DestroyAllStaticObjects();
    }

    /// <summary>
    /// Shorthand for checking player range
    /// </summary>  
    bool IsPlayerInRange (float range)
    {
        return PlayerManager.IsPlayerInRange(transform, range);
    }
    

    void RegisterPlacer ()
    {
        _allPlacers.Add(this);
    }

    void UnRegisterPlacer()
    {
        _allPlacers.Remove(this);
    }


    #endregion

    #region debug
    
    struct TestSphere
    {
        public Color col;
        public Vector3 position;
        public float radius;

        public TestSphere (Color c, Vector3 pos, float r)
        {
            col = c;
            position = pos;
            radius = r;
        }
    }
    
        
    void AddTestSphere (Color col, Vector3 position, float radius)
    {
        _testSpheres.Add(new TestSphere(col, position, radius));
    }
    
    /// <summary>
    /// Clears out the spheres used for debugging.
    /// </summary>
    void ClearSpheres ()
    {
        _testSpheres.Clear();
    }

    List<TestSphere> _testSpheres = new List<TestSphere>();
    void OnDrawGizmos ()
    {
        Gizmos.color = new Color(0, 1, 0, .1f);
        Matrix4x4 rotMatrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
        Gizmos.matrix = rotMatrix;
        Gizmos.DrawWireCube(Vector3.zero, WorldTools.ExplorableBox(size));

        Gizmos.color = new Color(0, 1, 0, .02f);
        Gizmos.DrawCube(Vector3.zero, WorldTools.ExplorableBox(size));


    }

    Vector3 _lastPosition;
    Vector3 _lastDirection;
    void OnDrawGizmosSelected ()
    {
        foreach (TestSphere ts in _testSpheres)
        {
            Gizmos.color = ts.col;
            Gizmos.DrawWireSphere(ts.position, ts.radius);
        }

        if (_lastPosition != transform.position)
        {
            _lastPosition = transform.position;
            _hitNothing = false;
            _explorableOrientation = Vector3.zero;
            _lastDirection = TerrainDir();
        }
        DrawDirection(_lastDirection);
    }

    /// <summary>
    /// Draws gizmo rays in the given direction
    /// </summary>
    /// <param name="dir">Direction to draw the rays towards</param>
    void DrawDirection (Vector3 dir)
    {
        Gizmos.color = Color.green;
        float s = 5f * (int)size;

         Gizmos.DrawRay(transform.position + transform.forward * s, dir * s);
        Gizmos.DrawRay(transform.position - transform.forward * s, dir * s);
        Gizmos.DrawRay(transform.position + transform.right * s / 2, dir * s);
        Gizmos.DrawRay(transform.position - transform.right * s / 2, dir * s);
    }

    #region inspector functions and buttons

    [ButtonGroup("spawn"), PropertyTooltip("Searches for resources in scene. if none found, uses test resources.")]
    public void TestRollAndCreate ()
    {
        Clear();
        _hitNothing = false;
        _explorableOrientation = Vector3.zero;

        // populate using the test resources
        ResourceZone zone = GetComponentInParent<ResourceZone>();
        if (zone)
        {
            Debug.Log("Testing populating " + name + " with resources from " + zone.name);
            Populate(zone.resources);
        }
        else Debug.LogError("There's no resourceZone component in my ancestry. Can't test populate!");//Populate(_testResources);
    }
    
   	[ButtonGroup("placement")]
    void PlaceOnGround ()
    {
        MatchGround(-1);
    }

    [ButtonGroup("placement")]
    void PlaceOnCeiling ()
    {
        MatchGround(1);
    }

    [ButtonGroup("spawn")]
    void Clear ()
    {
        foreach (GameObject go in _spawnedObjects)
        {
            if (go == null) continue;
            DestroyImmediate(go.gameObject);
        }
        foreach (GameObject go in _staticObjects)
        {
            if (go == null) continue;
            DestroyImmediate(go.gameObject);
        }
        _spawnedObjects.Clear();
        _staticObjects.Clear();
    }
    #endregion
    
    void MatchGround (int direction)
    {
        Ray ray = new Ray(transform.position, Vector3.up * direction);

        //cast ray down and check all colliders, and order them by distance hit
        RaycastHit[] allHit = Physics.RaycastAll(ray, 800).OrderBy(hit => hit.distance).ToArray<RaycastHit>();


        foreach (RaycastHit newHit in allHit)
        {
            // ignore tools layer  
            if (newHit.collider.gameObject.layer == LayerMask.NameToLayer("Tools")) continue;

            transform.rotation = Quaternion.LookRotation(newHit.normal);
            transform.Rotate(90, 0, 0, Space.Self);
            transform.position = newHit.point + newHit.normal;
            float height = (int)size * 5.0f * -direction;

            //if placing on cieling, rotate 180 so it's still facing up
            if (direction > 0)
                transform.Rotate(new Vector3(0, 0, 180), Space.Self);
            transform.Translate(new Vector3(0, height / 2, 0), Space.Self);

            break;
        }
    }

    #endregion

    #region RollingMethods
    
    List<Tag> _tempTags = new List<Tag>();
    public List<Tag> RollingTags
    {
        get {
            if (_tempTags.Count < 1)
            {
                _tempTags = _tags;
            }
            return _tempTags;
        }
        set { _tempTags = value; }
    }

    /// <summary>
    /// Safe add tags makes sure no overlap is happening in the
    /// </summary>
    public void CombineTagList (List<Tag> ts)
    {

        RollingTags = new List<Tag>(_tags);
        foreach (Tag t in ts)
        {
            if (!_tags.Contains(t))
                RollingTags.Add(t);
        }
    }

    public Transform Roller ()
    {
        return transform;
    }
    
    /// <summary>
    /// The requirements for a valid explorable for this placer are described in the query. used with Table's TagFilteredList function
    /// </summary>
    public bool RollQuery (Entry obj)
    {
      
        SpawnableEntry spawnObj = obj as SpawnableEntry;
        if (spawnObj == null)// If its not a SpawnAbleEntry or a table containing a spawnableEntry, return false
        {
            Table tab = obj as Table;
            if (tab != null) //Checkif its a table, and if it contains spawnableEntries
            {
                if (!tab.ContainsAssignable(typeof(SpawnableEntry)))
                {
                    return false;
                }
                else
                {
                    return true;
                    //tab.Roll<SpawnableEntry>(RollQuery); // If it is a table, roll that table instead
                }
            }
            else
            {
                return false;
            }
        }
        
        ShipEncounter ship = spawnObj as ShipEncounter;
        if (ship != null)
        {
            /*
            if (!PopulationManager.Get().CanISpawnShip())
            {
                Debug.Log("Cant Spawn a ship at the moment: " + spawnObj, spawnObj);
                return false;
            }
            */
        }

        //Checks the size against the current roll size
        if (!spawnObj.ThinnerOrEqualWidth(Mathf.Clamp((int)CurrentRollSize(), 1, 50)))
        {
            //Debug.Log("too fat", spawnObj);
            return false;
        }

        if (!spawnObj.CanSpawnDirection(TerrainDir()))
        {
            //Debug.Log("direction wrong", spawnObj);
            return false;
        }

        //Filters by danger
        if (!spawnObj.CanAfford(_currentRollRes))
        {
            Debug.Log("Cant afford, Wallet: (" + _currentRollRes + "), Price: (V: " + spawnObj.Value()+" D: " + spawnObj.Danger() +  ")", spawnObj);
            return false;
        }

      // Debug.Log("Legal   " + spawnObj.name + " Size: " + (int)spawnObj.size + " / "+CurrentRollSize() + " Cost: " +  (int)spawnObj.Size() + " / " + currentPopulationBudget);
        return spawnObj.MatchRollerTags(this);
    }

    Vector3 _explorableOrientation = Vector3.zero;
    bool _hitNothing;
    /// <summary>
    /// Identifies the closest Terrain surface if there is one
    /// </summary>
    Vector3 TerrainDir ()
    {

        if (_hitNothing) return Vector3.zero;
        if (_explorableOrientation != Vector3.zero) return _explorableOrientation;

        _hitNothing = true;
        float topDistance = 999;
        float botDistance = 999;
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.up, out hit, 5f * (int)size, Calc.IncludeLayer("Terrain")))
        {
            topDistance = hit.distance;
            _hitNothing = false;
        }

        if (Physics.Raycast(transform.position, -transform.up, out hit, 5f * (int)size, Calc.IncludeLayer("Terrain")))
        {
            botDistance = hit.distance;
            _hitNothing = false;
        }

        //If ground distance is closer
        if (topDistance > botDistance)
            _explorableOrientation = -transform.up;
        else if (topDistance < botDistance)
            _explorableOrientation = transform.up;
        else
            _explorableOrientation = Vector3.zero;


        return _explorableOrientation;
    }
    #endregion


    /// <summary>
    /// Populates the placer with a population budget
    /// </summary>
    void Populate (PopResources populationBudget)
    {
        StartCoroutine(PopulateLoop(populationBudget));
    }

    IEnumerator PopulateLoop(PopResources budget)
    {
        ClearSpheres();
        
        myRollResources = new PopResources(budget);
        _currentRollRes = new PopResources(myRollResources);
    
        //PopResources usedRes = new PopResources();
        Debug.Log("Attempting Population with " + myRollResources);

        
        // Will attempt as many as Size times to populate something
        int spawnObjects = Mathf.Clamp(Random.Range((int)size/4, (int)size),1, 8);
        
        // to populate as long as there is popbudget left
        float availableSpace  = spawnObjects;
        
        while (availableSpace > 0)
        {
            Vector3 legalPosition = Vector3.zero;
            Quaternion floorRotation = Quaternion.identity;
            
            // Find an entry to spawn from the table. If it's null, it means nothing else can be spawned, and we should 
            // break the loop.
            SpawnableEntry spawnableEntry = FindAndPlace<SpawnableEntry>(ref legalPosition, ref floorRotation); 
            if (spawnableEntry == null)
            {
                Debug.Log("Could not find a spawnableEntry, breaking Resource Loop");
                break;
            }
            
            // Instantiate the entry
            GameObject newInstance = InstantiateEntry(spawnableEntry, legalPosition, floorRotation);
            if (newInstance == null) continue;

            availableSpace -= spawnableEntry.slotSize;
          
            // place the instance in the same parent as this
            newInstance.transform.parent = transform.parent;
          
            // find all the room placers in the new spawned instance, and set their values for gold and danger.
            foreach (InteriorManager im in InteriorManager.GetAll(newInstance))
            {
                RoomPlacer roomPlacer = im.GetComponentInChildren<RoomPlacer>();
    
                if (roomPlacer) roomPlacer.PrepPopulation(_currentRollRes);
                
                //usedRes.value += _currentRollRes.value;

                yield return null;
            }

            Spawnable spawnable = newInstance.GetComponent<Spawnable>();
            if (spawnable)
            {
                spawnable.currentResources = new PopResources(_currentRollRes);
                _currentRollRes -= spawnableEntry.resourceCost;
            }
            
            if(newInstance.GetComponent<Explorable>()) _staticObjects.Add(newInstance);
            _spawnedObjects.Add(newInstance);

            yield return null;
        }
        
        // Only use value from resources baed on interior spawn
        //myRollResources = usedRes;
    }


    public float CurrentRollSize()
    {
        return (int)size * 5;
    }


    /// <summary>
    /// Roll And Create without tags, Returns the size score of the spawned one
    /// </summary>
    /// <returns>The Spent Population budget</returns>
    T FindAndPlace<T> ( ref Vector3 legalPosition, ref Quaternion floorRotation) where T : SpawnableEntry
    {
        Table table = GetTable<T>();
        if (!table)
        {
            Debug.LogError("No table could be found which contains type: " + typeof(T));
            return null;
        }

        T proposedSpawn = (T)table.Roll<T>(RollQuery); //(T)GetTable<T>().Roll<T>(RollQuery);

        int rollTries = 0;
        
        // Loop to find spot for proposed spawn, it will try several times to find a spot for the rolled shclomo 
        // before rolling something else, slightl smaller

        if (proposedSpawn == null)
        {
            Debug.Log("No Proposed Spawn  Check for <b>Empty </b>, or <b>no matching entries</b> to the <color=green> " + 
                      "RollQuery </color> in this <color=cyan> Table </color>", gameObject);
            return null;
        }

        int rollSize = (int)CurrentRollSize();

        while (!proposedSpawn.ValidPosition(transform, rollSize, out legalPosition, out floorRotation))
        {
            //we tried populating with Current size and couldnt find space, 
            if (rollTries < 4)
            {
                if (rollSize > 1)
                    rollSize /= 2;
                rollTries++;                
            }
            else return null;

            //If we couldnt find a position, reduce the size and roll again    
            proposedSpawn = (T)GetTable<T>().Roll<T>(RollQuery);
            if (proposedSpawn == null) break; 
        }
       
        return proposedSpawn;
    }

    
    /// <summary>
    /// Instantiates an instance of the game object related to the given spawnable entry.
    /// </summary>
    /// <param name="et">The spawnable entry to create an instance of.</param>
    /// <param name="etPosition">Position to place the entry at.</param>
    /// <param name="etRotation">Rotation to place the entry at.</param>
    /// <returns>The gameobject of the newly created instance.</returns>
    GameObject InstantiateEntry (SpawnableEntry et, Vector3 etPosition, Quaternion etRotation)
    {
        //If it is a roller
        IRoller isSpawnARoller = et as IRoller;

        isSpawnARoller?.CombineTagList(RollingTags);

        //After we have solved all the spatial needs of what we are going to spawn, we do some extra processing on it
        //Debug.Log("Attempting Process with " + rollResources);
        et = et.Process(myRollResources);

        if (et == null)
        {
            Debug.Log("Something went wrong with the processing on: " + et.name);
            return null;
        }
        
        //Our Object is ready to spawn, call create on it       
        return et.Create(etPosition, etRotation, transform.parent);
    }

    /// <summary>
    /// waits several seconds(Resets when revisited) before removing this explorable, 
    /// deactivating and activating a new random placer somewhere
    /// </summary>
    IEnumerator WaitAndDestroy ()
    {
        while (_time < destroyTime)
            yield return new WaitForSeconds(2);

        DestroyAllStaticObjects();
    }

    /// <summary>
    /// Destroys all objects that were spawned by this placer
    /// </summary>
    public void DestroyAllSpawns ()
    {
        foreach (GameObject go in _spawnedObjects)
        {
            if (go == null) continue;
            Destroy(go.gameObject);
        }
        _spawnedObjects.Clear();

    }
    
    /// <summary>
    /// Destroys all objects that were spawned by this placer
    /// </summary>
    void DestroyAllStaticObjects ()
    {
        foreach (GameObject go in _staticObjects)
        {
            if (go == null) continue;
            Destroy(go.gameObject);
        }
        _staticObjects.Clear();

    }
}
