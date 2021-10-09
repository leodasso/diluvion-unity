using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Diluvion.Roll
{

	/// <summary>
	/// Limits the resources that can be given to any explorable placer that's a child of this object.
	/// </summary>
	public class ResourceLimitZone : MonoBehaviour
	{
		//[ToggleLeft, HorizontalGroup("tech")]
		//public bool limitTechCost;
		
		//[MinValue(0), ShowIf("limitTechCost"), HorizontalGroup("tech")]
		//public float techCost;
		
		
		[ToggleLeft, HorizontalGroup("value")]
		public bool limitValue;
		
		[MinValue(0), ShowIf("limitValue"), HorizontalGroup("value")]
		public float value;
		
		
		[ToggleLeft, HorizontalGroup("danger")]
		public bool limitDanger;
		
		[MinValue(0), ShowIf("limitDanger"), HorizontalGroup("danger")]
		public float danger;
	}
}