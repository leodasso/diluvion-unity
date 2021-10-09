using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using SpiderWeb;

/// <summary>
/// Navigation manager has tools for getting the patrol paths of this scene
/// </summary>
public class NavigationManager : MonoBehaviour
{
    public static NavigationManager navManager;
    public bool hideLOSConnections = false;

    List<PathMono> navigationPoints = new List<PathMono>();

    public List<PathMono> NavigationPoints
    {
        get
        {
            if (navigationPoints.Contains(null))
                navigationPoints.Clear();
            if (navigationPoints.Count > 0)
                return navigationPoints;

            navigationPoints = GetNavPoints();
            return navigationPoints;
        }
        private set
        {
            navigationPoints = value;
        }
    }
    
    #if UNITY_EDITOR

    [Button]
    void CleanupInvalidConnections()
    {
        foreach(PathMono pm in NavigationPoints)
        {
            pm.RemoveInvalidConnections();
        }
    }
    #endif

    LayerMask terrainMask;
   // public List<PatrolPath> scenePatrolPaths;

	void Awake() {
        terrainMask = LayerMask.NameToLayer("Terrain");
		//GetAllPatrolPaths();
	}


    List<PathMono> GetNavPoints()
    {
        navigationPoints.Clear();
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("PathNode"))
        {
            PathMono pm = go.GetComponent<PathMono>();
            if (pm != null)
                navigationPoints.Add(pm);
        }
        return navigationPoints;
    }

    public static NavigationManager Get()
    {
        if (navManager != null) return navManager;
        navManager = FindObjectOfType<NavigationManager>();

        if(navManager==null)
        {
            GameObject navman = new GameObject("NavManager");
            navManager = navman.AddComponent<NavigationManager>();
        }
        return navManager;     
    }

    /// <summary>
    /// Finds the closest Node to the input positio that has line of sight to the looker
    /// </summary>   
    public PathMono ClosestLOSPathMonoToPosition(Vector3 position, Vector3 looker)
    {
       
        terrainMask = LayerMask.GetMask("Terrain");
        List<PathMono> sortedPath = NavigationPoints.OrderBy(pm => Vector3.Distance(pm.transform.position, position)).ToList();

        for (int i=0; i<sortedPath.Count; i++)
        {
            Vector3 pathPosition = sortedPath[i].transform.position;
            if (!Physics.Raycast(pathPosition, looker - pathPosition, Vector3.Distance(pathPosition, looker),
                terrainMask))
            {
                Debug.DrawRay(pathPosition, looker - pathPosition, Color.green, 5);
                Debug.DrawRay(pathPosition, Vector3.up*3, Color.green, 5);
                return sortedPath[i];
            }
            Debug.DrawRay(pathPosition, looker - pathPosition, Color.blue, 5);
        }
        Debug.LogError("Could not find any Pathmono in LOS");
        return null;

    }
    
    /// <summary>
    /// Finds the closest Node to the input positio that has line of sight to the looker
    /// </summary>   
    public List<PathMono> ClosestLOSPathMonos(Vector3 position, Vector3 looker, int amounts = 1)
    {
        List<PathMono> returnPath = new List<PathMono>();
        int foundAmount = 0;
        terrainMask = LayerMask.GetMask("Terrain");
        List<PathMono> sortedPath = NavigationPoints.OrderBy(pm => Vector3.Distance(pm.transform.position, position)).ToList();

        for (int i=0; i<sortedPath.Count; i++)
        {
            Vector3 pathPosition = sortedPath[i].transform.position;
            if (!Physics.Raycast(pathPosition, looker - pathPosition, Vector3.Distance(pathPosition, looker),terrainMask))
            {
                Debug.DrawRay(pathPosition, looker - pathPosition, Color.red, 5);
                returnPath.Add(sortedPath[i]);
                foundAmount++;
                if (foundAmount >= amounts)
                    break;
            }
            Debug.DrawRay(pathPosition, looker - pathPosition, Color.blue, 5);
        }
        
        if (returnPath.Count < 1)
        {
            Debug.LogError("Could not find any Pathmono in LOS");
            return null;
        }
        else
        {
            return returnPath;
        }
    }

}
#region OldNavManager

#endregion
