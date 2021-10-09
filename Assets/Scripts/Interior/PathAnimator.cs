using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using FluffyUnderware.Curvy;
using Sirenix.OdinInspector;
using SpiderWeb;
using Diluvion;

#if UNITY_EDITOR
using UnityEditor;
#endif


/// <summary>
/// Used to animate the path between different rooms
/// </summary>
[RequireComponent(typeof(CurvySpline))]
public class PathAnimator : MonoBehaviour
{

    [Tooltip("The prefab that will be instantiate along the curve to create a path")]
    public GameObject prefab;

    [Range(.05f, 10)]
    public float padding = .5f;

    public float duration = 1;

    public float handleAngleSnap = 45;

    public CurvySplineSegment start;
    public CurvySplineSegment end;

    [ReadOnly]
    public CurvySpline spline;

    [Range(0, 1)]
    public float value = .5f;

    float sfxInterval = .2f;

    [ShowInInspector, ReadOnly]
    int totalPoints = 0;

    [ReadOnly]
    public List<GameObject> pathInstances = new List<GameObject>();

    /// <summary>
    /// Loads a ref to the path animator prefab
    /// </summary>
    static GameObject Prefab ()
    {
        return Resources.Load("path animator") as GameObject;
    }

    /// <summary>
    /// Creates a path between the two given positions.
    /// </summary>
    public static PathAnimator MakePath (Vector3 startPos, Vector3 endPos, Transform parent = null)
    {
        PathAnimator newInstance = Instantiate(Prefab(), parent).GetComponent<PathAnimator>();
        newInstance.transform.localEulerAngles = Vector3.zero;

        newInstance.SetPosition(startPos, endPos);
        return newInstance;
    }

    // Use this for initialization
    void Start ()
    {
        spline = GetComponent<CurvySpline>();
        StartCoroutine(AnimatePath(0, 1, duration));
        
        // destroy the path after 15 seconds
        Destroy(gameObject, 15);
    }

    void Update ()
    {
        if (!prefab) return;
        RefreshPath();
    }

    /// <summary>
    /// Set the position of the start & end points of the spline
    /// </summary>
    void SetPosition (Vector3 startPos, Vector3 endPos)
    {
        start.transform.position = startPos;
        end.transform.position = endPos;

        float dist = Vector3.Distance(startPos, endPos);

        // Set up for pointing the curve handles in the correct direction
        Vector3 startHandle = endPos - startPos;
        Vector3 endHandle = startPos - endPos;

        // Clamp the handle positions so they dont get too long
        startHandle = Vector3.ClampMagnitude(startHandle, dist / 3);
        endHandle = Vector3.ClampMagnitude(endHandle, dist / 3);

        startHandle = Calc.SnapTo(startHandle, handleAngleSnap);
        endHandle = Calc.SnapTo(endHandle, handleAngleSnap);

        start.HandleOut = startHandle;
        end.HandleIn = endHandle;
    }


    IEnumerator AnimatePath (float startValue, float endValue, float time)
    {
        float progress = 0;

        startValue = Mathf.Clamp01(startValue);
        endValue = Mathf.Clamp01(endValue);

        value = startValue;

        float sfxTimer = 0;

        while (progress < 1)
        {
            sfxTimer += Time.unscaledDeltaTime;
            if (sfxTimer >= sfxInterval)
            {
                sfxTimer = 0;
                SpiderSound.MakeSound("Play_Footstep_Dots", gameObject);
            }
            
            
            progress += Time.unscaledDeltaTime / time;
            float smoothProg = Calc.EaseBothLerp(0, 1, progress);
            value = Mathf.Lerp(startValue, endValue, smoothProg);

            yield return null;
        }

        value = endValue;

        OrbitCam.ClearFocus();
    }

    void RefreshPath ()
    {
        totalPoints = Mathf.RoundToInt((spline.Length / padding) * value);

        GameObject newDot = null;

        // trim any unnecessary path instances
        if (totalPoints < pathInstances.Count)
        {
            for (int i = totalPoints; i < pathInstances.Count; i++)
#if UNITY_EDITOR
                DestroyImmediate(pathInstances [i]);
#else
                Destroy(pathInstances [i]);
#endif

            // Clean up the list by removing null entries that we just destroyed
            pathInstances = pathInstances.Where(point => point != null).ToList();
        }

        // Add instances if the path is longer now
        else if (totalPoints > pathInstances.Count)
        {
            for (int i = pathInstances.Count; i < totalPoints; i++)
            {
                GameObject newPathInstance = Instantiate(prefab, transform);
                pathInstances.Add(newPathInstance);
                newPathInstance.transform.localEulerAngles = Vector3.zero;

                newDot = newPathInstance;
            }
        }

        // re-position the path instances
        for (int j = 0; j < pathInstances.Count; j++)
        {
            float splinePos = j * padding;

            Vector3 posOnSpine = spline.InterpolateByDistanceFast(splinePos);

            if (pathInstances[j] == null) continue;
            
            pathInstances [j].transform.localPosition = posOnSpine;
        }

        if (newDot != null) OrbitCam.FocusOn(newDot, false);

#if UNITY_EDITOR
        UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
#endif
    }
}
