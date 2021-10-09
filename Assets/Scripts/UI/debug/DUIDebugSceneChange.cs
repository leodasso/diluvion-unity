using UnityEngine;

namespace DUI
{
    public class DUIDebugSceneChange : DUIDebug
    {
        public void LoadSceneOfName(string name)
        {
            CheatManager.Get().LoadScene(name);
        }

        public void LoadFjords()
        {
            CheatManager.Get().LoadZone(Zones.ForgottenFjords);
        }

        public void LoadRoyals()
        {
            CheatManager.Get().LoadZone(Zones.RoyalRuins);
        }

        public void LoadDepths()
        {
            CheatManager.Get().LoadZone(Zones.DwellerDepths);
        }
    }
}