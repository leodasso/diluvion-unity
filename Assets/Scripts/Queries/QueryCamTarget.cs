using System.Collections;
using System.Collections.Generic;
using Diluvion;
using UnityEngine;

namespace Queries
{

	[CreateAssetMenu(menuName = "Diluvion/queries/camera target")]
	public class QueryCamTarget : Query
	{
		[Tooltip("Query will return true if the player ship is the target, and vice versa.")]
		public bool playerShipIsTarget = true;
		
		public override bool IsTrue(Object o)
		{
			if (PlayerManager.PlayerShip() == null) return false;
			bool shipIsTarget = PlayerManager.PlayerShip().transform == OrbitCam.CurrentTarget();

			return shipIsTarget == playerShipIsTarget;
		}

		public override string ToString()
		{
			return "Query is " + playerShipIsTarget + " if player ship is the camera's current target.";
		}

		protected override void Test()
		{
			Debug.Log( ToString() + " result: " + IsTrue(null));
		}
	}
}