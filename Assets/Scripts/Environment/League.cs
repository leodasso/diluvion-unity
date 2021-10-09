using UnityEngine;
using System;
using System.Collections;

[Serializable]
public class League {

	public string leagueName = "untitled";
	public float upperHeight = 0;
	public float lowerHeight = -100;
	public Atmosphere localAtmosphere;

	public League()
	{
		leagueName = "untitled";
		upperHeight = 0;
		lowerHeight = -100;
		localAtmosphere = new Atmosphere();
	}

	public bool InThisLeague(float depth) {

		if (depth < upperHeight && depth > lowerHeight) return true;
		else return false;
	}

	public bool HigherThan(float depth) {

		if (lowerHeight > depth) return true;
		else return false;
	}

	public bool LowerThan(float depth) {

		if (upperHeight < depth) return true;
		else return false;
	}
}
