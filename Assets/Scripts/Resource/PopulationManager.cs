using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace Diluvion.Roll
{

    /// <summary>
    /// Holder for population resources, used for inputting and outputting used and unused resources
    /// </summary>
    [System.Serializable]
    public class PopResources
    {
        /// <summary>
        /// Represents the physical size we are working with, 
        /// </summary>
        // [SerializeField]
        // public int techCost; TODO maybe implement techcost as a performance value

       
        /// <summary>
        /// Represents the in-game gold value we are working with
        /// </summary>
        [SerializeField]
        public int value;

        /// <summary>
        /// Represents the danger level we are working with
        /// </summary>
        [SerializeField]
        public int danger;

        public int hazardDanger;

        public PopResources( int val = 0, int dan = 0, int hazDanger = 0)
        {
            value = val;
            danger = dan;
            hazardDanger = hazDanger;
        }
        
        //Copy Function
        public PopResources(PopResources res)
        {
            value = res.value;
            danger = res.danger;
            hazardDanger = res.hazardDanger;
        }

        public override string ToString()
        {
            return ", <b>V:</b><color=yellow>" + value + "</color>, <b>D:</b><color=red>" + danger + "</color><b>H:</b><color=blue>"+hazardDanger  + "</color>";
        }

        public static bool CanAfford(PopResources budget, PopResources cost)
        {
            if (budget.danger < cost.danger) return false;
            if (budget.value < cost.value) return false;
            if (budget.hazardDanger < cost.hazardDanger) return false;
            return true;
        }

        #region operators
        public static PopResources operator +(PopResources pop1, PopResources pop2)
        {
            pop1.danger += pop2.danger;
            pop1.value += pop2.value;
            pop1.hazardDanger += pop2.hazardDanger;
           // pop1.techCost += pop2.techCost;
            return pop1;
        }

        public static PopResources operator -(PopResources pop1, PopResources pop2)
        {
            pop1.danger -= pop2.danger;
            pop1.value -= pop2.value;
            pop1.hazardDanger -= pop2.hazardDanger;
           // pop1.techCost -= pop2.techCost;
            return pop1;
        }

        public static PopResources operator *(PopResources pop1, float multiplier)
        {
            pop1.danger *= Mathf.RoundToInt(multiplier);
            pop1.value *= Mathf.RoundToInt(multiplier);
            pop1.hazardDanger *= Mathf.RoundToInt(multiplier);
            //pop1.techCost *= Mathf.RoundToInt(multiplier);
            return pop1;
        }

        public static PopResources operator /(PopResources pop1, float multiplier)
        {
            pop1.danger /= Mathf.RoundToInt(multiplier);
            pop1.value /= Mathf.RoundToInt(multiplier);
            pop1.hazardDanger /= Mathf.RoundToInt(multiplier);
           // pop1.techCost /= Mathf.RoundToInt(multiplier);
            return pop1;
        }

        public void Clamp( PopResources min, PopResources max)
        {
            danger = Mathf.Clamp(danger, min.danger, max.danger);
            value = Mathf.Clamp(value, min.value, max.value);
            hazardDanger = Mathf.Clamp(hazardDanger, min.value, max.value);
            //techCost = Mathf.Clamp(techCost, min.techCost, max.techCost);
        }

        public void ClampMax (PopResources max)
        {
            danger = Mathf.Clamp(danger, 0, max.danger);
            value = Mathf.Clamp(value, 0, max.value);
            hazardDanger = Mathf.Clamp(hazardDanger, 0, max.hazardDanger);
            //techCost = Mathf.Clamp(techCost, 0, max.techCost);
        }

        #endregion
    }
}