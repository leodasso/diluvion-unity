using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Diluvion
{

	[CreateAssetMenu(fileName = "forge action", menuName = "Diluvion/actions/open forge")]
	public class OpenForge : Action
	{
		protected override void Test()
		{
			throw new System.NotImplementedException();
		}

		public override bool DoAction(Object o)
		{
			MonoBehaviour mono = o as MonoBehaviour;
			if (mono == null)
			{
				Debug.Log("Can't open forge on " + o.name);
				return false;
			}
			
			Forger forger = mono.GetComponent<Forger>();
			if (forger == null)
			{
				Debug.Log("No inventory attached to " + o.name);
				return false;
			}

			forger.CreateUI();
			return true;
		}

		public override string ToString()
		{
			return "Opens the forger panel.";
		}
	}
}