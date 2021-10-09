using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "game version")]
public class GameVersion : ScriptableObject
{

	public string versionNumber;

	static GameVersion _instance;

	static GameVersion Get()
	{
		if (_instance) return _instance;
		_instance = Resources.Load<GameVersion>("game version");
		return _instance;
	}
	
	public static string VersionNumber()
	{
		return Get().versionNumber;
	}
}
