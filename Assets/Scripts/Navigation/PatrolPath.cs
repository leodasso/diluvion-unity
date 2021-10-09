using UnityEngine;

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SpiderWeb;
using Diluvion;
using HeavyDutyInspector;

//TODO YAGNI
//TODO Move All Pathfinding Bullshit to its own class to allow multiple access
// MOVE TO CURVY FOR PATH EDITING  OR FIGURE OUT MULTI-PATH FOR BETTER PATHFINDING

public class PatrolPath : MonoBehaviour
{

    
    [Button("Recreate Patrol Bounds","PatrolBounds", true)]
    public bool patrolBounds;
    [Comment("Although you _Can_ make the width of the patrol smaller than 25, it is highly unrecomended.")]
    [Range(10, 200)]
    public float patrolBaseRadius = 50;
    public Material areaDebugMaterial;

    [HideInInspector()]
    public WaypointNode start;
    [HideInInspector()]
    public WaypointNode finish;
	public List<PathMono> allWPS;
    [HideInInspector()]
    public bool foldout = false;

    //public MegaShape currentShape;
    //public MegaSpline currentSpline;
    float length;
    WaypointNode startPoint;
    WaypointNode targetPoint;
    Dictionary<Vector3, float> interpThicknessDict = new Dictionary<Vector3, float>();
    List<WaypointNode> closedList = new List<WaypointNode>();
    List<PathNode> openList = new List<PathNode>();
 
    bool noPossiblePath;
    bool pathFound;

    void Awake() {
         
        GetAllWaypoints();
        if (GetComponent<MeshRenderer>())
            Destroy(GetComponent<MeshRenderer>());
        if (GetComponent<MeshFilter>())
            Destroy(GetComponent<MeshFilter>());

         if (NavigationManager.Get() == null) return;
       // NavigationManager.Get().AddPath(this);
    }

    /*
    void OnDestroy()
    {
        if (!Application.isPlaying) return;
        if (NavigationManager.Get()==null) return;      
      //  NavigationManager.Get().RemovePath(this);
    }
    */



    //Create Patrol Bounds and surrounding mesh
    public void PatrolBounds()
    {
        GetAllWaypoints();

        /*
        currentShape = GetComponent<MegaShape>();
        if (currentShape == null)
            currentShape = gameObject.AddComponent<MegaShape>();

        currentShape.stepdist = 100;
        List<Vector3> splinePoints = new List<Vector3>();
        List<WaypointNode> pwList = new List<WaypointNode>();
        PathFind pFind = new PathFind();
		foreach(WaypointNode wp in pFind.PatrolToTarget(allWPS[0], allWPS[0]))
			pwList.Add(wp);

        //Create a patrolpath from the parent of ALLWPS[0] back to start

        foreach (WaypointNode p in pwList)
        {
            splinePoints.Add(p.transform.localPosition);          
        }

        currentSpline = currentShape.splines[0];

        if (currentSpline == null)
            currentShape.BuildSpline(splinePoints.ToArray(), true);
        else
            currentShape.BuildSpline(0, splinePoints.ToArray(), true);        

        //Set up the megashape to not have 1million handles
        currentShape.mat1 = areaDebugMaterial;
        currentShape.makeMesh = true;
        currentShape.meshType = MeshShapeType.Tube; 
        currentShape.BuildMesh();
        if (GetComponent<MeshRenderer>().sharedMaterial != areaDebugMaterial)
            GetComponent<MeshRenderer>().sharedMaterial = areaDebugMaterial;
        currentShape.tradius = patrolBaseRadius;
        currentShape.drawHandles = false;
        currentShape.drawKnots = false;
        currentShape.drawspline = false;
        currentShape.drawTwist = false;
        currentShape.showorigin = false;
        currentShape.stepdist = currentSpline.length / 8;
      
        AnimationCurve acu = new AnimationCurve();
       
     
        int count = 0;
        foreach (WaypointNode p in pwList)
        {
            float keyPos = (count*1.0f / (pwList.Count) * 1.0f);            
            Keyframe key = new Keyframe(keyPos, p.allowedMoveDistance);
            key.tangentMode = 21;
            acu.AddKey(key);
           
            p.SetTotalMoveDistance(patrolBaseRadius);
            count++;
        }
        acu.AddKey(1, acu.keys[0].value);      
        currentShape.scaleX = acu;
        Debug.Log("Built spline at: " + currentShape.splines.Count);
        */
    }
        
  

    //returns the distance to the closest point on the curve from targetspot
    //TODO Check performance, might be heavier than say, putting all waypointnodes in a list and doing a check individually
    public float CurveClosestDistance(Vector3 targetSpot)
    {
        float sqrDistance = 0;
        float interpPoint = 0;
        Vector3 closestPoint = ClosestCurvePoint(targetSpot, out interpPoint);
        sqrDistance = (targetSpot - closestPoint).sqrMagnitude;
        return sqrDistance;
    }

