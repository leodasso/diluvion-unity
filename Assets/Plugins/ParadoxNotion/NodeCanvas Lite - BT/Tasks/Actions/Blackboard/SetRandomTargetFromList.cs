using NodeCanvas.Framework;
using ParadoxNotion;
using ParadoxNotion.Design;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
namespace NodeCanvas.Tasks.Actions{


	[Category("✫ Blackboard")]
	[Description("Pick a random location from this list")]
	public class SetRandomTargetFromList<T> : ActionTask where T : Component
    {    

        public BBParameter<List<T>> targetList;
        public BBParameter<int> minTimesUntilRepick;

        [SerializeField]
        List<T> currentList = new List<T>();
        [SerializeField]
        List<T> removedList = new List<T>();
        int count = 0;

        [BlackboardOnly]
        public BBParameter<T> selected = new BBParameter<T>();        

		protected override string info{
			get { return "Pick a random target. No repick for: " + minTimesUntilRepick.value; }
		}

        protected override string OnInit()
        {
            if(!targetList.isNull)
                currentList = new List<T>(targetList.value);
            return base.OnInit();
        }

        T PickRandom()
        {
            int pickIndex = Random.Range(0, currentList.Count);

            T pickedTransform = currentList[pickIndex];

            if (count > 0)
            {
                currentList.RemoveAt(pickIndex);
                removedList.Add(pickedTransform);
                count++;
            }

            if(count> minTimesUntilRepick.value)
            {
                T v3ToRemove = removedList.First();
                removedList.Remove(v3ToRemove);
                currentList.Add(v3ToRemove);
            }       

            return pickedTransform;
        }
        

		protected override void OnExecute()
        {         
            if((currentList==null||currentList.Count<1)&&!targetList.isNull)
                currentList = new List<T>(targetList.value);
            if (currentList!=null&& currentList.Count>0)
                selected.value = PickRandom();
            EndAction();
		}
	}
}