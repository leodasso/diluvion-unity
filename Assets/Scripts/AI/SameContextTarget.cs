using NodeCanvas.Framework;
using ParadoxNotion.Design;


namespace Diluvion.AI{

	[Category("Diluvion")]
	[Description("Checks to see if one CT is equal to another")]
	public class SameContextTarget : ConditionTask
    {

        public BBParameter<ContextTarget> newContext;
        public BBParameter<ContextTarget> oldContext;
        //   public BBParameter<bool> usePriority;

        protected override string info
        {
            get
            {
                if(!invert)
                    return "NewContext is the same target as oldContext";
                else
                    return "NewContext is not the same target as oldContext";
            }
        }
        protected override string OnInit(){
			return null;
		}

		protected override bool OnCheck()
        {
            if (newContext.isNull && oldContext.isNull) return false;          

            if (!newContext.isNull)            
                if (oldContext.isNull) return false;             
            
            if(!oldContext.isNull)            
                if (newContext.isNull) return false;

            if (newContext.value != oldContext.value) return false;
            if (!newContext.value.Changed()) return true;
            newContext.value.SetUpdated();
            return false;
        }
	}
}