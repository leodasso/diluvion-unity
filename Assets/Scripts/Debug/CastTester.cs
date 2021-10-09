using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Sirenix.OdinInspector;

public class CastTester : MonoBehaviour
{
    public int iterations = 1000;
    public int range = 100;
    public int width = 3;
    public int rays = 4;
   
    LayerMask mask;
    Stopwatch stopWatch;

    public void OnDrawGizmosSelected()
    {
        Vector3 offset = transform.right * width;
        for (int j = 0; j < rays; j++)
        {
            float rotation = j * 360 / rays;
            Vector3 rayOrigin = Quaternion.AngleAxis(rotation, transform.forward) * offset;
            Gizmos.DrawRay(transform.position+ rayOrigin, transform.forward * range);
        }
       
    }

    [Button]
    void SpherecastTest()
    {
        mask = LayerMask.GetMask("Terrain");
        RaycastHit hit;
        Ray ray = new Ray(transform.position, transform.forward);
        stopWatch = new Stopwatch();
        stopWatch.Start();
        for (int i=0; i<iterations; i++)
        {
            Physics.SphereCast(ray, width, out hit, range, mask);
        }
        stopWatch.Stop();
        UnityEngine.Debug.Log(iterations + " sphereCasts :" + stopWatch.ElapsedTicks);
    }

    [Button]
    void RaycastTest()
    {

        RaycastHit hit;
        mask = LayerMask.GetMask("Terrain");
        stopWatch = new Stopwatch();
        Vector3 offset = transform.right*width;
        //  float rotation = j * 360 / rays;
        //  Vector3 rayOrigin = Quaternion.AngleAxis(rotation, transform.forward) * offset;
        stopWatch.Start();
   
        for (int i = 0; i < iterations; i++)
        {
            for (int j = 0; j < rays; j++)
            {           
                Physics.Raycast(transform.position , transform.forward, out hit, range, mask);
            }
        }
        stopWatch.Stop();
        UnityEngine.Debug.Log(iterations +" * "+ rays + " rayCasts :" + stopWatch.ElapsedTicks);
    }
}
