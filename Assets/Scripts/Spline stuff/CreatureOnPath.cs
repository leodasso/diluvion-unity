using UnityEngine;
using System.Collections;
using FluffyUnderware.Curvy;
using HeavyDutyInspector;
using SpiderWeb;


//TODO Distance calculation optimizations, find a good measure and use that for EVERYTHING
public class CreatureOnPath : ObjectsOnPath
{
    [Space(order = 2)]
    [Space(order = 2)]
    [ComplexHeader("Creature Options", Style.Box, Alignment.Left, ColorEnum.White, ColorEnum.Magenta, order = 3)]
    public CurvySplineSegment headSplineSegment;
    public CurvySplineSegment neckSegment;
    public AnimationCurve yOffsetDistance = new AnimationCurve();
    public float neckXZMax = 3;
    public float neckYMax = 7;
    public float neckClampAngle = 30;
    public Transform chaseTarget;

    public float lookVectorMag;
    public float lookRotationSpeed = 15;
    public float moveSpeed;
    public float moveSmoothing = 5;
    public float maxMoveDistance = 35;
    public float closeMoveDistance;
    public float moveSpeedMultiplier = 1;
    
    
    Vector3 startAimVector;
   
   
    Vector3 startPosition;
    Vector3 neckStartPos;
    Vector3 baseTargetVector;
    Vector3 headAimVector;
    Vector3 targetHeadPos;
    Vector3 neckToheadAim;
    Vector3 headBaseDir;

    float startOffsetDistance;
    float headBaseDistance;
    bool extending;
    float previousLength = 15;
    float distanceToHead;
    float neckContractBonus = 1;
    float lerpDirection = -1;
    public void Start()
    {
        startOffsetDistance = splineOffsetDistance;
        startAimVector = headSplineSegment.HandleIn;
        startPosition = headSplineSegment.position;
        targetHeadPos = startPosition;
        previousLength = spline.Length;
    }

    //CanChase
    public bool IfCanChase()
    {
        if(chaseTarget == null) return false;
        if(headSplineSegment == null) return false;
        if(neckSegment == null) return false; 
        return true;
    }

 
    
    public float DeltaLength()
    {
        float tempLength = (spline.Length - previousLength)*Time.deltaTime;
        previousLength = spline.Length;
        return tempLength;
    }

    public void SetSpeedMultiplier(float multi)
    {
        moveSpeedMultiplier = multi;
    }

    public void SetTarget(Transform target)
    {
        chaseTarget = target;

    }

    public void Cleartarget()
    {
        chaseTarget = null;
    }

    public void Update()
    {
        ChaseBehaviour();
    }

    float totalNeckSpeed;
    float totalMoveSpeed;
    float targetNeckSpeed;
    float targetMoveSpeed;
    void ChaseBehaviour()
    {
        extending = true;
        if (DeltaLength() >= 0)
            extending = false;

        if (extending)
            neckContractBonus = 1;
        else
            neckContractBonus = 2;

        headBaseDir = headSplineSegment.position - transform.position;

      

        if (IfCanChase())
        {
          
            headAimVector = (chaseTarget.position - headSplineSegment.position);
            neckToheadAim = (headSplineSegment.position - neckSegment.position);

            headSplineSegment.AutoHandles = false;
            Vector3 clampedHeadDir = ClampedAim(neckToheadAim, headAimVector, neckClampAngle);
            Vector3 headDir = PointHandlesAtTarget(headSplineSegment, lookRotationSpeed, clampedHeadDir, lookVectorMag);

            lerpDirection = 1;

            PointHandles(headSplineSegment, headDir);
            targetHeadPos = ClampedTargetPos(chaseTarget.position, maxMoveDistance);
        }
        else
        {
            lerpDirection = 0;
            targetHeadPos = startPosition;
            headSplineSegment.AutoHandles = true;
            //PointHandles(headSplineSegment, PointHandlesAtTarget(headSplineSegment, lookRotationSpeed/2, startAimVector, lookVectorMag));
        }
        Vector3 neckposition = MoveNeckHandle();


        //Neck and head move speed smoother
        if (Calc.WithinDistance(0.3f, targetHeadPos, headSplineSegment.position))
            targetMoveSpeed = 0;
        else
            targetMoveSpeed = moveSpeed * moveSpeedMultiplier;

        if (Calc.WithinDistance(0.3f, neckposition, neckSegment.position))
            targetNeckSpeed = 0;
        else
            targetNeckSpeed = moveSpeed * moveSpeedMultiplier;

       
        totalMoveSpeed = Mathf.MoveTowards(totalMoveSpeed, targetMoveSpeed, Time.deltaTime * moveSmoothing);
        totalNeckSpeed = Mathf.MoveTowards(totalNeckSpeed, targetNeckSpeed, Time.deltaTime* moveSmoothing*2);

        LerpMoveAlongSpline(lerpDirection);
        headSplineSegment.position = MoveTo(headSplineSegment.position, targetHeadPos, totalMoveSpeed) + UpOffsetEvaluate(headBaseDir);
        neckSegment.position = MoveTo(neckSegment.position, neckposition, totalNeckSpeed);

    }
    
