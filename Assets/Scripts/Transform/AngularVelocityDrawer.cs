
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AngularInstance
{
    public string label = "AI";
    public Vector3 angularTorque;
    public float widthMultiplier = 2;
    public Color myColor = Color.white;
    public List<string> log = new List<string>();
    
    public AngularInstance(){}
        
          
    public void SetAngularTorque(Vector3 torque)
    {
        angularTorque = torque;
       
        log.Add(angularTorque + " at " + Time.time);
        if(log.Count>15)
            log.RemoveAt(0);
    }
}

public class AngularVelocityDrawer : MonoBehaviour
{
    [SerializeField]
    public List<AngularInstance> aiList = new List<AngularInstance>();
    public float longestBound = 5;
    private float width;
    private float noseDistance;

    private Quaternion startRot;
    
    void Start()
    {
        startRot = transform.rotation;
        if (GetComponent<Avoider>())
        {
            longestBound = GetComponent<Avoider>().WidestPart();
            return;
        }
        
        Bounds b = new Bounds(transform.position, Vector3.one);
        foreach(Collider c in GetComponentsInChildren<Collider>())        
            b.Encapsulate(c.bounds);

        width = b.extents.x;
        noseDistance = Mathf.Abs(b.max.z- transform.position.z);
        longestBound = Mathf.Max(width, noseDistance);

        transform.rotation = startRot;
    }

    public AngularInstance Add()
    {
        AngularInstance instance = new AngularInstance();
        aiList.Add(instance);
        return instance;
    }

    public void RemoveInstance(AngularInstance ai)
    {
        aiList.Remove(ai);
    }



  
    
    
}
