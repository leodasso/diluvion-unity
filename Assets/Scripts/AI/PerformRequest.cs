using NodeCanvas.Framework;
using ParadoxNotion.Design;


namespace Diluvion.AI{

	[Category("Diluvion")]
	public abstract class PerformRequest<T> : ActionTask<AIMono> where T: AIRequest{

        public BBParameter<T> request;	

		protected override void OnExecute()
        {          
            EndAction(DoRequest(request.value));
		}	

        protected abstract bool DoRequest(T request);
	}
}