    //Evaluates the curve to create an up offset to make it seem like we are coming out of a tunnel
    public Vector3 UpOffsetEvaluate(Vector3 v)
    {
        float flatDistance = Mathf.Clamp(new Vector3(v.x, 0, v.z).magnitude,0, maxMoveDistance) ;
        return transform.up*yOffsetDistance.Evaluate(flatDistance / maxMoveDistance);

    }



    //ClampTargetPos, the position will not be further away from this transform than the distanceclamp
    public Vector3 ClampedTargetPos(Vector3 targetPos, float distanceClamp)
    {
        Vector3 returnPos;
        baseTargetVector = (targetPos - transform.position);
       // Debug.DrawRay(transform.position, baseTargetVector, Color.red, 0.01f);
        Vector3 biasedTargetPos = targetPos - baseTargetVector.normalized * closeMoveDistance;
       // Debug.DrawLine(transform.position, biasedTargetPos, Color.cyan, 0.01f);

        if (Calc.WithinDistance(distanceClamp, transform.position, biasedTargetPos))
            returnPos = biasedTargetPos;
        else
            returnPos = transform.position + (biasedTargetPos- transform.position).normalized * distanceClamp;

       // Debug.DrawLine(transform.position, returnPos, Color.yellow, 0.01f);
        return returnPos;
    }
    
    //Moves the neck handle as a percentage of the heads position
    public Vector3 MoveNeckHandle()
    {
        headBaseDistance = headBaseDir.magnitude; 
        float neckY = Mathf.Clamp(headBaseDistance / 6, 0.1f, neckYMax);
        float neckXZ = Mathf.Clamp(headBaseDistance /6, 0f, neckXZMax);
      
        Vector3 hbdN = headBaseDir.normalized;//normalized shorthand for the headbaseDir
        Vector3 neckMovePos = new Vector3(hbdN.x * neckXZ, (hbdN.y+0.5f) * neckY, hbdN.z * neckXZ);
        Debug.DrawRay(transform.position, neckMovePos, Color.yellow, 0.01f);
        return transform.position+neckMovePos;
    }

    //Shorthand for moveTowards
    public Vector3 MoveTo(Vector3 currentPos, Vector3 worldPos, float speed)
    {
        Vector3 targetPos;     

        targetPos = Vector3.MoveTowards(currentPos, worldPos, speed * Time.deltaTime);
        return targetPos;       
    }

    float time = 0;
    public void LerpMoveAlongSpline(float target)
    {
        time = Mathf.MoveTowards(time, target, Time.deltaTime);
        splineOffsetDistance = Mathf.Lerp(startOffsetDistance, 0, time);
    }


}
