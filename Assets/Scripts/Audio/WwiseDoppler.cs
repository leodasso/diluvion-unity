using UnityEngine;
using System.Collections;
using SpiderWeb;
using Diluvion;

public class WwiseDoppler : MonoBehaviour
{
    public float dopplerMultiplier = 10;
    PseudoVelocity listenerVelocity;
    PseudoVelocity myVelocity;
    float returnDoppler;

    void Awake()
    {
        if (Camera.main != null)
        {
            listenerVelocity = Camera.main.GetComponent<PseudoVelocity>();
        }
        else Destroy(this);
    }

    //Gets the listenervelocity
    Vector3 ListenerVelocity()
    {
        if (listenerVelocity != null) return listenerVelocity.velocity;
        listenerVelocity = OrbitCam.Get().GetComponent<PseudoVelocity>(); 
        if(listenerVelocity == null) { Debug.Log("Couldnt find listenerVelocity on camera: breaking", gameObject); return Vector3.zero; }
        return ListenerVelocity();
    }
    //Gets my velocity;
    Vector3 MyVelocity()
    {
        if (myVelocity != null) return myVelocity.velocity;
        if (GetComponent<Rigidbody>()) //prioritize rigidbody velocity if we dont have a pseudvelocity
            return GetComponent<Rigidbody>().velocity;
        if (GetComponent<PseudoVelocity>())
            myVelocity = GetComponent<PseudoVelocity>();
        else      
            myVelocity = gameObject.AddComponent<PseudoVelocity>();
        return MyVelocity();
    }

   

    Vector3 ListenerDirectionToMe()
    {
        if ( Camera.main == null ) return Vector3.forward;
        return transform.position - Camera.main.transform.position;
    }

    void Update()
    {
        returnDoppler = SpiderSound.DopplerPitch(dopplerMultiplier, ListenerDirectionToMe(), ListenerVelocity(), MyVelocity());
        SpiderSound.TweakRTPC("DopplerParam", returnDoppler,gameObject);
    }
}