using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using Diluvion;
using SpiderWeb;
#pragma warning disable 618



public class PathMonoInspector : Editor
{
    public float linkThreshold = 5;
  
    PathMono pw;
    PathMono closestOtherWP;
    //bool mouseUp = false;

    public void OnEnable()
    {
        pw = (PathMono)target;
   
        SceneView.onSceneGUIDelegate = null;
        SceneView.onSceneGUIDelegate += SceneGUI;
    }
    

    public override void OnInspectorGUI()
    {
        linkThreshold = EditorGUILayout.FloatField("Link Threshold", linkThreshold);
   //     pw.allowedMoveDistance = EditorGUILayout.FloatField("allowedMoveDistance", pw.allowedMoveDistance);
        Handles.DrawWireDisc(pw.transform.position, Vector3.up, linkThreshold);
        DrawDefaultInspector();

    }



    void OnSceneGUI()
    {
        if (pw == null) return;
        //if(NewHandle())        
            //Selection.activeTransform = pw.Add().transform;

       

        //adds a splitting handle between each of the connected points to the selected object
        if (closestOtherWP != null)
            Handles.color = Color.red;
        else
            Handles.color = Color.green;

        Handles.DrawWireDisc(pw.transform.position, Vector3.up, linkThreshold);
        if (pw.Neighbours == null) return;
        if (pw.Neighbours.Count < 1) return;
	    foreach (PathMono p in pw.Neighbours)
	    {
	        if (p == null) continue;           
	        if (SplitHandle(p, 0.5f))
	        {
	            Selection.activeTransform = pw.SplitTowards(p, 0.5f).transform;  
	            break;
	        }
	        if (RemoveHandle(p, 0.5f))
	        {
	            pw.RemoveConnection(p);
	            break;
	        }
	    }
    }

    void SceneGUI(SceneView sceneView)
    {
        Event e = Event.current;
        if (e.isMouse)
        {  
            if (e.button == 0)
            {                      

                if (e.type == EventType.MouseDrag)
                {
                    closestOtherWP = CloseToAnother(linkThreshold);                                   
                }
                else if(e.type == EventType.MouseUp)
                {
                    AttemptMerge(closestOtherWP);
                    closestOtherWP = null;        
                }            
            }
        }      
    }



    void AttemptMerge(PathMono targetPW)
    {     
        if (targetPW != null)
            pw.Replace(targetPW);//TODO Show a hovering gizmo
       // else
            //Debug.Log("Nothing Nearby");     

    }


    PathMono CloseToAnother(float threshold)
    {
        PathMono returnWP = null;
        if (pw == null) return null;
        //if (pw.transform.parent == null) return null;
        List<Transform> siblingList = new List<Transform>(pw.transform.GetComponentsInChildren<Transform>());
        siblingList.Remove(pw.transform);
        //Debug.Log("SiblingList is: " + siblingList.Count);
        if (siblingList.Count < 1) return null;

        Transform returnTrans = Calc.FindNearestTransform(siblingList, pw.transform);

        if (!Calc.WithinDistance(threshold, returnTrans, pw.transform)) return null;
       // Debug.Log("Found a valid point within distance: " + returnTrans.name);
        returnWP = returnTrans.GetComponent<PathMono>();

        return returnWP;
    }


    void SelectionCircle()
    {
   
        

    }

    bool NewHandle()
    {
        Handles.color = Color.green;
        return Handles.Button(pw.transform.position + new Vector3(5, 0, 5), Quaternion.LookRotation(Vector3.up),1, 1, Handles.CircleCap);

    }

    bool SplitHandle(PathMono p, float distance)
    {
        Handles.color = Color.green;
        Vector3 location = pw.transform.position + (p.transform.position - pw.transform.position)* distance+ Vector3.up * 2;
        return Handles.Button(location, Quaternion.LookRotation(Vector3.up), 2,2, Handles.SphereCap);

    }


    bool RemoveHandle(PathMono p, float distance)
    {
        Handles.color = Color.red;
        Vector3 location = pw.transform.position + ((p.transform.position - pw.transform.position) * distance) - Vector3.up*2;
        return Handles.Button(location, Quaternion.LookRotation(Vector3.up), 2, 2, Handles.SphereCap);

    }




}
