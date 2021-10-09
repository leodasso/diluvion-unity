using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;

/// <summary>
/// Casts the object this is attached to in a given direction until it collides with a surface of the given layer(s).
/// </summary>
public class CastToDirection : MonoBehaviour
{

    [Tooltip("Move the casted object along the casting line after it's been casted out.")]
    public float surfaceOffset;

    [ToggleLeft]
    public bool castOnStart = true;

    [ToggleLeft]
    public bool animateCasting;
    
    [ShowIf("animateCasting")]
    public float animationTime = 2;

    [ToggleLeft]
    public bool tryUntilSuccessful = true;

    [ToggleLeft]
    public bool stayAtCastPosition;
    public Vector3 direction;
    public Space space;
    public LayerMask castLayerMask;

    Vector3 _castedPosition;
    bool _casted;

    void OnDrawGizmosSelected()
    {
        Ray castRay = CastingRay();

        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, castRay.direction * 100);
    }


	// Use this for initialization
	void Start () {

        //Debug.Log("Cast start " + name, gameObject);
        if ( castOnStart ) StartCoroutine(TryCast());
	}

    void OnSpawned()
    {
        if ( !_casted && tryUntilSuccessful ) {
            //Debug.Log(name + " is trying to cast on enable.", gameObject);
            StartCoroutine(TryCast());
        }
    }


    IEnumerator TryCast()
    {
        if ( tryUntilSuccessful )
        {
            int tries = 0;
            while ( !_casted && tries < 99 )
            {
                Cast();
                tries++;
                yield return new WaitForSeconds(1);
            }
        }
    }

    void LateUpdate()
    {
        if (stayAtCastPosition && _casted)
            transform.position = _castedPosition;
    }



    Ray CastingRay()
    {
        Vector3 finalDirection = direction;
        if ( space == Space.Self ) finalDirection = transform.TransformDirection(direction);

        return new Ray(transform.position, finalDirection);
    }

    [Button]
    public void Cast()
    {
        Ray newRay = CastingRay();

        // Create list of all colliders hit by cast
        List<RaycastHit> allHitList = Physics.RaycastAll(newRay, 800, castLayerMask).ToList();

        allHitList = allHitList.OrderBy(hit => hit.distance).ToList();

        // Check for and remove self from the list
        RaycastHit selfhit = new RaycastHit();
        foreach (RaycastHit h in allHitList) {
            if ( h.collider.gameObject == gameObject ) {
                selfhit = h;
                break;
            }
        }
        if ( selfhit.collider != null ) allHitList.Remove(selfhit);

        if ( allHitList.Count < 1 ) {
            //Debug.Log("Cast failed because there were no colliders to hit.");
            return;
        }
        RaycastHit goodHit = allHitList.First();

        //Debug.Log("Casted to hit " + goodHit.collider.name + ".");

        //transform.rotation = Quaternion.LookRotation(chain.transform.forward, -goodHit.normal);
        _castedPosition = goodHit.point + goodHit.normal;

        if (!animateCasting)
            transform.position = _castedPosition;
        else
        {
            StartCoroutine(LerpPosition());
        }

        _casted = true;
        
        transform.Translate(transform.forward * surfaceOffset);
    }

    IEnumerator LerpPosition()
    {
        Debug.Log("Starting animated cast! to " + _castedPosition);

        Vector3 startPos = transform.position;
        float progress = 0;

        while (progress < 1)
        {
            transform.position = Vector3.Lerp(startPos, _castedPosition, progress);
            progress += Time.deltaTime / animationTime;
            yield return null;
        }

        transform.position = _castedPosition;
    }
}
