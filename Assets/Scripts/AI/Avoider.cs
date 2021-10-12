using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Sirenix.OdinInspector;
#if UNITY_EDITOR
using UnityEditor;
#endif

public enum Avoiding
{
    Evading,
    InsideObject,
    Sliding,
    Nothing
}

public class Avoider : MonoBehaviour
{
    public float momentumMultiplier = 2f;
    public float closeMultiplier = 1.3f;
    public float farMultiplier= 0.35f;
    [SerializeField]
    float width = 3;
    public float widthMultiplier = 1.5f;
    public float avoidBreak = 8;

    [SerializeField]
    Avoiding avoidStatus;
    Vector3 aVector;
    RaycastHit[] hits;
    [SerializeField, ShowIf("debug")]
    List<GameObject> hitObjects = new List<GameObject>();
    
    
    Ray ray;
    LayerMask terrainMask;
    //LayerMask defaultMask;
    [SerializeField]
    bool debug = false;
    Vector3 castDir = Vector3.forward;
    float distanceToCheck = 15;
    public float noseDistance;
    private Collider[] myColliders;

    private void Awake()
    {
        Setup();
    }
    Quaternion startRot;


    public float WidestPart()
    {
        return Mathf.Max(noseDistance, width);
    }
    
    [SerializeField]
    private Bounds b;
    [Button]
    public void Setup()
    {
        SetMask(LayerMask.GetMask("Terrain", "Default"));
        
        //defaultMask = LayerMask.GetMask("Default");
        
        ray = new Ray(transform.position, transform.forward);
        
        startRot = transform.rotation;
        
        transform.rotation = Quaternion.identity;

        b = new Bounds(transform.position, Vector3.one);
        
        myColliders = GetComponentsInChildren<Collider>();

        foreach (Collider c in myColliders)
        {
            if (c.gameObject.layer == LayerMask.NameToLayer("Tools")) continue;
            if (c.isTrigger) continue;
            b.Encapsulate(c.bounds);

        }
            
        width = Mathf.Max(b.extents.x, b.extents.y) * widthMultiplier;
        
        noseDistance = Mathf.Abs(b.max.z- transform.position.z);
        
//        Debug.Log(gameObject.name  + " got width " + width+" / " + noseDistance, gameObject);
        
        transform.rotation = startRot;
    }

    public void SetMask(int mask)
    {
        terrainMask = mask;
    }

    public Avoiding Status()
    {
        return avoidStatus;
    }

    /// <summary>
    /// Returns the momentum of the ship, Velocity * Mass * Drag
    /// </summary>
    /// <returns></returns>
    public float ShipMomentum()
    {
        return CurrentSpeed* ShipMass();
    }

    public float ShipMass()
    {
        return GetComponent<Rigidbody>().mass;
    }

    private float CurrentSpeed { get; set; }


    /// <summary>
    /// Overload for avoidVector cast, assuming the current position as the start point for avoidance
    /// </summary>
    /// <param name="direction"></param>
    /// <returns></returns>
    float collisionDistance;
    public Avoiding NormalizedAvoidVector(Vector3 direction,float momentum, out Vector3 avoidVector, out float crashDistance)
    {
        collisionDistance = 0;
        CurrentSpeed = CurrentVelocity().magnitude;
        distanceToCheck = Mathf.Clamp(momentum, Mathf.Clamp(noseDistance,1,50), Mathf.Clamp(noseDistance*2,5,150));// Mathf.Clamp(checkDistance, noseDistance + 1,  3 * momentumMultiplier);

        avoidStatus = AvoidVector(transform.position, direction, out avoidVector, out collisionDistance);

        //TODO Normalized Distance needs to be how many lengths of momentum the ship can stop before coming to a halt.
        //TODO Its Momentum - Drag - Reverse Speed
        
        crashDistance = collisionDistance - noseDistance;

       // if (avoidStatus != Avoiding.Nothing)
         //   Debug.Log("Normalized Distance: " + collisionDistance + " " +  noseDistance + " " + (distanceToCheck) + " = " + crashDistance , gameObject); 
        return avoidStatus;
    }

    /// <summary>    
    /// Overload for avoidVector cast, assuming the current position as the start point for avoidance
    /// </summary>
    /// <param name="direction"></param>
    /// <param name="avoidVector"></param>
    /// <param name="distance">s</param>
    /// <returns></returns>
    Avoiding AvoidVector(Vector3 direction, out Vector3 avoidVector,  out float distance)
    {
        distanceToCheck = Mathf.Clamp(momentumMultiplier * CurrentSpeed, noseDistance + width + 5, ShipMomentum() * momentumMultiplier);
        return AvoidVector(transform.position, direction,out avoidVector, out distance); 
    }


    /// <summary>
    /// Base AvoidVector 
    /// </summary>
    Avoiding AvoidVector(Vector3 position, Vector3 direction, out Vector3 avoidVector , out float distance)
    {
        castDir = (direction.normalized+ CurrentVelocity().normalized)/2;           
        return avoidStatus = SphereCastHit(position, castDir, out avoidVector, out distance); ;
    }

