using NodeCanvas.Framework;
using NodeCanvas.Framework.Internal;
using ParadoxNotion.Design;
using UnityEngine;

namespace NodeCanvas.Tasks.Conditions{

	[Category("✫ Blackboard")]
	public class CheckEnum : ConditionTask {

		[BlackboardOnly]
		public BBObjectParameter valueA = new BBObjectParameter(typeof(System.Enum));
		public BBObjectParameter valueB = new BBObjectParameter(typeof(System.Enum));

		protected override string info{
			get {return valueA + " == " + valueB;}
		}

		protected override bool OnCheck()
        {
            bool equals = Equals(valueA.value, valueB.value);
            Debug.Log(valueA.value.ToString() + " =/= " + valueB.value.ToString() + " (" + equals + ")" );
            return equals;

        }

		////////////////////////////////////////
		///////////GUI AND EDITOR STUFF/////////
		////////////////////////////////////////
		#if UNITY_EDITOR
		
		protected override void OnTaskInspectorGUI(){
			DrawDefaultInspector();
			if (GUI.changed && valueB.varType != valueA.refType){
				valueB.SetType( valueA.refType );
			}
		}

		#endif
	}
}