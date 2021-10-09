using UnityEngine;
using System.Collections;

namespace Diluvion
{

    /// <summary>
    /// This goes on a single game object in the scene, and can handle general world functions.
    /// </summary>
    public class WorldControl : MonoBehaviour
    {
        static WorldControl wc;

        /// <summary>
        /// Gets the world control object. If none exist, creates one.
        /// </summary>
        public static WorldControl Get ()
        {
            if (wc) return wc;
            
            GameObject wcObj = Resources.Load("World") as GameObject;
            wc = Instantiate(wcObj).GetComponent<WorldControl>();
            return wc;
        }

        public static bool Exists()
        {
            return wc != null;
        }

        /// <summary>
        /// Destroys the world instance
        /// </summary>
        public static void Clear()
        {
            if (!wc) return;
            Debug.Log("Destroying world control singleton!");
            DestroyImmediate(wc.gameObject);
            //Destroy(wc.gameObject);
            //wc = null;
        }
    }
}