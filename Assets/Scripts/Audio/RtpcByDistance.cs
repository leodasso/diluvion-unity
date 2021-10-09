using UnityEngine;
using System.Collections;
using SpiderWeb;


public class RtpcByDistance : MonoBehaviour
{
    public string rtpcName;

    public float maxRTPCValue = 100;
    public float minRTPCValue = 0;
    public bool trackDistance = false;
    public float currentRTPC = 0;
    float distanceNormalized;
    ShortestDistanceToMyPoints sdtmp;

  

    /// <summary>
    /// safe ShortestDistanceGetter;
    /// </summary>

    ShortestDistanceToMyPoints SDTMP()
    {
        if (sdtmp != null) return sdtmp;
        sdtmp = GetComponent<ShortestDistanceToMyPoints>();        
        return sdtmp;
    }
    
    /// <summary>
    /// Overload for LerpRTPC which assumes a SDTMP module is attached, and uses the output from that
    /// </summary>
    public float LerpRTPC()
    {
        if (SDTMP() == null) return 0;//no measuring point      
        float distanceNormalized = SDTMP().DistanceFromClosestPointNormalized();
        return LerpRTPC(distanceNormalized);
    }

    //Gets the Lerped RTPC value between min and max from the normalized input value 
    public float LerpRTPC(float normalizedValue)
    {
        float returnRtpc = 0;
        returnRtpc = Mathf.Lerp(maxRTPCValue, minRTPCValue, normalizedValue);
        return returnRtpc;
    }

    public void Update()
    {
        if (!trackDistance) return;        
        currentRTPC = LerpRTPC();
        SpiderSound.TweakRTPC(rtpcName, currentRTPC, null);
    }
   
}

/* public float LerpRTPC()
   {
       float returnRtpc = 0;
       Vector3 smallestVector = new Vector3(999,999,999);
       foreach(Transform t in distancePoints)
       {
           if (t == null) continue;
           Vector3 distanceVector = t.position - player.position;
           if (distanceVector.sqrMagnitude < smallestVector.sqrMagnitude)
               smallestVector = distanceVector;
       }
       Debug.DrawRay(player.transform.position, smallestVector, Color.green, 0.01f);

       distanceNormalized = Mathf.Lerp(0.0f, 1.0f, ( Mathf.Clamp(smallestVector.magnitude, minDistance, maxDistance) - minDistance)*1.0f / (maxDistance - minDistance)*1.0f);

       returnRtpc = Mathf.Lerp(maxRTPCValue, minRTPCValue, distanceNormalized);

       return returnRtpc;
   }
   */
