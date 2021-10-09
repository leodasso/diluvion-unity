using System;
using NodeCanvas.Framework;
using ParadoxNotion;
using ParadoxNotion.Design;
using UnityEngine;

namespace Diluvion.AI
{


	[Category("Diluvion")]
	[Description("Gets the stat value off the current captain")]
	public class UseStat: ActionTask<AIMono>
    {

 
        public BBParameter<CaptainStat> thestat = CaptainStat.Bravery;
        public BBParameter<AnimationCurve> curve = new BBParameter<AnimationCurve>(); 
        public BBParameter<float> result = 0;
        public BBParameter<Captain> captain;
       

        public Captain Captain
        {
            get
            {
                if (!captain.isNull) return captain.value;
                
                captain.value = agent.MyCaptain;
                return captain.value;
            }
         
        }

        protected override string info
        {         
            get
            {
                string resultString = "";
               
                if (!captain.isNull)
                {
                    resultString += StatNumber() + " (" + thestat.ToString() + ") ";
                    result.value = CalculateResult();
                    resultString += " = " + result.value;
                }
                else
                {
                    result.value = CalculateResult();
                    if(!result.isNull)
                        resultString = "Parsing stat: " + "(" + thestat.ToString() + ") \n Outputs To: " + result.name + "( " + result.value+ " )";
                    else
                    {
                        resultString = "Parsing stat: " + "(" + thestat.ToString() + ")";
                    }
                }
                return resultString;
            }
        }

        float StatNumber()
        {
         
            if (agent!=null&&!thestat.isNull && Application.isPlaying&&Captain)
            {
                return Captain.GetStat(thestat.value);
            }
            return 0;
        }

        float CalculateResult()
        {
            if(curve.isNull || curve.value.keys==null || curve.value.keys.Length<2)            
                curve.value = new AnimationCurve(new Keyframe(-10,0), new Keyframe(10,30));
            
            return curve.value.Evaluate(StatNumber());
        }

        protected override string OnInit()
        {
            captain.value = null;
            return base.OnInit();
        }
        
      
        protected override void OnExecute()
        {
            result.value = CalculateResult();
            EndAction(true);
        }       
	}
}