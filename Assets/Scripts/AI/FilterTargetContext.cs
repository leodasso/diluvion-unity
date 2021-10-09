using System.Runtime.Remoting.Contexts;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using System.Collections.Generic;


namespace Diluvion.AI{

	[Category("Diluvion")]
	[Description("Gets context targets best matching the input context")]
	public class FilterTargetContext : ActionTask<AIMono>
	{
		public BBParameter<TargetContext> filter;
		public BBParameter<List<ContextTarget>> checkList;
		public BBParameter<List<ContextTarget>> returnList;
		
		
		
		protected override string OnInit(){
			return null;
		}

		protected override void OnExecute()
		{

			returnList.value = new List<ContextTarget>();
			foreach (ContextTarget ct in checkList.value)
			{
				if (filter.value == ct.MyTargetContext)
				{
					returnList.value.Add(ct);
				}
			}
			
			returnList.value.Sort();
			EndAction(true);
		}

	}
}