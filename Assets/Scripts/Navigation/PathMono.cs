using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Diluvion;
using SpiderWeb;
using Sirenix.OdinInspector;
#pragma warning disable 618

[ExecuteInEditMode]
[System.Serializable]
public class PathMono : MonoBehaviour 
{
    [InfoBox("Alt+s for path menu.\n Gray line means obstructed. \n Red line means no mutual connection")]
    [SerializeField]
    public List<PathMono> connectedPoints;


    
    [SerializeField]
    public List<PathMono> safePoints;
    
#if UNITY_EDITOR
    
    [ButtonGroup("connections")]
    void ShowConnections()
    {
        _showConnections = true;
    }

    [ButtonGroup("connections")]
    void HideConnections()
    {
        _showConnections = false;
    }
    
    SerializedObject connectedPointsProp;//TODO Make UNDO Proof with serializedProperty version of connectedPoints!A VB
    #endif
    
    [SerializeField]
    [HideInInspector]
    public List<PathMono> Neighbours // Main public access to connectedPoints 
    {
        protected set
        {
            safePoints = value;
        }
        get
        {
            return safePoints;
        }
    }



    static int count = 0;
    LayerMask mask;
    
    #if UNITY_EDITOR

    void OnEnable()
    {
        if (Application.isPlaying) return;      
        if (instanceID == 0)
        {
            instanceID = GetInstanceID();    
            return;
        }
        if (gameObject.tag != "PathNode")
            gameObject.tag = "PathNode";

        mask = LayerMask.GetMask("Terrain");
    }

#endif


    void OnDestroy()
    {
        count--;
        List<PathMono> deathConnected = new List<PathMono>(connectedPoints);
        foreach(PathMono pm in deathConnected)
        {
            pm.Disconnect(this);
            pm.SafeRemove(this);
        }
    }

    //List<PathMono> removalList = new List<PathMono>();
    
	public void RenameToIndex(GameObject target, int index)
	{
		if (target == null) return;
		target.name = target.transform.parent.name + " WP " + index;
	}

    public void Remove()
    {
        Remove(this);
    }

    public void Remove(PathMono pw)
	{
        if (pw == this) return;
        if (!connectedPoints.Contains(pw)) return;
        //Debug.Log("Removing " + this.name + " from " + pw.name);
        connectedPoints.Remove(pw);
   

    }

    public void Disconnect(PathMono pw)
    {
        Remove(pw);
        pw.Remove(this);
    }
    static PathMono defaultObject;
    /// <summary>
    /// Gets the pathobject from resources
    /// </summary>
    static PathMono DefaultNode()
    {
        if (defaultObject!=null) return defaultObject;
        defaultObject = Resources.Load<PathMono>("pathObject");
        return defaultObject;
    }

    /// <summary>
    /// Basic Create Node
    /// </summary>
    public static PathMono CreateNode()
    {
        PathMono newWP = (PathMono)Instantiate(DefaultNode());
        newWP.name = DefaultNode().name +" "+ count;
        count++;
        return newWP;
    }

    /// <summary>
    /// Create Node at Position
    /// </summary>
    public static PathMono CreateNode(Vector3 position)
	{
        PathMono newWP = CreateNode();
        newWP.transform.position = position;
        return newWP;
	}

    /// <summary>
    /// Basic Constructor
    /// </summary>  
	public void ChamferVertex()
	{
		Split();
	}

    public List<PathMono> Split()
	{
		List<PathMono> returnList = new List<PathMono>(); 
		//Move the points a little away from each other for easy selection
		if (connectedPoints == null) return null;

		foreach (PathMono pw in connectedPoints)
		{
			if (pw == null) continue;
			SplitTowards(pw, 0.25f);
			returnList.Add(pw);
		}    

		return returnList;
	}

	//Creates a waypoint halfway between this point and the target
	public PathMono SplitTowards(PathMono target, float distancePercent)
	{      
		PathMono returnWP = CreateNode(transform.position + (target.transform.position - transform.position) * distancePercent);
		returnWP.connectedPoints = new List<PathMono>();
		returnWP.transform.parent = transform.parent;
		Insert(returnWP, target);
		return returnWP;
	}

	//properly sets up points between this PathMono, an inserted one, and the target
	public void Insert(PathMono toInsert, PathMono target)
	{
		toInsert.connectedPoints = new List<PathMono>();

        //Setting up return waypoint targets
        toInsert.AddPoint(this);
		toInsert.AddPoint(target);

		//Setting up target waypoints
		target.AddPoint(toInsert);
		target.connectedPoints.Remove(this);

		//setting up my neighbours
		AddPoint(toInsert);
		connectedPoints.Remove(target);       

	}

