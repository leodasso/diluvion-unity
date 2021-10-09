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
    
    public class DTSingleton<T> : MonoBehaviour where T : MonoBehaviour, IDTSingleton
    {
        static T _instance;
        static object _lock;
        static bool applicationIsQuitting = false;
        bool isDuplicateInstance = false;

        public static bool HasInstance
        {
            get { return _instance != null; }
        }

        public static T Instance
        {
            get
            {
                if (!Application.isPlaying)
                    applicationIsQuitting = false;
                if (applicationIsQuitting)
                {
                    return null;
                }

                if (_lock == null)
                    _lock = new object();
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = (T)FindObjectOfType(typeof(T));
                        
                        if (FindObjectsOfType(typeof(T)).Length > 1)
                        {
                            Debug.LogError("[DTSingleton] Something went really wrong " +
                                " - there should never be more than 1 singleton!" +
                                " PLEASE INFORM THE AUTHOR!");
                            return _instance;
                        }

                        if (_instance == null)
                        {
                            GameObject singleton = new GameObject();
                            _instance = singleton.AddComponent<T>();
                        }
                       
                    }
                    return _instance;
                }
            }
        }

        public virtual void Awake()
        {
            // We might have double instances, e.g. by loading a scene
            // In this case, give the original object a chance to merge data, then destroy the new instance
            if (_instance != null && _instance.GetInstanceID() != GetInstanceID())
            {
                ((IDTSingleton)_instance).MergeDoubleLoaded((IDTSingleton)this);
                this.isDuplicateInstance = true;
                this.Invoke("DestroySelf",0);
            }
        }
        
        protected virtual void OnDestroy()
        {
            if (Application.isPlaying && !isDuplicateInstance)
            {
                applicationIsQuitting = true;
                _instance = null;
            }
        }

        protected virtual void MergeDoubleLoaded(IDTSingleton newInstance)
        {
        }

        void DestroySelf()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                DestroyImmediate(this.gameObject);
            else
#endif
                Destroy(this.gameObject);
        }
        
    }

    public interface IDTSingleton
    {
        void MergeDoubleLoaded(IDTSingleton newInstance);
    }
}
