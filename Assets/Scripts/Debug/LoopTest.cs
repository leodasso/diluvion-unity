using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Sirenix.OdinInspector;

public class LoopTest : MonoBehaviour {

    Dictionary<int, float> numbers = new Dictionary<int, float>();

    public float number1;

    public int iterations = 10000;

	// Use this for initialization
	void Start () {
		
	}
	
    [Button]
	public void Test()
    {

        numbers.Clear();
        for (int i = 0; i < iterations; i++)
        {
            numbers.Add(i, Random.Range(0f, 999999f));
        }

        Stopwatch stopwatch = new Stopwatch();

        stopwatch.Start();

        for (int i = 0; i < iterations; i++)
        {
            bool lesser = (number1 > numbers[i]);
        }

        stopwatch.Stop();

        UnityEngine.Debug.Log("Did " + iterations + " comparisons, which took " + stopwatch.ElapsedTicks + " ticks / " + stopwatch.ElapsedMilliseconds + " ms.");
    }
}
