using System.Collections;
using System.Collections.Generic;
using Diluvion.Roll;
using DUI;
using UnityEngine;

namespace Diluvion
{
	[CreateAssetMenu(fileName = "ship summon", menuName = "Diluvion/actions/summon ship")]
	public class CallShip : HazardAction
	{
		public string warningString = "Warning, signature approaching on Sonar!";
		
		public ShipEncounter shipToSpawn;
		[Range(1, 5), Tooltip("How many turns does it take to summon?")]
		public int summonTime = 1;

		static int _summonTimer;

		public PopResources spawnResources;


		public override bool DoAction(Object o)
		{
			BattleLog summonLog;
			if (_summonTimer > 0)
			{
				_summonTimer--;
				summonLog = new BattleLog("......." + _summonTimer + "........");
			}
			else
			{
				_summonTimer = summonTime;
				// TODO LOC
				summonLog = new BattleLog(warningString);
				SummonShip();
			}
			
			summonLog.onEnd += BattlePanel.Iterate;
			BattlePanel.Log(summonLog);

			return true;
		}

		void SummonShip()
		{
			// get player ship's position
			Vector3 playerPos = PlayerManager.PlayerShip().transform.position;

			// Get a waypoint near player to spawn the enemy ship
			List<PathMono> waypoints = NavigationManager.Get().ClosestLOSPathMonos(playerPos, playerPos, 3);

			if (waypoints.Count < 1)
			{
				Debug.LogError("Can't find any waypoints to spawn a ship!");
				return;
			}
			else Debug.Log("Going to spawn at " + waypoints[1].name, waypoints[1].gameObject);

			Vector3 spawnPos = waypoints[1].transform.position;
			Quaternion spawnRot = Quaternion.LookRotation(playerPos - spawnPos, Vector3.up);
			
			shipToSpawn.Process(spawnResources);
			shipToSpawn.Create(spawnPos, spawnRot);
		}

		public override void DoAttack(Hazard hazard)
		{
			// Log this attack
			BattleLog newLog = new BattleLog(hazard.LocAttack(), hazard: hazard.LocName(), t: 5);
			BattlePanel.Log(newLog);
			//newLog.onEnd += BattlePanel.Get().SpawnCrewBoxes;
			DoAction(null);
			BattlePanel.Shake(2, 1);
		}

		public override void BattleBeginPrep()
		{
			base.BattleBeginPrep();
			_summonTimer = summonTime;
		}

		protected override void Test()
		{
			base.Test();
			SummonShip();
			//DoAction(null);
		}

		public override string ToString()
		{
			return "summons a ship from " + shipToSpawn.name;
		}
	}
}