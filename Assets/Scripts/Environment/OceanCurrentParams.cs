using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu(fileName = "new ocean current params", menuName = "Diluvion/ocean current params")]
public class OceanCurrentParams : ScriptableObject
{

	public float force = 20;
	[Tooltip("when adding the force at a position relative to the rigidbody, the default is the rigidbody's center " +
	         "of gravity. If this value is greater than zero, it will add a noise to this position, making a kind of " +
	         "wobbly force add.")]
	public float noiseOnPositionAmt = 1;

	public float noiseIntensity = 10;
	[MinValue(.1f)]
	public float noiseFrequency = 2;
}
