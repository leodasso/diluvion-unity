using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TickLoadingBar : MonoBehaviour {

	public List<TickRotate> rotators;


	public void SetLoading(float loadingLerp) {

        //Debug.Log("Setting load progress: " + loadingLerp);

		float adjustedLerp = loadingLerp * rotators.Count;
		int rotatorsToStop = Mathf.RoundToInt(adjustedLerp);

        for (int i = 0; i < rotatorsToStop; i++)
        {
            if (i >= rotators.Count) continue;
            rotators[i].GotoKeyRotation();
        }
	}
}
