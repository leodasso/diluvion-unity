using UnityEngine;


namespace Diluvion.SaveLoad
{

    public enum SavedState
    {
        DisableOnLoad,
        noAction
    }

    public class SavedInstance : MonoBehaviour
    {

        public SavedState savedState = SavedState.DisableOnLoad;
        [SerializeField] private int myID = 0;
        protected bool saved = false;

        public virtual void Awake()
        {
            LoadInstanceState();
        }

        //safe ID getter
        public int MyID()
        {
            return myID;
        }

        /// <summary>
        /// Called on start if this instance has been saved.
        /// </summary>
        protected virtual void LoadedBehaviour()
        {
            // Debug.Log("Loading " + gameObject.name + " to " + savedState.ToString());
            switch (savedState)
            {
                case SavedState.DisableOnLoad:
                {
                    DisableOnLoad();
                    break;
                }
                default:
                {
                    break;
                }
            }
            saved = true;
        }

        /// <summary>
        /// Disables the game object
        /// </summary>
        void DisableOnLoad()
        {
            gameObject.SetActive(false);
        }

        /// <summary>
        /// Loads the instance state from the save file
        /// </summary>
        public void LoadInstanceState()
        {
            //Debug.Log("Attempting to load: " + gameObject.name + " Instance settings");
            if (DSave.current == null) return;

            //Debug.Log("Checking current save file for saved instance ID " + MyID());
            if (!DSave.current.LoadInstance(MyID()))
            {
                //Debug.Log("No save data was found for instance ID " + MyID());
                return;
            }
            LoadedBehaviour();
        }

        /// <summary>
        /// Saves the instance into the current dsave.
        /// </summary>
        public virtual void SaveInstanceState()
        {
            if (DSave.current == null)
            {
                Debug.Log(gameObject.name + " couldn't find a dsave file to save to!", gameObject);
                return;
            }
            //Debug.Log(gameObject.name + " is being saved to " + savedState.ToString());
            saved = true;
            DSave.current.SaveInstance(MyID());
        }

    }
}