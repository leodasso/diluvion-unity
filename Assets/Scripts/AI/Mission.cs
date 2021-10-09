using UnityEngine;
using System.Collections;

/// <summary>
/// Class containing mission information;
/// </summary>
/// 
public class Job
{
	public string nicename;
	public Vector3 destination;


	public Job()
	{
		nicename = "NewMission";
		destination = Vector3.one;
	}


}
