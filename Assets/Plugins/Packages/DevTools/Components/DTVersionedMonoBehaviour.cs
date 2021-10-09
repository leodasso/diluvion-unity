// =====================================================================
// Copyright 2013-2017 Fluffy Underware
// All rights reserved
// 
// http://www.fluffyunderware.com
// =====================================================================

using UnityEngine;
using System.Collections;

namespace FluffyUnderware.DevTools
{
    public class DTVersionedMonoBehaviour : MonoBehaviour
    {
        [SerializeField,HideInInspector]
        string m_Version;

        string mNewVersion;

        /// <summary>
        /// Gets the version of this component
        /// </summary>
        public string Version { get { return m_Version; } }

        /// <summary>
        /// (Editor only) Checks for a new version and calls UpgradeVersion() if neccessary
        /// </summary>
        protected void CheckForVersionUpgrade()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                var versionInfo = GetType().GetCustomAttributes(typeof(DTVersionAttribute), true);
                if (versionInfo.Length > 0)
                {
                    string newVersion = ((DTVersionAttribute)versionInfo[0]).Version;
                    if (!string.IsNullOrEmpty(newVersion) && string.Compare(Version,newVersion)==-1)//Version != newVersion)
                    {
                        if (UpgradeVersion(Version, newVersion))
                            m_Version = newVersion;
                    }

                }
            }
#endif
        }

        /// <summary>
        /// (Editor only) Performs a version upgrade
        /// </summary>
        /// <param name="oldVersion">the old version (serialized in the Component)</param>
        /// <param name="newVersion">the new version (read from the DTVersion attribute)</param>
        /// <returns>true to store the new version in the component, false to fail</returns>
        protected virtual bool UpgradeVersion(string oldVersion, string newVersion)
        {
            if (string.IsNullOrEmpty(oldVersion))
                Debug.LogFormat("[{0}] Upgrading '{1}' to version {2}! PLEASE SAVE THE SCENE!", GetType().Name, name, newVersion);
            else
                Debug.LogFormat("[{0}] Upgrading '{1}' from version {2} to {3}! PLEASE SAVE THE SCENE!",GetType().Name,name,oldVersion,newVersion);
            return true;
        }
    }
}
