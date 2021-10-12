using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using Diluvion;
using System.Collections.Generic;
using Diluvion.Roll;
using Diluvion.Sonar;
using NodeCanvas.Tasks.Actions;
using Sirenix.OdinInspector;

public class Explorable : Spawnable //, ICullable
{
    [TabGroup("References"), ReadOnly]
    public DebugExplorable debugEx;

    /*
    public float cullDistance
    {
        get
        {
            return 200;
        }
    }

    
    public ObjectResponse Culled { get; set; }
    public ObjectResponse UnCulled { get; set; }
    public ObjectResponse Despawned { get; set; }
*/

  
    #if UNITY_EDITOR

    protected override SpawnableEntry CreateEntryAsset()
    {
        return ScriptableObjectUtility.CreateSpawnableAsset<ExplorableEntry>(gameObject);
    }

    protected override string EntryAssetPath()
    {
        return base.EntryAssetPath() + "/Explorables";
    }

    #region Debug
    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();

        offsetFromFloor = OffsetFromFloor();

        if (entry)
        {
            Gizmos.color = Color.red;
            Vector3 upVector = BoundingCenter() + transform.up * Dir() * height;
            Gizmos.DrawLine(BoundingCenter(), upVector);
            Gizmos.color = Color.blue;
            Handles.CircleHandleCap(0, Root(), Quaternion.LookRotation(transform.up * Dir()), width, EventType.Repaint);
        }
    }

  

    [Button()]
    public void StartDebug()
    {           
        if (GameManager.DebugMode())
            SpawnDebugObject();
    }

    public void SpawnDebugObject()
    {
        if (prefabRef == null) return;
        if (debugEx != null) return;
      
        debugEx =  (DebugExplorable)Instantiate(Resources.Load<DebugExplorable>("DebugExplorable"),transform.position,transform.rotation);
        debugEx.name = gameObject.name;
        debugEx.transform.SetParent(transform.parent);
        debugEx.InitDebugExplorable(this);
    }
    
    #endregion
    
#endif

    [SerializeField]
    private Signature explorableSignature;

    private Signature ExplorableSignature
    {
        get
        {
            if (explorableSignature != null) return explorableSignature;
            return explorableSignature = SonarGlobal.GetSignature("explorable");
        }
    }

    /// <summary>
    /// Fix ExplorableTag
    /// </summary>
    void CheckExplorableTag()
    {
        foreach (SonarStats ss in GetComponentsInChildren<SonarStats>())
            if (!ss.HasSignature(ExplorableSignature))
                ss.AddSignature(ExplorableSignature);
    }
    
    #region positioning

    int Dir()
    {
        if (ceiling) return -1;
        return 1;
    }


    protected override void GetBounds()
    {
        base.GetBounds();
        localRoot = new Vector3(localCenter.x, localCenter.y - b.extents.y * Dir(), localCenter.z);

    }

    public override Vector3 Root()
    {
        return BoundingCenter() + offsetFromFloor;
    }

    /// <summary>
    /// Gets the current center of the bounding box
    /// </summary>
    protected override Vector3 BoundingCenter()
    {
        return transform.position + localRoot;
    }

    public Vector3 OffsetFromFloor()
    {

        Vector3 upPosition = BoundingCenter() + transform.up * height * Dir();
        RaycastHit[] hits = Physics.RaycastAll(upPosition, -transform.up * Dir(), height, LayerMask.GetMask("Terrain"));
        
        /*
        if (Physics.Raycast(upPosition, -transform.up * Dir(), out hit, height, LayerMask.GetMask("Terrain")))
        {
            return hit.point - BoundingCenter();
        }
        */

        foreach (var hit in hits)
        {
            // Omit any hits that are part of this explorable
            if (hit.collider == null) continue;
            if (hit.collider.gameObject.GetComponentInParent<Explorable>()) continue;

            return hit.point - BoundingCenter();
        }
        
        return Vector3.zero;
    }
    
    #endregion


    /// <summary>
    /// Conditions for allowing this culler to cull while in range
    /// <para>True if we can't see this explorable.</para>
    /// </summary>
    /// <returns></returns>
    public bool CanCull()
    {
        //TODO Check for inventory
        return !SpiderWeb.Cam.CanIsee(Camera.main, transform.position);
       
    }

    public float DestroyTime()
    {
        //TODO Change time based on if its been docked with or not
        return 10;
    }

   #region CullingCallbacks

    void OnEnable()
    {
        CheckExplorableTag();
        if (!Application.isPlaying) return;
        
        //if(UnCulled!=null) UnCulled(gameObject);
    }

    void OnDisable()
    {
        if (!Application.isPlaying) return;
        
        //if(Culled!=null)Culled(gameObject);
    }

    #endregion

}

#region old code
/// <summary>
/// Check to see if any of my active children are fighting
/// </summary>
/* public bool ExplorableEngaged()
 {
     CombatZone cz = GameObject.FindObjectOfType<CombatZone>();
     if (cz == null) return false;
     if (!cz.gameObject.activeInHierarchy) return false;

     GetActiveChildren();// check to see if i have any active children
     if (activeChildren == null) return false;
     if (activeChildren.Count < 1) return false;

     foreach(GameObject go in activeChildren)// if my children are currently fighting a player, return true, otherwise continue
     {
         if (go == null) continue;
         if (!go.activeInHierarchy) continue;
         if (cz.belligerents.Contains(go)) return true;
     }
     return false;
 }*/


/*
void GetActiveChildren()
{
activeChildren.Clear();
foreach(ArkCreature ak in GetComponentsInChildren<ArkCreature>())
{
    if (ak == null) continue;
    if (!ak.gameObject.activeInHierarchy) continue;
    activeChildren.Add(ak.gameObject);
}
foreach(Bridge b in GetComponentsInChildren<Bridge>())
{
    if (b == null) continue;
    if (!b.gameObject.activeInHierarchy) continue;
    activeChildren.Add(b.gameObject);
}
}
*/
#endregion