    public bool IsConnected(PathMono point)
    {
        if (connectedPoints.Count < 1) return false; 
        return connectedPoints.Contains(point);
    }

    
    public void Connect(PathMono point)
    {
        if (point == null) return;
        point.AddPoint(this);
        AddPoint(point);
       // Debug.Log("Joining " + point.name + " and " + this.name);
    }

	//Avoid adding the point twice
	public void AddPoint(PathMono point)
	{
        if (point == this) return;
		if (connectedPoints.Contains(point)) return; 
		connectedPoints.Add(point);     
    }

    public void ConnectRange(List<PathMono> points)
    {
        foreach (PathMono pm in points)
            Connect(pm);
    }

	//For adding a range of points without including myself
	public void AddRange(List<PathMono> points)
	{
		foreach (PathMono pw in points)
			AddPoint(pw);
	}

	public void RemoveConnection(PathMono p)
	{
		Remove(p);
        SafeRemove(p);
		p.Remove(this);
        p.SafeRemove(this);
	}

	//For replacing another waypoint with this
	public void Replace(PathMono other)
	{
		AddRange(other.connectedPoints);
        foreach (PathMono p in other.connectedPoints)
        {
            if (p == null) continue;
            p.AddPoint(this);
        }
        GameObject.Destroy(other.gameObject);
	}

    public void SafeRemove(PathMono m)
    {
        if (safePoints.Contains(m))
            safePoints.Remove(m);
        if (m.safePoints.Contains(this))
            m.safePoints.Remove(this);
    }

    public void SafeAdd(PathMono m)
    {
        if (!safePoints.Contains(m))
            safePoints.Add(m);
        if (!m.safePoints.Contains(this))
            m.safePoints.Add(this);
    }

    public bool SetupCorrectly()
	{
		if (connectedPoints == null) return false;
		if (connectedPoints.Count < 1) return false;

		foreach (PathMono otherPoint in connectedPoints) {

			//if the other point doesn't exist, there's an error
			if (otherPoint == null) return false;

			//if the other point doesnt have this point, there's an error
			if (!OtherPointHasThis(otherPoint)) return false;
		}
		return true;
	}

	bool OtherPointHasThis(PathMono otherPoint)
	{
		if (otherPoint == null) return false;

		//check if the other Waypoint links this
		List<PathMono> otherPointConnections = otherPoint.connectedPoints;

		if (otherPointConnections == null) return false;

		//if everything's hooked up right, show the line as Cyan
		foreach (PathMono otherPoint2 in otherPointConnections) 
			if (otherPoint2 != null)
			if (otherPoint2 == this)
				return true;			

		return false;
	}

#if UNITY_EDITOR

    //catch duplication of this GameObject
    [SerializeField]
    public int instanceID = 0;
    [SerializeField]
    [HideInInspector]
    PathMono me;
    void Awake()
    {     
        if (Application.isPlaying)
            return;
        
        if (instanceID != GetInstanceID()&& GetInstanceID() < 0)
        {
            connectedPoints =new List<PathMono>();
            safePoints = new List<PathMono>();
           // Debug.LogError("Detected Duplicate!",gameObject);          
            me = EditorUtility.InstanceIDToObject(instanceID)as PathMono;
            instanceID = GetInstanceID();
            if (me!=null)
                Connect(me); 
        }
      
    }


    static bool _showConnections;
    
