using UnityEngine;
using System.Collections;
using HeavyDutyInspector;
using System.Diagnostics;

public class DistTest : MonoBehaviour {

    [Button("Test", "Test", true)]
    public bool hidden1;
    public int iterations = 10;
    public GameObject a;
    public GameObject b;

	// Use this for initialization
	void Test ()
    {
        if (!a || !b) return;

        long distTotal = 0;
        long sqrMagTotal = 0;

        Stopwatch total = new Stopwatch();
        Stopwatch sw = new Stopwatch();

        total.Start();

        sw.Reset();
        sw.Start();
        for (int i = 0; i < iterations; i++)
        {
            float d = Vector3.Distance(a.transform.position, b.transform.position);
        }
        sw.Stop();
        distTotal += sw.ElapsedTicks;

        sw.Reset();
        sw.Start();
        for (int i = 0; i < iterations; i++)
        {
            float d = (a.transform.position - b.transform.position).sqrMagnitude;
        }
        sw.Stop();
        sqrMagTotal += sw.ElapsedTicks;

        total.Stop();

        float totalCalc = iterations * 2;

        UnityEngine.Debug.Log("distance total ticks: " + distTotal);
        UnityEngine.Debug.Log("sqr mag total ticks: " + sqrMagTotal);

        UnityEngine.Debug.Log("total elapsed time: " + total.ElapsedMilliseconds + "ms To calculate distance " + totalCalc + " times.");
    }

}
