using NodeCanvas.Framework;
using ParadoxNotion.Design;
using Diluvion.Sonar;
using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace Diluvion.AI {

    [Category("Diluvion")]
    [Description("Gets a list of sonarStats searching by the list of input tags")]
    public class GetSonarWithTags : ActionTask<AIMono> {

        public bool ignoreTerrain = false;
        public bool getSignaturesFromAI = false;
        [HideIf("getSignaturesFromAI")]
        public BBParameter<List<Signature>> signaturesToLookFor;
        public BBParameter<float> range = 99999;//Defaults to max range

        public BBParameter<List<SonarStats>> foundSonars;


        protected override string OnInit() {
            return null;
        }

        public virtual List<SonarStats> FoundSonars()
        {
            if (getSignaturesFromAI)
                signaturesToLookFor.value = new List<Signature>(agent.InterestSignatures);
            if(!ignoreTerrain)
                return agent.MyListener.SigsWithTags(signaturesToLookFor.value, range.value);
            else
                return agent.MyListener.OmniSigsWithTags(signaturesToLookFor.value, range.value);
        }

        protected override void OnExecute()
        {
            foundSonars.value = FoundSonars();

            if (foundSonars.isNull || foundSonars.value.Count<1) EndAction(false); //returns false if there are no sonars found to check against       

            EndAction(true);
		}
	}
}