using UnityEngine;
using System.Collections;

public class EnableTarget : MonoBehaviour {

    public GameObject target;



    public void Mouth(int i)
    {
        Debug.Log("Mouth " + i);
        if (i < 1)
            Enable(false);
        else
            Enable(true);
    }
 

    public void Enable(bool enable)
    {
        if (!target) return;

        target.SetActive(enable);
    }
}