    public void GetAllWaypoints()
    {
        //init the list
		allWPS = new List<PathMono>();

        //convert array of Waypoints to list
		allWPS.AddRange(GetComponentsInChildren<PathMono>());

        //if there's no Waypoints, destroy the interior path component
        if (allWPS.Count < 1) {
            Debug.Log("No Waypoints, so removing interior path component.", gameObject);
            Destroy(this);
        }
    }

    //Gets the Point on the curve closest to targetPos, interpAlpha is the normalized position on the curve(between 0-1)
    public Vector3 ClosestCurvePoint(Vector3 targetPos)
    {
        return targetPos;
        /*
        float interpAlpha = 0f;
        int knot = 0;
        Vector3 tangent = Vector3.zero;
        Vector3 returnVector = currentShape.FindNearestPointWorld(targetPos, 5, ref knot, ref tangent, ref interpAlpha);
        return returnVector;
        */
    }


    //Gets the Point on the curve closest to targetPos, interpAlpha is the normalized position on the curve(between 0-1)
    public Vector3 ClosestCurvePoint(Vector3 targetPos, out float interpAlpha)
    {
        interpAlpha = 1;
        return targetPos;
        /*
        interpAlpha = 0f;
        int knot = 0;       
        Vector3 tangent = Vector3.zero;
        if (currentShape == null) return Vector3.zero;
        Vector3 returnVector = currentShape.FindNearestPointWorld(targetPos, 5, ref knot, ref tangent, ref interpAlpha);       
        return returnVector;
        */

    }

    public Vector3 ClosestCurvePoint(Vector3 targetPos, out Vector3 tan, out float interpAlpha)
    {
        tan = Vector3.one;
        interpAlpha = 1;
        return targetPos;
        /*
        interpAlpha = 0f;
        int knot = 0;
        tan = Vector3.zero;
        if (currentShape == null) return Vector3.zero;
        Vector3 returnVector = currentShape.FindNearestPointWorld(targetPos, 5, ref knot, ref tan, ref interpAlpha);
        return returnVector;
        */
    }

    public Vector3 GetClosestLegalPos(Vector3 targetPos)
    {
        return targetPos;
        /*
        float baseRadius = currentShape.tradius;
        float interpAlpha = 0; 
        Vector3 returnV3;
        Vector3 closestCurvePoint = ClosestCurvePoint(targetPos, out interpAlpha);
        float meshRadius = currentShape.scaleX.Evaluate(interpAlpha) * baseRadius / 2;//Gets the width from the closest point
       
        //Debug.Log("Radius from Mesh " + meshRadius + " Out of: " + interpAlpha + " and " + baseRadius);
        Vector3 closestMeshPoint = closestCurvePoint + (targetPos - closestCurvePoint).normalized * meshRadius;
        if ((targetPos - closestCurvePoint).sqrMagnitude > ((targetPos - closestCurvePoint).normalized * meshRadius).sqrMagnitude)
            returnV3 = closestMeshPoint;
        else
            returnV3 = targetPos;
        //Debug.DrawLine(closestCurvePoint, closestMeshPoint, Color.blue, 5);
        //Debug.DrawLine(closestCurvePoint, tangent, Color.green, 5);
        return returnV3;
        */
    }

 
    //TODO Revisit this for chase 
    public List<Vector3> ReturnPoints(List<Vector3> allpoints)
    {
        List<Vector3> returnList = new List<Vector3>();

        foreach(Vector3 v3 in allpoints)
        {

        }

        return returnList;
    }


   /* //Same side check for ATTACKING ONLY
    public Vector3 GetClosestLegalPos(Vector3 fromPos, Vector3 targetPos)
    {       
        float baseRadius = currentShape.tradius;
        float interpAlpha = 0;
        Vector3 tangent;
       
        Vector3 closestCurvePoint = ClosestCurvePoint(fromPos, out tangent, out interpAlpha);

        float meshRadius = currentShape.scaleX.Evaluate(interpAlpha) * baseRadius / 2;//Gets the width from the closest point
         
        Vector3 projection =  Vector3.Project(tangent.normalized * meshRadius, (targetPos - closestCurvePoint).normalized);
        Vector3 closestMeshPoint = closestCurvePoint + projection;
        Debug.DrawRay(closestCurvePoint, tangent.normalized, Color.red, 16);
        Debug.DrawLine(closestCurvePoint, closestMeshPoint, Color.blue, 16);

        return closestMeshPoint;
    }
    */
    public bool IsWithinRangeOfPatrol(Transform target, float distance)
    {     
        Vector3 targetsClosestPoint = GetClosestLegalPos(target.position);

        return Calc.WithinDistance(distance, target, targetsClosestPoint);

    }

    void Start()
    {
        //sortedWaypoints = PathToTarget(allWPS, start, finish);
    }



}
