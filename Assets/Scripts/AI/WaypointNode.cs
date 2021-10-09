using UnityEngine;
using System.Collections;


public class WaypointNode : PathMono {

    public float allowedMoveDistance = 1;
    [Range(0.1f,1)]
    public float approachSpeed = 1;
    //CharacterWaypoint[] allWaypoints;


    float totalAllowedMoveDistance = 0;
	public void SetTotalMoveDistance(float total)
	{
		totalAllowedMoveDistance = total * allowedMoveDistance;
	}

	public float AllowedMoveDistance()
	{
		return totalAllowedMoveDistance;
	}


}
