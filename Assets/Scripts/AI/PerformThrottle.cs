using NodeCanvas.Framework;
using ParadoxNotion.Design;
using Diluvion.Ships;

namespace Diluvion.AI{

	[Category("Diluvion")]
    [Description("Sets the Throttle on the shipmover from a request")]
	public class PerformThrottle: PerformRequest<ThrottleRequest>
    {
        public BBParameter<Throttle> throttle = new BBParameter<Throttle>();


        protected override string info
        {
            get { return "Setting throttle to " + throttle.value.ToString(); }
        }

        protected override bool DoRequest(ThrottleRequest request)
        {
            if (agent.MyShipMover.NoRepeatThrottle(request.throttle))
                throttle.value = request.throttle;

            return true;
        }
	}
}