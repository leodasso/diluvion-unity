using UnityEngine;
using System.Collections;
using RootMotion.FinalIK;
using HeavyDutyInspector;

public enum RotationLimitChangeDirection
{
    X,
    Y,
    Z
}

[RequireComponent(typeof(Collider))]
public class RotationLimitChange : MonoBehaviour
{
    public float height;
    public RotationLimitChangeDirection interpolateDir = RotationLimitChangeDirection.Y;  
    public float startAngle = 1;
    public float endAngle = 90;

    [Button("Start Test", "StartValues", true )]
    public bool testStart;

  
    Collider myCOl;
    Vector3 localStartPosition;
    Vector3 localEndPosition;
    Ray insideRay;

    // Use this for initialization
    void Awake()
    {
        StartValues();
    }
    public void StartValues()
    {
        myCOl = GetComponent<Collider>();     
        height = GetStartHeight(myCOl);
        localStartPosition = transform.position - GetDirection() * (height / 2);
        insideRay = new Ray(localStartPosition, GetDirection() * height);
        Debug.DrawRay(insideRay.origin, insideRay.direction, Color.red, 55);

    }

    Vector3 GetDirection()
    {
        switch (interpolateDir)
        {
            case RotationLimitChangeDirection.X:
                return transform.right;
            case RotationLimitChangeDirection.Y:
                return transform.up;
            case RotationLimitChangeDirection.Z:
                return transform.forward;
            default:
                return Vector3.zero;
        }
    }

    Quaternion savedRot;
    //Gets the bottom of the collider
    float GetStartHeight(Collider col)
    {
        float returnFloat = 0;
        savedRot = transform.rotation;
        transform.rotation = Quaternion.identity;
        switch (interpolateDir)
        {
            case RotationLimitChangeDirection.X:
                {
                    returnFloat = col.bounds.size.x;
                    break;
                }
            case RotationLimitChangeDirection.Y:
                {
                    returnFloat = col.bounds.size.y;
                    break;
                }
            case RotationLimitChangeDirection.Z:
                {
                    returnFloat = col.bounds.size.z;
                    break;
                }
            default:
                {
                    returnFloat = 1;
                    break;
                }
        }
        transform.rotation = savedRot;
        return returnFloat;
    }



    public void OnTriggerStay(Collider other)
    {
        if(other.GetComponent<RotationLimitAngle>())        
            ChangeRotationAngle(other.GetComponent<RotationLimitAngle>());

        
    }

    /// <summary>
    /// Changes the rotation angle depending on several Factors
    /// </summary>
    /// <param name="otherAngle"></param>
    public void ChangeRotationAngle(RotationLimitAngle otherAngle)
    {
        float interpValue = HeightProgress(otherAngle.transform);
        float totalValue = Mathf.Lerp(startAngle, endAngle, interpValue);
        otherAngle.limit = totalValue;
    }


    float HeightProgress(Transform trans)
    {
        Vector3 normalizedVector = Vector3.Project(trans.position - insideRay.origin, insideRay.direction.normalized);
        Debug.DrawRay(insideRay.origin, normalizedVector, Color.red, 0.01f);
        if (IfNegative(trans.position - insideRay.origin)) // if the target trans pivot point is inside the area, return true, else return 0
            return 0;
     
        float insideDistance = normalizedVector.magnitude;
        return Mathf.Clamp(insideDistance / height, 0.0f, 1.0f);
    }

    //check if the insideRay direction, is the same direction as the vector
    bool IfNegative(Vector3 targetVector)
    {
        if (Vector3.Dot(insideRay.direction.normalized, targetVector.normalized) < 0)
            return true;
        else
            return false;
    }


	// Update is called once per frame
	void Update () {
	
	}
}
