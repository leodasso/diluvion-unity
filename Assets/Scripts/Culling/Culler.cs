using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Diluvion;



/*
public interface ICullable
{
    bool CanCull();
    float cullDistance
    {
        get;
    }

    float DestroyTime();

    ObjectResponse Culled { get; set; }
    ObjectResponse Despawned{ get; set; }
    ObjectResponse UnCulled{ get; set; }
}

public delegate void ObjectResponse(GameObject gameObject);
/// <summary>
/// A child object that culls its parent when not useful to the game
/// </summary>
public class Culler : MonoBehaviour
{
    public enum Cullstate
    {
        InRange,
        Culled,
        Despawn
    }

    /// <summary>
    /// Current State of the culler
    /// </summary>
    public Cullstate cullState = Cullstate.InRange;
   
    protected ObjectResponse despawn;
    protected ObjectResponse culled;
    protected ObjectResponse unCulled;

    public GameObject cullingObject;
    public float cullRange = 200;
    static List<Culler> allCullers = new List<Culler>();
    static int activeIndex;
    int myIndex;
    System.Func<bool> cullCondition;



    /// <summary>
    /// The amount of time this culler can stay culled before despawning permanently
    /// </summary>
    float killTime = 30;

    private void OnDrawGizmos()
    {
        Color sphereColor = new Color(0,1,0,0.1f);
        if (cullState == Cullstate.Culled)
            sphereColor = new Color(1, 0, 0, 0.1f);
        Gizmos.color = sphereColor;

        Gizmos.DrawSphere(transform.position, 5);
    }

    public Culler()
    {

    }

    public Culler (GameObject newCulledObject, ICullable c)
    {        
        InitCuller(newCulledObject).SetCullValues(c);
    }

    /// <summary>
    /// Set Cull values if they are available, otherwise itll use defaults
    /// </summary>
    public void SetCullValues(ICullable c)
    {
        if (c == null) return;
        cullCondition = c.CanCull;
        cullRange = c.cullDistance;
        killTime = c.DestroyTime();
    }

    /// <summary>
    /// Initializes the culler to fit the target
    /// </summary>
    /// <param name="target"></param>
    public virtual Culler InitCuller(GameObject target) // TODO make an Icullable?
    {
        GameObject cullInstance = new GameObject(target + "_culler");
       
        Culler c = cullInstance.AddComponent<Culler>();
        c .cullingObject = target;
        c .Activate(target);
        return c;
    }


    /// <summary>
    /// Shorthand for checking for player range
    /// </summary>
    bool IsPlayerInRange(float range)
    {
        return PlayerManager.IsPlayerInRange(transform, range);
    }


    private void Awake()
    {
        RegisterPlacer();
    }

    private void Update()
    {
        if (activeIndex != myIndex) return;
        CheckCullState();


    }

    private void LateUpdate()
    {
        if (activeIndex != myIndex) return;
        IterateIndex();
    }

    #region iterationChunk


    static void IterateIndex()
    {
        activeIndex--;
        if (activeIndex < 0)
            activeIndex = allCullers.Count - 1;
    }

    /// <summary>
    /// Resets the indexes of all the listeners. This is called after a listener is destroyed.
    /// </summary>
    void ResetForRemoval()
    {
        if (!Application.isPlaying) return;
        //Debug.Log("Removing" + this.name);
        for (int i = 0; i < allCullers.Count; i++)
        {
            if (allCullers[i].myIndex > myIndex)
                allCullers[i].myIndex--;
        }
        if (activeIndex >= allCullers.Count)
            activeIndex = 0;
    }

    /// <summary>
    /// Startup registration for the static list
    /// </summary>
    void RegisterPlacer()
    {
        myIndex = allCullers.Count;
        allCullers.Add(this);
    }

    #endregion

    void CheckCullState()
    {
        switch (cullState)
        {
            case Cullstate.InRange:
                {
                    InRange();
                    break;
                }
            case Cullstate.Culled:
                {
                    Culled();
                    break;
                }
            case Cullstate.Despawn:
                {
                    Despawn();
                    break;
                }
        }
    }

    /// <summary>
    /// If the cullcondition is null always return true
    /// </summary>
    /// <returns></returns>
    bool CullConditionMet()
    {
        if (cullCondition == null) return true;
        return cullCondition();
    }

    /// <summary>
    /// What we do while the player is in range
    /// </summary>
    protected virtual void InRange()
    {
        if (!CullConditionMet()) return;
        if (IsPlayerInRange(cullRange)) return;
        cullState = Cullstate.Culled;
        Deactivate();
    }


    float timeCulled = 0;
    /// <summary>
    /// What we do while the player is away before we despawn
    /// </summary>
    protected virtual void Culled()
    {
        if (!IsPlayerInRange(cullRange))
        {
            timeCulled += Time.deltaTime*allCullers.Count;

            if (timeCulled < killTime) return;

            timeCulled = 0;
            cullState = Cullstate.Despawn;
            return;
        }
        //If we get back in range, set it back to Inrange
        timeCulled = 0;
        cullState = Cullstate.InRange;
        Activate(cullingObject);
    }


    /// <summary>
    /// Destroys the culling object(And its culling target if its culled)
    /// </summary>
    protected virtual void Despawn()
    {
        Debug.Log("DESPAWNING FROM CULLER" ,gameObject);
       
        if (despawn != null)
            despawn(gameObject);

        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        ResetForRemoval();
        allCullers.Remove(this);
    }


    /// <summary>
    /// We set this Culler to a child of the targetObject and track player distance
    /// </summary>
    protected void Activate(GameObject target)
    {
        if (cullingObject == null) { Debug.LogError("No Gameobject in culler: " + gameObject.name, gameObject); return; }


        cullingObject.SetActive(true);

        if (unCulled != null)
            unCulled(gameObject);
        transform.position = cullingObject.transform.position;
        transform.SetParent(cullingObject.transform);       
    }


    /// <summary>
    /// We deactivate the target and make it a child of this culler
    /// </summary>
    protected void Deactivate()
    {
        if (cullingObject == null) { Debug.LogError("No Gameobject in culler: " + gameObject.name, gameObject); return; }
        
        if (culled != null)
            culled(gameObject);

        transform.parent = cullingObject.transform.parent;
        cullingObject.transform.SetParent(transform);
        cullingObject.SetActive(false);
    }
}
*/
