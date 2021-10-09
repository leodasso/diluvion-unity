using System.Collections;
using System.Collections.Generic;
using Diluvion.Roll;
using UnityEngine;
using Sirenix.OdinInspector;


public class ResourceZone : MonoBehaviour
{
	[Tooltip("The chance each placer has of spawning"), Range(0, 1)]
	public float placerChance = .6f;

	[Tooltip("Each explorable placer has this many resources to populate with.")]
	public PopResources resources;
}
