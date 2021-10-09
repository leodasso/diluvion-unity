using Diluvion.Ships;
using NodeCanvas.Framework;
using ParadoxNotion.Design;


namespace Diluvion.AI{

	[Category("Diluvion")]
	[Description("Callback for when a weapon is fired.")]    
	[EventReceiver("ShotFired")]
	public class OnFired : ConditionTask<AIMono>
	{


		public BBParameter<WeaponModule> targetModule;
		protected override string OnInit(){
			return null;
		}

		protected override bool OnCheck(){
			return false;
		}
		
		public void ShotFired(WeaponSystem ws)
		{
			if (ws == null) return;
			if(MatchingWeapon(ws.module))
				YieldReturn(true);
		}

		bool MatchingWeapon(WeaponModule wm)
		{
			if (targetModule.isNull&&targetModule.isNone) return true;
			return targetModule.value == wm;
		}
	}
}