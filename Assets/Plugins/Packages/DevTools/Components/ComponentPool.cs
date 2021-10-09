// =====================================================================
// Copyright 2013-2017 Fluffy Underware
// All rights reserved
// 
// http://www.fluffyunderware.com
// =====================================================================

#if UNITY_5_3 || UNITY_5_2 || UNITY_5_1 || UNITY_5_0
#define PRE_UNITY_5_4
#endif

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
#if !PRE_UNITY_5_4
using UnityEngine.SceneManagement;
#endif
namespace FluffyUnderware.DevTools
{
    public class ComponentPool : MonoBehaviour, IPool
    {
        [SerializeField, HideInInspector]
        string m_Identifier;

        [Inline]
        [SerializeField]
        PoolSettings m_Settings;

        public PoolSettings Settings
        {
            get { return m_Settings; }
            set
            {
                if (m_Settings != value)
                    m_Settings = value;
                
                if (m_Settings != null)
                    m_Settings.OnValidate();
            }
        }

        PoolManager mManager;

        public PoolManager Manager
        {
            get
            {
                if (mManager == null)
                    mManager = GetComponent<PoolManager>();
                return mManager;
            }
        }

        public string Identifier
        {
            get { return m_Identifier; }
            set
            {
            }
        }

        public System.Type Type
        {
            get
            {
                return System.Type.GetType(Identifier);
            }
        }


        public int Count
        {
            get { return mObjects.Count; }
        }

        List<Component> mObjects = new List<Component>();

        double mLastTime;
        double mDeltaTime;

        public void Initialize(System.Type type, PoolSettings settings)
        {
            m_Identifier = type.AssemblyQualifiedName;
            m_Settings = settings;
            mLastTime = DTTime.TimeSinceStartup + UnityEngine.Random.Range(0, Settings.Speed);
            if (Settings.Prewarm)
                Reset();
        }

        void Start()
        {
            if (Settings.Prewarm)
                Reset();
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            Settings = m_Settings;
        }
#endif

        void OnEnable()
        {
#if !PRE_UNITY_5_4
            SceneManager.sceneLoaded += OnSceneLoaded;
#endif
        }

        void OnDisable()
        {
        }

        public void Update()
        {
            if (Application.isPlaying)
            {
                mDeltaTime += DTTime.TimeSinceStartup - mLastTime;
                mLastTime = DTTime.TimeSinceStartup;

                if (Settings.Speed > 0)
                {
                    int c = (int)(mDeltaTime / Settings.Speed);
                    mDeltaTime -= c;

                    if (Count > Settings.Threshold)
                    {
                        c = Mathf.Min(c, Count - Settings.Threshold);
                        while (c-- > 0)
                        {
                            if (Settings.Debug)
                                log("Threshold exceeded: Deleting item");
                            destroy(mObjects[0]);
                            mObjects.RemoveAt(0);
                        }
                    }
                    else if (Count < Settings.MinItems)
                    {
                        c = Mathf.Min(c, Settings.MinItems - Count);
                        while (c-- > 0)
                        {
                            if (Settings.Debug)
                                log("Below MinItems: Adding item");
                            mObjects.Add(create());
                        }
                    }
                }
                else
                    mDeltaTime = 0;
            }
        }

        public void Reset()
        {
            if (Application.isPlaying)
            {
                while (Count < Settings.MinItems)
                {
                    mObjects.Add(create());
                }
                while (Count > Settings.Threshold)
                {
                    destroy(mObjects[0]);
                    mObjects.RemoveAt(0);
                }
                if (Settings.Debug)
                    log("Prewarm/Reset");
            }
        }
#if !PRE_UNITY_5_4
        public void OnSceneLoaded(Scene scn, LoadSceneMode mode)
        {
            for (int i = mObjects.Count - 1; i >= 0; i--)
                if (mObjects[i] == null)
                    mObjects.RemoveAt(i);
        }
#endif

        public void Clear()
        {
            if (Settings.Debug)
                log("Clear");
            for (int i = 0; i < Count; i++)
                destroy(mObjects[i]);
            mObjects.Clear();
        }

        public void Push(Component item)
        {
            sendBeforePush(item);
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                DestroyImmediate(item.gameObject);
            }
            else
#endif
                if (item != null)
            {
                mObjects.Add(item);
                item.transform.parent = Manager.transform;
                item.gameObject.hideFlags = (Settings.Debug) ? HideFlags.DontSave : HideFlags.HideAndDontSave;
                if (Settings.AutoEnableDisable)
                    item.gameObject.SetActive(false);
            }
        }

        public Component Pop(Transform parent = null)
        {
            Component item = null;
            if (Count > 0)
            {
                item = mObjects[0];
                mObjects.RemoveAt(0);
            }
            else
            {
                if (Settings.AutoCreate || !Application.isPlaying)
                {
                    if (Settings.Debug)
                        log("Auto create item");
                    item = create();
                }
            }
            if (item)
            {
                item.gameObject.hideFlags = HideFlags.None;
                item.transform.parent = parent;
                if (Settings.AutoEnableDisable)
                    item.gameObject.SetActive(true);
                sendAfterPop(item);
                if (Settings.Debug)
                    log("Pop " + item);
            }

            return item;
        }

        public T Pop<T>(Transform parent) where T : Component
        {
            return Pop(parent) as T;
        }

        Component create()
        {
            var go = new GameObject();
            go.name = Identifier;
            go.transform.parent = Manager.transform;
            if (Settings.AutoEnableDisable)
                go.SetActive(false);
            var c = go.AddComponent(Type);
            return c;
        }

        void destroy(Component item)
        {
            if (item != null)
                GameObject.Destroy(item.gameObject);
        }

        void setParent(Component item, Transform parent)
        {
            if (item != null)
                item.transform.parent = parent;
        }

        void sendAfterPop(Component item)
        {
            item.gameObject.SendMessage("OnAfterPop", SendMessageOptions.DontRequireReceiver);
        }

        void sendBeforePush(Component item)
        {
            item.gameObject.SendMessage("OnBeforePush", SendMessageOptions.DontRequireReceiver);
        }

        void log(string msg)
        {
            Debug.Log(string.Format("[{0}] ({1} items) {2}", Identifier, Count, msg));
        }

    }
}
