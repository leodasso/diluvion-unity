using NodeCanvas.Framework;
using ParadoxNotion;
using ParadoxNotion.Design;
using UnityEngine;

namespace Diluvion.AI{


    public enum FlatPlane
    {
        XY,
        XZ,
        YZ
    }

	[Category("Diluvion/Movement")]
	[Description("Gets the distance along the plane")]
	public class FlatDistance : ConditionTask
    {
        public BBParameter<Vector3> targetLocation;
        public FlatPlane plane = FlatPlane.XZ;
        public CompareMethod checkType = CompareMethod.LessThan;

        public BBParameter<float> compareDistance;

        [SliderField(0, 0.1f)]
        public float floatingPoint = 0.05f;


        protected override string info
        {
            get { return "FlatDistance" + OperationTools.GetCompareString(checkType) + compareDistance + " to " + targetLocation; }
        }

        protected override string OnInit(){
			return null;
		}

		protected override bool OnCheck()
        {
            Vector3 direction = targetLocation.value - agent.transform.position;
            float flatDistance = 0;

            switch(plane)
            {
                case FlatPlane.XY:
                    {
                        flatDistance = new Vector3(direction.x, direction.y, 0).magnitude;
                        Debug.DrawRay(agent.transform.position, new Vector3(direction.x, direction.y, 0), Color.blue);
                        break;
                    }
                case FlatPlane.XZ:
                    {
                        flatDistance = new Vector3(direction.x, 0, direction.z).magnitude;
                        Debug.DrawRay(agent.transform.position, new Vector3(direction.x, 0, direction.z), Color.green);
                        break;
                    }
                case FlatPlane.YZ:
                    {
                        flatDistance = new Vector3( 0, direction.y, direction.z).magnitude;
                        Debug.DrawRay(agent.transform.position, new Vector3(0, direction.y, direction.z), Color.red);
                        break;
                    }
            }

            return OperationTools.Compare(flatDistance, compareDistance.value, checkType, floatingPoint);
           
		}
	}
}