using UnityEngine;
using System.Collections;
using SpiderWeb;
using Diluvion;
using Diluvion.Ships;

[RequireComponent(typeof(CreatureOnPath))]
[RequireComponent(typeof(CapsuleCollider))]
public class BobbitWormAI : MonoBehaviour
{

    public BobbitHead myHead;
    public float chaseSpeed = 1;//multiplies the speed at which we chase a target
    public float pullSpeed = 1; //multiplies the speed at which we pull a target
    public float biteDelay = 3; //the time we wait to bite after getting in range to bite (Should be synced with animation, or removed in favor of a callback)
    public float biteDamage = 3; //the snap damage of the initial bite
    public float biteForce = 100; //Snap rigibody applied force
    public float lairArea = 3;  // size of the struggle cave
    public bool grabber; //Two different worm behaviours, grabber and biter, grabbers grab, biters bite
    public Transform deathAnimationTarget;

    float currentHealth;
    float maxHealth;


    bool chasing = false;
    bool biting = false;
    CreatureOnPath controller;
    Transform chaseTarget;
    Ray biteRay;
    Hull myhull;

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position, lairArea);
    }

    void Start()
    {
        myhull = GetComponent<Hull>();
        currentHealth = myhull.currentHealth;
        maxHealth = myhull.maxHealth;
        myhull.imHit += IWasHit;
        controller = GetComponent<CreatureOnPath>();
        if (myHead)
            myHead.inRange += HeadInRange;
    }


    //The trigger is the area at which the worms starts to react
    public void OnTriggerEnter(Collider col)
    {
        if (col.GetComponent<Bridge>())
        {
            Debug.Log("Got the Bridge to chase: " + col.transform.name);
            StartCoroutine(Chase(col.transform));
        }
    }

    //The trigger is the area at which the worms stops following
    public void OnTriggerExit(Collider col)
    {
        if (col.transform == chaseTarget)
        {
            Debug.Log("Ending chase of " + col.name);
            chasing = false;
        }
    }


    /// Controls the overall movement behaviour   
    IEnumerator Chase(Transform cT)
    {      
        if (chasing) yield break;
        currentHealth = maxHealth;
        Debug.Log("Chasing " + cT.name);
        chasing = true;
        chaseTarget = cT;
        controller.SetTarget(chaseTarget);
        controller.SetSpeedMultiplier(chaseSpeed);
        while (chasing)
        {      
            yield return new WaitForEndOfFrame();
        }
        controller.SetSpeedMultiplier(pullSpeed);
        controller.Cleartarget();
    }


    //Callback for when i am hit
    public void IWasHit(float damage, GameObject go)
    {
        Debug.Log("Worm was hit for: " + damage + "/" + maxHealth);
        currentHealth -= damage;
        if (currentHealth <= 0)
            Retreat();

    }

    //Reset the worm
    void Retreat()
    {
        if (chasing == false && biting == false) return;
        Debug.Log("Worm got scared and is moving with his auntie and uncle in bel air");
        myHead.LetGo();
        chasing = false;       
        biting = false;
    }

    //Controls the bite behaviour, witha  delay before it actually bites(Corresponds to the bite animation)
    float timeElapsed;
    IEnumerator Bite(float startDelay, Transform bTarget)
    {
        if (biting) yield break;
        biting = true;
        Debug.Log("Starting to bite " + bTarget.name);
        timeElapsed = 0;
        while (biting)
        {
            if (timeElapsed > startDelay)
                break;        

            yield return new WaitForEndOfFrame();
            timeElapsed += Time.deltaTime;
        }
        // Debug.Log("Ended BiteWait, biting is: " + biting);     
        if (!biting)
            yield break;
        biteRay = new Ray(myHead.transform.position, (bTarget.position - myHead.transform.position));
     
        //DO ATTACK ANIMATION HERE
        if (grabber)
            StartCoroutine(Grab(bTarget));
        else
            Snap(bTarget, biteDamage, biteForce);
    }

    //Bite Physics application
    RaycastHit biteHit;
    void BiteForce(Rigidbody targetR, float force)
    {
        if (targetR == null) return;
        Debug.Log("Biding rigibody fug hard: " + biteForce);
        if (Physics.Raycast(biteRay, out biteHit, Calc.GunsafeLayer().value))
            targetR.AddForceAtPosition(-biteRay.direction.normalized * force, biteHit.point);
    }

    //Bite Damage application
    void BiteDamage(Hull targetHull, float damage)
    {
        if (targetHull == null) return;
        Debug.Log("Biding hull for " + biteDamage);
        targetHull.Damage(damage, 1, gameObject);
    }


    //Instant Bite and retreat
    void Snap(Transform biteTarget, float damage, float force)
    {
        Debug.Log("Snibbedy snab :D:D:DXDXD");
        BiteForce(biteTarget.GetComponent<Rigidbody>(), force);
        BiteDamage(biteTarget.GetComponent<Hull>(), damage);

        biting = false;
        chasing = false;
    }

    //Big worms grab the ship and try to take it with them to their death cave
    IEnumerator Grab(Transform grabTarget)
    {
        BiteDamage(grabTarget.GetComponent<Hull>(), biteDamage);
        myHead.Grab(grabTarget.GetComponent<Rigidbody>());
        chasing = false;
        while (Calc.OutsideDistance(lairArea, transform, grabTarget.position))
        {
            if (!biting) yield break;
            yield return new WaitForEndOfFrame();
        }
       
        //TODO maybe a coin flip on death? Like he slips or something
        StartCoroutine(DeathPull(grabTarget));
    }

    //Final pull, TODO Add a pull animation
    IEnumerator DeathPull(Transform target)
    {
        Debug.Log("DeathPull");
        yield return new WaitForSeconds(1);
        if (!biting)
            yield break;

        controller.SetTarget(deathAnimationTarget);
        controller.SetSpeedMultiplier(0.7f);

        yield return new WaitForSeconds(1.2f);
        if (!biting)
            yield break;

        controller.Cleartarget();
        controller.SetSpeedMultiplier(2f);

        yield return new WaitForSeconds(0.5f);
        if (!biting)
            yield break;    
       
        Snap(target, 3000, 3000);

    }

    //added to the head Object so that when the head collides with the chase target, it starts its behaviour
    public void HeadInRange(Transform target)
    {
        if (!chasing||biting) return;
        Debug.Log("headInRange called " + target);
        if (target != null)
            StartCoroutine(Bite(biteDelay, target));
        else
            chasing = false;
    }
}