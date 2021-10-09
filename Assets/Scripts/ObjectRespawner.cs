using UnityEngine;
using System.Collections;

public class ObjectRespawner : MonoBehaviour
{
    public string resourceToInstantiate;
    public GameObject instanceToWatch;
    GameObject resourceReference;
    public float waitTime = 60;

    float counter;

    void Start()
    {
        resourceReference = Resources.Load(resourceToInstantiate)as GameObject;
        if (instanceToWatch == null)
            instanceToWatch = transform.parent.gameObject;        
       
        if (instanceToWatch == null) return;
        transform.SetParent(instanceToWatch.transform.parent);
        Init(instanceToWatch);
        transform.position = instanceToWatch.transform.position;
        transform.rotation = instanceToWatch.transform.rotation;       
    }

    public void Init(GameObject watchMe)
    {
        instanceToWatch = watchMe;
        if (instanceToWatch.GetComponent<IOnDead>() != null)
            instanceToWatch.GetComponent<IOnDead>().onDead += RespawnMe;
    }
	
    public void RespawnMe(float afterSeconds = 0)
    {
        StartCoroutine(KillInstance(afterSeconds));
    }

    IEnumerator KillInstance(float ss)
    {
        yield return new WaitForSeconds(ss);
        if(instanceToWatch!=null)
           Debug.Log("DESPAWNING FROM:" + this.name , this); Destroy(instanceToWatch.gameObject);
    }


    public void SpawnMe()
    {
        if (resourceReference == null) return;
        Init(Instantiate(resourceReference, transform.position, transform.rotation)as GameObject);
    }  
    
    void Update()
    {
        if (instanceToWatch != null) { counter = 0; return; }

        if (counter < waitTime)
            counter += Time.deltaTime;
        else
            SpawnMe();
    }

}