    Vector3 CurrentVelocity()
    {
        return GetComponent<Rigidbody>().velocity;
    }

    private void OnDrawGizmosSelected()
    {
        if (!debug) return;
        #if UNITY_EDITOR
        Handles.color = new Color(0, 1, 0, 0.2f);
        Handles.SphereHandleCap(0, lastSuccessfulCastPoint, Quaternion.identity, width * 2, EventType.Repaint);
        Handles.SphereHandleCap(0, lastSuccessfulCastPoint + castDir.normalized * distanceToCheck, Quaternion.identity, width*2, EventType.Repaint);
        Handles.color = new Color(1, 0, 0, 0.2f);
        Handles.SphereHandleCap(0, lastSuccessfulCastPoint+ castDir.normalized * collisionDistance, Quaternion.identity, width*2, EventType.Repaint);  
        #endif
    }

    private Vector3 lastSuccessfulCastPoint;
    Avoiding SphereCastHit(Vector3 position, Vector3 direction, out Vector3 avoidVector, out float distance)
    {
        avoidVector = Vector3.zero;
     
        ray.origin = position;
        ray.direction = direction;
        hits = new RaycastHit[0];
        hits = Physics.SphereCastAll(ray, width, distanceToCheck, terrainMask);
        distance = distanceToCheck;
        RaycastHit hit;
        Vector3 bestAvoidanceDir = Vector3.zero;
        if(debug)
            hitObjects.Clear();
     
        for (int i = 0; i < hits.Length; i++)
        {
            hit = hits[i];
            if(myColliders.Contains(hit.collider))continue;
           
            Vector3 point = hit.point;


            // If point is 0, that means the hit is too close to this object
            if (point == Vector3.zero)
            {
                ray.origin = lastSuccessfulCastPoint;
                if (debug)
                    Debug.DrawRay(ray.origin, ray.direction*distanceToCheck, Color.cyan);
                
                  
                RaycastHit[] behindHits = Physics.SphereCastAll(ray, width, distanceToCheck, terrainMask);
                for(int j=0; j<behindHits.Length; j++)
                    if(behindHits[j].collider == hit.collider)
                    {
                        hit = behindHits[j];
                        point = hit.point;
                        break;
                    }
                
                if (debug)
                    hitObjects.Add(hit.collider.gameObject);
                
                if(point == Vector3.zero) return Avoiding.InsideObject;
            }
            else
                lastSuccessfulCastPoint = position;
            
            if(Vector3.Distance(lastSuccessfulCastPoint, position)> avoidBreak)
                return Avoiding.Sliding;

            Vector3 hitVector = point - lastSuccessfulCastPoint;
            float hitAngle = Vector3.Angle(hitVector.normalized, ray.direction.normalized) * Mathf.Deg2Rad;
            float cosine = Mathf.Cos(hitAngle);
            float hitDistance = Vector3.Distance(point, lastSuccessfulCastPoint);
            float flatRayDistance = cosine * hitDistance;
            Vector3 adjacentPoint = ray.GetPoint(flatRayDistance);
            Vector3  adjacentVector = adjacentPoint - point;
            Vector3 sphereCenter = ray.GetPoint(hit.distance);
            Vector3 sphereDirection = point - sphereCenter;

            if (hitDistance < distance)
                distance = hitDistance;
            float collisionFrontDOT = Vector3.Dot(ray.direction.normalized, sphereDirection);
            float distanceMultiplier = AvoidVectorMultiplier(hitDistance);
            float avoidMultiplier = distanceMultiplier * collisionFrontDOT;
           // Debug.Log("DistanceMultiplier: " + distanceMultiplier + " * " + collisionFrontDOT + " = " + avoidMultiplier);
      
            bestAvoidanceDir += adjacentVector * avoidMultiplier;

            if (debug)
            {
                Debug.DrawRay(ray.origin, (direction.normalized + adjacentVector * avoidMultiplier ), Color.red);
                Debug.DrawLine(ray.origin, adjacentPoint, Color.magenta);
                Debug.DrawLine(hit.point, adjacentPoint, Color.cyan);
                Debug.DrawLine(ray.origin, hit.point, Color.yellow);
//                Debug.Log("Hit a thing: " + hit.collider.gameObject.name, hit.collider.gameObject);
            }
        }
        //  Debug.Log(hits.Length + " length of hits.");
        if (bestAvoidanceDir == Vector3.zero){ lastSuccessfulCastPoint = position; return Avoiding.Nothing;}

        avoidVector = bestAvoidanceDir;

        if (debug)
        {
            Debug.DrawRay(ray.origin, direction.normalized * 5, Color.green);         
          
        }
        return Avoiding.Evading;
    }

    /// <summary>
    /// Returns a multiplier based on distance to the target, giving priority to closer hazards
    /// </summary>
    /// <param name="distance"></param>
    /// <returns></returns>
    float AvoidVectorMultiplier(float distance)
    {
        float normalizedDistance = Mathf.Clamp01(distance / distanceToCheck);
        return Mathf.Lerp(closeMultiplier, farMultiplier, normalizedDistance);
    }
}
 
