
namespace Diluvion.SaveLoad
{

    /// <summary>
    /// Saves when the attached hull component is destroyed.
    /// </summary>
    public class HullInstanceSaver : SavedInstance
    {
        Hull myHull;

        public override void Awake()
        {
            base.Awake();
            if (!gameObject.activeInHierarchy) return;
            AddHullToDeathDelegate();
        }

        void AddHullToDeathDelegate()
        {
            myHull = GetComponent<Hull>();
            if (myHull == null) return;
            myHull.myDeath += MyDeath;
        }

        void MyDeath(Hull hull, string bywhat)
        {
            SaveInstanceState();
        }
    }
}