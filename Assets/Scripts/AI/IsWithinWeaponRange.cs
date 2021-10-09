using Diluvion.Ships;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;


namespace Diluvion.AI{

	[Category("Diluvion")]
	[Description("Returns true if the target is within range of any of my weapons")]
	public class IsWithinWeaponRange : ConditionTask<AIMono>
	{

		public BBParameter<Vector3> target;
		public BBParameter<float> range;
		public BBParameter<float> distanceFromWeapon;


		private List<MountGroup> _mountGroups = new List<MountGroup>();

		private List<MountGroup> MyMountGroups
		{
			get
			{
				if (_mountGroups != null && _mountGroups.Count > 0) return _mountGroups;
				_mountGroups = new List<MountGroup>(agent.GetComponentsInChildren<MountGroup>());
				return _mountGroups;
			}
		}

		protected override string info{
			get
			{
				if(target.isNull||range.isNull)return " Returns true if target is within input distance.";
				return "True if " + target.name + " is within " + range.name + "(" + range.value + ")";
			}
		}
			
		/// <summary>
		/// Gets the closest mountgroup to the target position
		/// </summary>
		/// <param name="target"></param>
		/// <returns></returns>
		public MountGroup GetClosestMountGroup(Vector3 target)
		{
			if (MyMountGroups.Count == 1) return MyMountGroups.First();
			return MyMountGroups.OrderBy(mg => Vector3.Distance(mg.transform.position, target)).First();
		}
	
		
		protected override string OnInit(){
			return null;
		}

		protected override bool OnCheck()
		{
			Vector3 targetpos = target.value;
			distanceFromWeapon.value = Vector3.Distance(targetpos, GetClosestMountGroup(targetpos).transform.position);
			return  	distanceFromWeapon.value < range.value;
		}
	}
}