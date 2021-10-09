using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class KillArkBeacons : MonoBehaviour
{


    void Start()
    {
        List<ArkBeacon> allABS =  new List<ArkBeacon>(FindObjectsOfType<ArkBeacon>());
        foreach (ArkBeacon ab in allABS)
            Destroy(ab.gameObject);

        List<ThornChaser> tsns = new List<ThornChaser>(FindObjectsOfType<ThornChaser>());
        foreach (ThornChaser ts in tsns)
            Destroy(ts.gameObject);

    }

}
