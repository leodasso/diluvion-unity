using NodeCanvas.Framework;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;

namespace Diluvion.AI{

	[ParadoxNotion.Design.Category("Diluvion")]
	[ParadoxNotion.Design.Description("Gets an item from a list based on a normalized value between min and max")]
	public class PickFromListBasedOnValue <T> : ActionTask<AIMono> where T : MonoBehaviour
	{
		public BBParameter<List<T>> fromList;
		
		[MinMaxSlider(-10, 10)]
		public BBParameter<float> pureValue;

		public BBParameter<T> returnPick;


		protected override void OnExecute()
		{
			int returnedIndex = Mathf.RoundToInt((pureValue.value + 10) / 20);
			if (returnedIndex >= fromList.value.Count)
			{
				if (fromList.isNull || fromList.value.Count < 1)
				{
					EndAction(false);
					return;
				}
				else
					returnPick.value = fromList.value.First();
			}
			else
			{
				returnPick.value = fromList.value[returnedIndex];
			}
			
			EndAction(true);
		}
	}
}