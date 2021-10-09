using Diluvion.Ships;
using Diluvion;

namespace DUI
{
    public class DUIDebug : DUIPanel
    {

        CheatManager cheatManager;
        protected CheatManager CheatMan()
        {
            if (cheatManager != null) return cheatManager;
            cheatManager = CheatManager.Get(); ;
            return cheatManager;
        }

        public Bridge PlayerSub()
        {
            return PlayerManager.pBridge;
        }

        public virtual void Kill()
        {
            Destroy(gameObject);
        }
    }
}