using NodeCanvas.Framework;
using ParadoxNotion.Design;


namespace Diluvion.AI{

	[Category("Diluvion")]
	[Description("Gets the values from the input DamageRange")]
	public class ParseDamageRange : ActionTask{

        public BBParameter<DamageRange> rangeToParse;
        public BBParameter<RangeType> type;
        public BBParameter<float> range;
        public BBParameter<float> dps;

		protected override string OnInit()
        {
			return null;
		}

		protected override string info{
			get {return range.name + "( " + range.value + " )" + " @ " + dps.name+ "( " + dps.value + " )" ;}
		}
		
		protected override void OnExecute()
        {

            if (rangeToParse.isNull) { EndAction(false); return; }
            type.value = rangeToParse.value.rangeType;
            range.value = rangeToParse.value.range;
            dps.value = rangeToParse.value.damage;
			EndAction(true);
		}

		protected override void OnUpdate(){
			
		}

		protected override void OnStop(){
			
		}

		protected override void OnPause(){
			
		}
	}
}