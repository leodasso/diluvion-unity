using UnityEngine;
using System.Collections;
using Sirenix.OdinInspector;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// When the trigger collider on this object is contacted by a rigidbody, will add force to linkedRigidbody based on the 
/// point of contact.
/// </summary>
public class AddForceOnContact : MonoBehaviour {

    public Rigidbody linkedRigidbody;
    public Collider col;
    
    public float forceToAdd = 5;
    public float forceTime = 1;
    public float resistance;
    
    public bool springedMovement = true;
    
    [ShowIf("springedMovement")]
    public float springForce = 1;
    [ShowIf("springedMovement")]
    public Vector3 restingDirection;
    [ShowIf("springedMovement")]
    public bool useStartingDirection;
    public float clampedVelocity = 6;


    Vector3 _totalSpringForce;
    float restAngVel = .02f;
    float restDOT = .98f;

    [Button("Get Refs")]
    void GetRigidbody()
    {
        if (!col) col = GetComponent<Collider>();
        if (col) col.isTrigger = true;
        linkedRigidbody = GetComponent<Rigidbody>();
        SetDirty();
    }


    void SetDirty()
    {
        #if UNITY_EDITOR
        EditorUtility.SetDirty(gameObject);
        #endif
    }


    // Use this for initialization
	void Start ()
	{
        if ( linkedRigidbody == null ) linkedRigidbody = GetComponent<Rigidbody>();

        // Disable this component if there's no rigidbody to act on.
	    if (linkedRigidbody == null)
	    {
	        Destroy(this);
	        return;
	    }

        // Memorize the direction I was facing at start to use as the direction to move towards 
        if ( useStartingDirection ) restingDirection = transform.up;
	    
	    //linkedRigidbody.Sleep();
	    
	}

    void FixedUpdate()
    {
        if (!linkedRigidbody) return;

        if ( linkedRigidbody.IsSleeping() ) return;

        if (springedMovement) {

            // Tell Rigidbody when to sleep
            float curDOT = Vector3.Dot(restingDirection, transform.up);
            if (curDOT > restDOT && linkedRigidbody.angularVelocity.magnitude < restAngVel)
                return;

            // Add force to spring the object back to its default position
            if (!linkedRigidbody.IsSleeping()) {
                _totalSpringForce = restingDirection.normalized * springForce;
                linkedRigidbody.AddForceAtPosition(_totalSpringForce, transform.position + transform.up * 5);
            }
        }
    }
	

    void OnTriggerEnter(Collider other)
    {
        if ( linkedRigidbody == null ) return;
        
        Rigidbody otherRB =  other.GetComponent<Rigidbody>();
        if ( otherRB == null ) return;
        

        // Apply resistance to the thing hitting me
        if (resistance > 0)
            StartCoroutine(AddSoftForce(otherRB, .5f, otherRB.velocity * -resistance, otherRB.transform.position, true));

        linkedRigidbody.WakeUp();

        // Get the direction of the collision. 
        Vector3 collisionDir = (transform.position - other.transform.position).normalized;
        // Put into local space so the height factor can be removed. This is to avoid pushing stuff straight down or up
        collisionDir = transform.InverseTransformDirection(collisionDir);
        collisionDir = new Vector3(collisionDir.x, 0, collisionDir.z);
        // Return the direction back to world space
        collisionDir = transform.TransformDirection(collisionDir);

        float collisionVel = Mathf.Clamp(otherRB.velocity.magnitude, 0, clampedVelocity);

        Vector3 newForce = collisionVel * 10 * forceToAdd * collisionDir;

        StartCoroutine(AddSoftForce(linkedRigidbody, forceTime, newForce, otherRB.transform.position));
    }

    /// <summary>
    /// Adds force to rigidbody rb over a period of time t, at position pos.
    /// </summary>
    IEnumerator AddSoftForce(Rigidbody rb, float t, Vector3 force, Vector3 pos, bool onRBPos = false)
    {
        // Get the total number of fixed updates that will happen over time t.
        float fixedUpdates = t / Time.fixedDeltaTime;

        // Get the amount of force that should be applied for each update
        Vector3 forcePerUpdate = force / fixedUpdates;

        float elapsed = 0;
        Vector3 forcePosition = pos;

        // Add force for the duration
        while (elapsed < t)
        {
            if (rb) {
                if ( onRBPos ) forcePosition = rb.transform.position;
                rb.AddForceAtPosition(forcePerUpdate, forcePosition);
            }
            elapsed += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }
        yield break;
    }
}