    protected virtual void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 1.5f);
    }

    protected virtual void OnDrawGizmos()
    {
        if (_showConnections)
            ColorLines(new Color(0, 1, 0, .25f));
    }
 

        /// <summary>
        /// Needs to be called every time a safe connection is broken/reconnected to maintain a good list
        /// 
        /// </summary>
    void CheckLOS()
    {
        foreach (PathMono otherPoint in connectedPoints)
        {
            if (otherPoint == null) continue;

            if (Physics.Raycast(transform.position, otherPoint.transform.position - transform.position, Vector3.Distance(otherPoint.transform.position, transform.position), mask))
                SafeRemove(otherPoint);
            else
                SafeAdd(otherPoint);
        }
    }

    bool InLOS(Transform t)
    {
        if (!NavigationManager.Get().hideLOSConnections) return true;
        if (Camera.current == null) return false;
        if (t == null) return false;
        Vector3 camPos = Camera.current.transform.position;
        Vector3 pointDir = t.position - camPos;
        float distance = Vector3.Distance(t.position, Camera.current.transform.position);
        if (Physics.Raycast(camPos, pointDir,distance, LayerMask.GetMask("Terrain")))
            return false;
        return true;
    }
    
    
    //bool brokeLine = false;
    Vector3 oldPosition;
    protected void ColorLines(Color color)
    {        
        if (!InLOS(transform)) return;
        if (connectedPoints == null) return;
        //if there's any points connected to this one, draw a gizmo line to it.
        if (connectedPoints.Count < 1) return;
       
        foreach (PathMono otherPoint in connectedPoints)
        {
            if (otherPoint == null) continue;
            if (!InLOS(otherPoint.transform)) continue;
          
                
            Gizmos.color = Color.red;

            if (OtherPointHasThis(otherPoint))
            {
                Gizmos.color = color;
            }

            if (Physics.Raycast(transform.position, otherPoint.transform.position - transform.position, Vector3.Distance(otherPoint.transform.position, transform.position), mask))
            {
                Gizmos.color = new Color(0.1f, 0.1f, 0.1f, 0.5f);
              
            }       
               
            Gizmos.DrawLine(transform.position, otherPoint.transform.position);               
        }
        List<PathMono> saferemoveList = new List<PathMono>();
        foreach(PathMono safePoint in safePoints)
        {
            if (!connectedPoints.Contains(safePoint))
                saferemoveList.Add(safePoint);
        }
        foreach(PathMono pm in saferemoveList)
        {
            safePoints.Remove(pm);
        }
    
        //TODO PERFORMANCE if the connections start lagging, gate the checkLos function
        //TC
        //if (oldPosition == transform.position) return;
        //oldPosition = transform.position;

        CheckLOS();       
    }

    [Button]
    public void RemoveInvalidConnections()
    {
        CheckLOS();
        List<PathMono> disconnectionList = new List<PathMono>();
       foreach(PathMono pm in connectedPoints)
        {
            if (!safePoints.Contains(pm))
                disconnectionList.Add(pm);
        }
       foreach(PathMono pm in disconnectionList)
        {
            if (pm == null) continue;
            pm.Disconnect(this);
            pm.SafeRemove(this);
        }
       /* disconnectionList.Clear();
        foreach (PathMono pm in safePoints)
        {
            if (!connectedPoints.Contains(pm))
                disconnectionList.Add(pm);
        }
        foreach (PathMono pm in disconnectionList)
        {
            if (pm == null) continue;
            pm.Disconnect(this);
            pm.SafeRemove(this);
        }*/
    }

    [Button]
    void RemoveAndConnectNeighbours()
    {
        foreach (PathMono pm1 in connectedPoints)
        {
            foreach (PathMono pm2 in connectedPoints)
            {
                pm1.Connect(pm2);
            }
        }
        DestroyImmediate(gameObject);
    }


    [Button]
    void MergeSelected()
    {
        PathMono firstMono = null;
        Vector3 median = Calc.Median(Selection.gameObjects);
        //List<GameObject> destroyList = new List<GameObject>();
        foreach(GameObject go in Selection.gameObjects)
        {
            PathMono currentPoint = go.GetComponent<PathMono>();
            if (!currentPoint) continue;

            if(firstMono!=null)
            {
                firstMono.ConnectRange(currentPoint.connectedPoints);
                DestroyImmediate(go);
                continue;               
            }
            firstMono = currentPoint;         
            go.transform.position = median;
        }
       // foreach(GameObject go in destroyList)
        //{
        //    De
      //  }

    }

    bool splithandleUp = false;
    bool SplitHandle(PathMono p)
    {
        splithandleUp = true;
        Handles.color = new Color(0, 1, 0, .1f);
        Vector3 location = transform.position + (p.transform.position - transform.position) * 0.5f + Vector3.up * 2;
        return Handles.Button(location, Quaternion.LookRotation(Vector3.up), 2, 2, Handles.SphereCap);
    }

    bool removeHandleUp = false;
    bool RemoveHandle(PathMono p)
    {       
        removeHandleUp = true;
        Handles.color = Color.red;
        Vector3 location = transform.position + ((p.transform.position - transform.position) * 0.5f) - Vector3.up * 2;
        return Handles.Button(location, Quaternion.LookRotation(-Vector3.up), 2,2, Handles.SphereCap);

    }

    #endif
}
