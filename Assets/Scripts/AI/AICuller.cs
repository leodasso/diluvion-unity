using UnityEngine;
using System.Collections;
using SpiderWeb;
using Diluvion;
using Diluvion.AI;

public class AICuller : MonoBehaviour
{
    public float cullRange = 300;
    public float cullBuffer = 20;
    public Vector3[] pathIWasFollowing;
    public float movespeed = 5;
    public bool simulateMovement = false;
    public GameObject instanceToCull;
    Transform targetParent; 
    Transform playerShipTransform;
    int movementIndex = 0;
    bool hasMovement = false;
    bool culled = false;
    Captain targetCaptain;
    Transform playerShip;
    bool initializedCuller = false;

    void Start()
    {
        InitCuller(instanceToCull, 5);
    }

    /// <summary>
    /// Initializes and checks to see if we can do movement
    /// </summary>

    public void InitCuller(GameObject target, float speed = 0)
    {
        if (target == null) return;
        if (!target.activeInHierarchy) return;
        name = target.name + "_culler";
        targetParent = target.transform.parent;       
        transform.SetParent(target.transform);      
        if (speed > 0)        
            simulateMovement = true;
    
        targetCaptain = target.GetComponentInChildren<Captain>();
       // StartCoroutine(DelaySetup(2));
        initializedCuller = true;
    }

    IEnumerator DelaySetup(float time)
    {
        yield return new WaitForSeconds(time);       

        InitPath();     
        if (!Calc.WithinDistance(cullRange + cullBuffer / 2, transform, PlayerManager.PlayerTransform()))
            Cull();
    }
    

    void InitPath()
    {
        if (targetCaptain == null) return;
       /* targetCaptain.GetPatrolPath();
        pathIWasFollowing = targetCaptain.pathToFollow.ToArray();*/
        // Debug.Log("TargetCaptain: " + targetCaptain.name + " / " + pathIWasFollowing.Length);
        movementIndex = 1;
    }


    //Disables the target gameObject of this object and sets it as a child  of the culler
    void Cull()
    {
        if (instanceToCull == null) return;
        if (!instanceToCull.activeInHierarchy) return;
        if (simulateMovement)
        {
           // Debug.Log("trying to simulate the movement along the path");
            if (targetCaptain)
            {
                //Debug.Log("we have a captain, attempting to get the index");
                InitPath();
                hasMovement = HasMovement();
             //   Debug.Log("We have movement " + hasMovement + " , index " + movementIndex);
            }
        }
        culled = true;
        transform.SetParent(targetParent);
        transform.position = instanceToCull.transform.position;
        instanceToCull.transform.SetParent(transform);
        instanceToCull.SetActive(false);      
    }

    //enables the target gameObject of this object and sets the culler back as a child of it
    void UnCull()
    {
        if (instanceToCull == null) return;
        if (instanceToCull.activeInHierarchy) return;
        culled = false;
        instanceToCull.SetActive(true);
        instanceToCull.transform.SetParent(targetParent);
        transform.position = instanceToCull.transform.position;
        transform.SetParent(instanceToCull.transform);    
    }

    //moves the culler around in a cheap manner  if the AI was following a large path
    void SimulatedMovement()
    {
        if (!culled) return;
        if (!hasMovement) return;
       // Vector3 thisVector = ThisVector(movementIndex, pathIWasFollowing);
        Vector3 nextVector = NextVector(ref movementIndex, pathIWasFollowing);
        //Debug.DrawLine(thisVector, nextVector, Color.red, 0.01f);
        if (!Calc.WithinDistance(3, transform.position, nextVector))
        {
            transform.position = Vector3.MoveTowards(transform.position, nextVector, movespeed * Time.deltaTime);
            transform.LookAt(nextVector);
        }
        else
            movementIndex++;
    }

    //Safe Getter for the current vector
    Vector3 ThisVector(int moveIndex, Vector3[] points)
    {
        if (points == null) return Vector3.zero;
        if (points.Length <= moveIndex) return Vector3.zero;
        //if (points[moveIndex] == null) return Vector3.zero;

        return points[moveIndex];
    }

    //Safe getter for the next Vector
    Vector3 NextVector(ref int moveIndex, Vector3[] points)
    {
        if (points == null) return Vector3.zero;
        //if (points[moveIndex] == null) return Vector3.zero;

        if (moveIndex >= points.Length)
            moveIndex = 0;

        if (moveIndex < 0)
            moveIndex = points.Length - 1;

        return points[moveIndex];
    }

    //Bool check for everything movement related
    bool HasMovement()
    {
        if (pathIWasFollowing == null) return false;
        if (pathIWasFollowing.Length < 1) return false;
        //if the path i was following was too small, dont bother doing movement
        //if (LongestSqrDistance(pathIWasFollowing) < 200*200) return false;
        return true;
    }


    void PollPlayerPosition()
    {
        if (Calc.WithinDistance(cullRange-cullBuffer/2, transform, PlayerManager.PlayerTransform()))
        {
            if (culled)
                UnCull();
        }
        if (!Calc.WithinDistance(cullRange + cullBuffer / 2, transform, PlayerManager.PlayerTransform()))
        {      
            if (!culled)
                Cull();    
        }
    }   


    //SimulatedMovement happnens in update to make sure its as fast as possible
    void Update()
    {
        if (!initializedCuller) return;
        SimulatedMovement();
        PollPlayerPosition();
    }



}
