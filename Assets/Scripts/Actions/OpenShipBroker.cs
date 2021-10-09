using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Diluvion
{

	[CreateAssetMenu(fileName = "shipbroker action", menuName = "Diluvion/actions/open ship broker", order = 0)]
	public class OpenShipBroker : Action
	{

		public override bool DoAction(Object o)
		{
			MonoBehaviour mono = o as MonoBehaviour;
			if (mono == null)
			{
				Debug.Log("Can't open ship broker on " + o.name);
				return false;
			}
			ShipBroker broker = mono.GetComponent<ShipBroker>();
			if (broker == null)
			{
				Debug.Log("No shipbroker attached to " + o.name);
				return false;
			}

			broker.DoAction();
			return true;
		}

		protected override void Test()
		{
			Debug.Log(ToString());
			DoAction(testObject);
		}

		public override string ToString()
		{
			return "Open ship broker panel.";
		}
	}
}