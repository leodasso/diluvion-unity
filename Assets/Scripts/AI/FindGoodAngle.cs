using System;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using Diluvion.Ships;
using UnityEngine;

namespace Diluvion.AI{

	[Category("Diluvion")]
	[Description("Finds the best angle to approach a target based on my strong vector and theirs")]
	public class FindGoodAngle : ActionTask<AIMono>
	{

		public BBParameter<Transform> targetTrans;
		public BBParameter<GunArrangement> myGunArrangement;

		public BBParameter<GunArrangement> targetMovement;
		
		public BBParameter<float> verticalOffset;
		public BBParameter<float> horizontalOffset;
		public BBParameter<bool> getBehind;
		
		
		protected override void OnExecute()
		{
			ParseGunArrangements(myGunArrangement.value, targetTrans.value);
			EndAction(true);
		}

		/// <summary>
		/// Figures out based on enemy gun arrangement and my arrangement what kind of offset we should have for maximum combat efficiency
		/// </summary>
		/// <param name="myArrangement"></param>
		/// <param name="enemyTransform"></param>
		void ParseGunArrangements(GunArrangement myArrangement, Transform enemyTransform)
		{
			if (enemyTransform == null)
			{
				SetTargetOffsetPreference(myArrangement);
				return;
			}
			Bridge b = enemyTransform.GetComponent<Bridge>();
			if (b == null)
			{
				SetTargetOffsetPreference(myArrangement);
				return;
			}

			GunArrangement selectedArrangement = PickArrangement(myArrangement, b.strongVector);
			
			SetTargetOffsetPreference(selectedArrangement);
		}

		
		/// <summary>
		/// What angle is preferred, based on their and our preferred angles
		/// </summary>
		GunArrangement PickArrangement(GunArrangement myArrangement, GunArrangement theirArrangement)
		{
			//If their arrangement is...
			switch (theirArrangement)
			{
				case GunArrangement.Back:
				{
					if(myArrangement==GunArrangement.Vertical||myArrangement==GunArrangement.Horizontal)
						return GunArrangement.Forward;
					return myArrangement;
				}
				case GunArrangement.Forward:
				{
					if(myArrangement==GunArrangement.Vertical||myArrangement==GunArrangement.Horizontal)
						return GunArrangement.Back;
					return myArrangement;
				}
				case GunArrangement.Top:
				{
					if(myArrangement==GunArrangement.Vertical)
						return GunArrangement.Down;
					
					return myArrangement;
				}
				case GunArrangement.Down:
				{
					if(myArrangement==GunArrangement.Vertical)
						return GunArrangement.Top;
			
					return myArrangement;
				}
				default:
				{
					return myArrangement;
				}
			}
		}

		//convenience for setting values
		void SetValues(float x, float y, bool behind)
		{
			verticalOffset.value = y;
			horizontalOffset.value = x;
			getBehind.value = behind;
		}

		/// <summary>
		/// Sets the offset values for the direction we want to use
		/// </summary>
		/// <param name="targetArrangement"></param>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		void SetTargetOffsetPreference(GunArrangement targetArrangement)
		{
			targetMovement.value = targetArrangement;
			switch (targetArrangement)
			{
				case GunArrangement.Back:
				{
					SetValues(0, 0, true);
					break;
				}
				case GunArrangement.Forward:
				{
					SetValues(0, 0, false);
					break;
				}
				case GunArrangement.Top:
				{
					SetValues(0, 1, false);
					break;
				}
				case GunArrangement.Down:
				{
					SetValues(0, -1, false);
					break;
				}
				case GunArrangement.Horizontal:
				{
					SetValues(1, 0, false);
					break;
				}
				case GunArrangement.Vertical:
				{
					SetValues(0, UnityEngine.Random.Range(-1,1), false);
					break;
				}
				default:
				{
					throw new ArgumentOutOfRangeException("targetArrangement", targetArrangement, null);
				}
			}
		}
	}
}