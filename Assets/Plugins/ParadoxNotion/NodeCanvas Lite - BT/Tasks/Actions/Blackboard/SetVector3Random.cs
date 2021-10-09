using NodeCanvas.Framework;
using ParadoxNotion;
using ParadoxNotion.Design;
using UnityEngine;


namespace NodeCanvas.Tasks.Actions{


    public enum RandomShape
    {
        Sphere,
        Circle
    }

	[Category("✫ Blackboard")]
	[Description("Set a Random Blackboard Vector3 variable around startPosition with radius")]
	public class SetVector3Random : ActionTask {

        public RandomShape rss = RandomShape.Circle;
        
		public BBParameter<Vector3> startPosition;
        public BBParameter<float> radius;

        [BlackboardOnly]
        public BBParameter<Vector3> randomPos;

		protected override string info{
			get {return "Random Vector3 inside a "+ rss.ToString() +" from " + startPosition + " with radius of" + radius; }
		}

        Vector3 RandomPos()
        {
            switch(rss)
            {               
            case RandomShape.Sphere:
                {
                    return Random.insideUnitSphere * radius.value + startPosition.value;                     
                }
            default://CIRCLE
                {
                    Vector2 randomPoint = Random.insideUnitCircle;
                    return new Vector3(randomPoint.x, 0, randomPoint.y) * radius.value + startPosition.value;
                }
            }
        }    

		protected override void OnExecute(){
            randomPos.value = RandomPos();
            EndAction();
		}
	}
}