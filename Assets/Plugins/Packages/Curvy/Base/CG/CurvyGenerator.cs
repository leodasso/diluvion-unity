// =====================================================================
// Copyright 2013-2017 Fluffy Underware
// All rights reserved
// 
// http://www.fluffyunderware.com
// =====================================================================

using UnityEngine;
using System.Collections.Generic;
using FluffyUnderware.DevTools;
#if UNITY_EDITOR
using UnityEditor;
#endif


namespace FluffyUnderware.Curvy.Generator
{
    /// <summary>
    /// Curvy Generator component
    /// </summary>
    [ExecuteInEditMode]
    [HelpURL(CurvySpline.DOCLINK + "generator")]
    [AddComponentMenu("Curvy/Generator",3)]
    [RequireComponent(typeof(PoolManager))]
    public class CurvyGenerator : DTVersionedMonoBehaviour
    {

        #region ### Serialized Fields ###

        [SerializeField, RangeEx(0,1,"Min Distance","Minimum distance between rasterized sample points")]
        float m_MinDistance=0.1f;

        [Tooltip("Show Debug Output?")]
        [SerializeField]
        bool m_ShowDebug;

        [SerializeField]
        bool m_AutoRefresh = true;
        [FieldCondition("m_AutoRefresh",true)]
        [Positive(Tooltip="Refresh delay (ms)")]
        [SerializeField]
        int m_RefreshDelay = 0;
        [FieldCondition("m_AutoRefresh", true)]
        [Positive(Tooltip="Refresh delay (ms)")]
        [SerializeField]
        int m_RefreshDelayEditor = 10;

        [Section("Events", false, false, 1000, HelpURL = CurvySpline.DOCLINK + "generator_events")]
        [SerializeField]
        CurvyCGEvent m_OnRefresh = new CurvyCGEvent();

        /// <summary>
        /// List of modules this Generator contains
        /// </summary>
        [HideInInspector]
        public List<CGModule> Modules = new List<CGModule>();

        [SerializeField, HideInInspector]
        internal int m_LastModuleID;

        #endregion

        #region ### Public Properties ###
        
        /// <summary>
        /// Gets or sets minimum distance between rasterized sample points
        /// </summary>
        public float MinDistance
        {
            get { return m_MinDistance; }
            set
            {
                if (m_MinDistance != value)
                   m_MinDistance = Mathf.Max(0.0001f,value);
                if (IsInitialized)
                    Refresh(true);
            }
        }

        /// <summary>
        /// Gets or sets whether to show debug outputs
        /// </summary>
        public bool ShowDebug
        {
            get { return m_ShowDebug; }
            set
            {
                if (m_ShowDebug != value)
                    m_ShowDebug = value;
            }
        }
        /// <summary>
        /// Gets or sets whether to automatically call <see cref="Refresh"/> if neccessary
        /// </summary>
        public bool AutoRefresh
        {
            get { return m_AutoRefresh; }
            set
            {
                if (m_AutoRefresh != value)
                    m_AutoRefresh = value;
            }
        }
        /// <summary>
        /// Gets or sets the minimum delay between two consecutive calls to <see cref="Refresh" while playing
        /// </summary>
        public int RefreshDelay
        {
            get { return m_RefreshDelay; }
            set
            {
                int v = Mathf.Max(0, value);
                if (m_RefreshDelay != v)
                    m_RefreshDelay = v;
            }
        }
        /// <summary>
        /// Gets or sets the minimum delay between two consecutive calls to <see cref="Refresh" in the editor
        /// </summary>
        public int RefreshDelayEditor
        {
            get { return m_RefreshDelayEditor; }
            set
            {
                int v = Mathf.Max(0, value);
                if (m_RefreshDelayEditor != v)
                    m_RefreshDelayEditor = v;
            }
        }

        /// <summary>
        /// Gets the PoolManager
        /// </summary>
        public PoolManager PoolManager
        {
            get
            {
                if (mPoolManager == null)
                    mPoolManager = GetComponent<PoolManager>();
                return mPoolManager;
            }
        }

        /// <summary>
        /// Event raised after refreshing the Generator
        /// </summary>
        public CurvyCGEvent OnRefresh
        {
            get { return m_OnRefresh; }
            set
            {
                if (m_OnRefresh != value)
                    m_OnRefresh = value;

            }
        }

        /// <summary>
        /// Gets whether the module and all its dependencies are fully initialized
        /// </summary>
        public bool IsInitialized { get { return mInitialized; } }
        /// <summary>
        /// Gets whether the Generator is about to get destroyed
        /// </summary>
        public bool Destroying { get; private set; }

        /// <summary>
        /// Dictionary to get a module by it's ID
        /// </summary>
        public Dictionary<int, CGModule> ModulesByID = new Dictionary<int, CGModule>();

        #endregion

        #region ### Private Fields ###

        bool mInitialized;
        bool mInitializedPhaseOne;
        bool mNeedSort = true;
        double mLastUpdateTime;
        PoolManager mPoolManager;
        
#if UNITY_EDITOR || CURVY_DEBUG
        // Debugging:
        public TimeMeasure DEBUG_ExecutionTime = new TimeMeasure(5);
#endif
#if UNITY_EDITOR
        // Refresh-Handling
        double mLastEditorUpdateTime;
        static List<CurvyGenerator> _updateCG = new List<CurvyGenerator>();

#endif

        #endregion

        #region ### Unity Callbacks ###
        /*! \cond UNITY */
        void Awake()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                UnityEditorInternal.ComponentUtility.MoveComponentUp(this);
            }
#endif
        }

        void OnEnable()
        {
            PoolManager.AutoCreatePools = true;
        }

        void OnDisable()
        {
            mInitialized = false;
            mInitializedPhaseOne = false;
            mNeedSort = true;
        }


        void OnDestroy()
        {
            Destroying = true;
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            MinDistance = m_MinDistance;
        }


        static void editorUpdate()
        {
            if (_updateCG.Count>0){
                var gen=_updateCG[0];
                if (DTTime.TimeSinceStartup-gen.mLastEditorUpdateTime > gen.m_RefreshDelayEditor * 0.001f )
                {
                    _updateCG.Remove(gen);
                    gen.Refresh();
                    EditorApplication.update -= editorUpdate;
                }
            }
            
        }
#endif

        void Update()
        {
            if (!IsInitialized)
                Initialize();
            else
            {
#if UNITY_EDITOR
                if (!Application.isPlaying)
                {
                    mLastEditorUpdateTime = DTTime.TimeSinceStartup;
                    if (!_updateCG.Contains(this))
                        _updateCG.Add(this);
                    EditorApplication.update += editorUpdate;
                }
#endif
                if (Application.isPlaying)
                {
                    if (AutoRefresh)
                    {
                        if (DTTime.TimeSinceStartup - mLastUpdateTime > RefreshDelay * 0.001f)
                        {
                            mLastUpdateTime = DTTime.TimeSinceStartup;
                            Refresh();
                        }
                    }
                }
            }
        }


        /*! \endcond */
        #endregion

        #region ### Public Static Methods ###
        
        /// <summary>
        /// Creates a new GameObject with a CurvyGenerator attached
        /// </summary>
        /// <returns>the Generator component</returns>
        public static CurvyGenerator Create()
        {
            var go = new GameObject("Curvy Generator", typeof(CurvyGenerator));
            return go.GetComponent<CurvyGenerator>();
        }

        #endregion

        #region ### Public Methods ###

        /// <summary>
        /// Adds a Module
        /// </summary>
        /// <typeparam name="T">type of the Module</typeparam>
        /// <returns>the new Module</returns>
        public T AddModule<T>() where T : CGModule
        {
            return (T)AddModule(typeof(T));
        }
        /// <summary>
        /// Adds a Module
        /// </summary>
        /// <param name="type">type of the Module</param>
        /// <returns>the new Module</returns>
        public CGModule AddModule(System.Type type)
        {
            var go = new GameObject("");
            go.transform.SetParent(transform,false);
            var mod = (CGModule)go.AddComponent(type);
            mod.SetUniqueIdINTERNAL();
            Modules.Add(mod);
            ModulesByID.Add(mod.UniqueID, mod);
            return mod;
        }

        /// <summary>
        /// Auto-Arrange modules' graph canvas position
        /// </summary>
        public void ArrangeModules()
        {
            Vector2 min = new Vector2(float.MaxValue, float.MaxValue);
            foreach (var mod in Modules)
            {
                min.x = Mathf.Min(mod.Properties.Dimensions.x, min.x);
                min.y = Mathf.Min(mod.Properties.Dimensions.y, min.y);
            }
            min -= new Vector2(10, 10);
            foreach (var mod in Modules)
            {
                mod.Properties.Dimensions.x -= min.x;
                mod.Properties.Dimensions.y -= min.y;
            }
        }

        /// <summary>
        /// Clear the whole generator
        /// </summary>
        public void Clear()
        {
            clearModules();
        }

         /// <summary>
        /// Deletes a module (same as PCGModule.Delete())
        /// </summary>
        /// <param name="module">a module</param>
        public void DeleteModule(CGModule module)
        {
            if (module)
                module.Delete();
        }

        /// <summary>
        /// Find modules of a given type
        /// </summary>
        /// <typeparam name="T">the module type</typeparam>
        /// <param name="includeOnRequestProcessing">whether to include IOnRequestProcessing modules</param>
        /// <returns>a list of zero or more modules</returns>
        public List<T> FindModules<T>(bool includeOnRequestProcessing = false) where T : CGModule
        {
            var res = new List<T>();
            for (int i = 0; i < Modules.Count; i++)
                if (Modules[i] is T && (includeOnRequestProcessing || !(Modules[i] is IOnRequestProcessing)))
                    res.Add((T)Modules[i]);
            return res;
        }

        /// <summary>
        /// Gets a list of modules, either including or excluding IOnRequestProcessing modules
        /// </summary>
        /// <param name="includeOnRequestProcessing">whether to include IOnRequestProcessing modules</param>
        public List<CGModule> GetModules(bool includeOnRequestProcessing = false)
        {
            
            if (includeOnRequestProcessing)
                return new List<CGModule>(Modules);
            else {
                var res = new List<CGModule>();
                for (int i = 0; i < Modules.Count; i++)
                    if (!(Modules[i] is IOnRequestProcessing))
                        res.Add(Modules[i]);
                return res;
            }
        }

        /// <summary>
        /// Gets a module by ID, either including or excluding IOnRequestProcessing modules
        /// </summary>
        /// <param name="moduleID">the ID of the module in question</param>
        /// <param name="includeOnRequestProcessing">whether to include IOnRequestProcessing modules</param>
        public CGModule GetModule(int moduleID, bool includeOnRequestProcessing = false)
        {
            CGModule res;
            if (ModulesByID.TryGetValue(moduleID, out res) && (includeOnRequestProcessing || !(res is IOnRequestProcessing)))
                return res;
            else
                return null;
        }

        /// <summary>
        /// Gets a module by ID, either including or excluding IOnRequestProcessing modules (Generic version)
        /// </summary>
        /// <typeparam name="T">type of the module</typeparam>
        /// <param name="moduleID">the ID of the module in question</param>
        /// <param name="includeOnRequestProcessing">whether to include IOnRequestProcessing modules</param>
        public T GetModule<T>(int moduleID, bool includeOnRequestProcessing = false) where T : CGModule
        {
            return GetModule(moduleID,includeOnRequestProcessing) as T;
        }

        /// <summary>
        /// Gets a module by name, either including or excluding IOnRequestProcessing modules 
        /// </summary>
        /// <param name="moduleName"></param>
        /// <param name="includeOnRequestProcessing"></param>
        public CGModule GetModule(string moduleName, bool includeOnRequestProcessing=false)
        {
            for (int i = 0; i < Modules.Count; i++)
                if (Modules[i].ModuleName.Equals(moduleName, System.StringComparison.CurrentCultureIgnoreCase) && (includeOnRequestProcessing || !(Modules[i] is IOnRequestProcessing)))
                    return Modules[i];

            return null;
        }

        /// <summary>
        /// Gets a module by name, either including or excluding IOnRequestProcessing modules (Generic version)
        /// </summary>
        /// <typeparam name="T">type of the module</typeparam>
        /// <param name="moduleName">the ID of the module in question</param>
        /// <param name="includeOnRequestProcessing">whether to include IOnRequestProcessing modules</param>
        public T GetModule<T>(string moduleName, bool includeOnRequestProcessing = false) where T : CGModule
        {
            return GetModule(moduleName,includeOnRequestProcessing) as T;
        }

        /// <summary>
        /// Gets a module's output slot by module ID and slotName
        /// </summary>
        /// <param name="moduleId">Id of the module</param>
        /// <param name="slotName">Name of the slot</param>
        public CGModuleOutputSlot GetModuleOutputSlot(int moduleId, string slotName)
        {
            var mod = GetModule(moduleId);
            if (mod)
                return mod.GetOutputSlot(slotName);
            else
                return null;
        }

        /// <summary>
        /// Gets a module's output slot by module name and slotName
        /// </summary>
        /// <param name="moduleName">Name of the module</param>
        /// <param name="slotName">Name of the slot</param>
        public CGModuleOutputSlot GetModuleOutputSlot(string moduleName, string slotName)
        {
            var mod = GetModule(moduleName);
            if (mod)
                return mod.GetOutputSlot(slotName);
            else
                return null;
        }
       
        /// <summary>
        /// Initializes the Generator
        /// </summary>
        /// <param name="force">true to force reinitialization</param>
        public void Initialize(bool force=false)
        {
            if (!mInitializedPhaseOne || force)
            {
                // Read modules
                Modules = new List<CGModule>(GetComponentsInChildren<CGModule>());
                ModulesByID.Clear();
                for (int i = 0; i < Modules.Count; i++)
                {
                    if (!Modules[i].IsInitialized || force)
                        Modules[i].Initialize();

                    if (ModulesByID.ContainsKey(Modules[i].UniqueID))
                    {
                        Debug.LogError("ID of '" + Modules[i].ModuleName + "' isn't unique!");
                        return;
                    }
                    ModulesByID.Add(Modules[i].UniqueID, Modules[i]);
                }
                
                if (Modules.Count > 0)
                {
                    // Sort them
                    sortModulesINTERNAL();
                }
                mInitializedPhaseOne = true;
            }
            for (int m = 0; m < Modules.Count; m++)
                if (Modules[m] is IExternalInput && !Modules[m].IsInitialized)
                    return;

            mInitialized = true;
            mInitializedPhaseOne = false;
            mNeedSort = mNeedSort || force;
            Refresh(true);
        }

        /// <summary>
        /// Refreshes the Generator
        /// </summary>
        /// <param name="forceUpdate">true to force a refresh of all modules</param>
        public void Refresh(bool forceUpdate = false)
        {
            if (!IsInitialized)
                return;
            if (mNeedSort)
                doSortModules();
            
            CGModule firstChanged=null;

            for (int i = 0; i < Modules.Count; i++)
            {
                if (forceUpdate && Modules[i] is IOnRequestProcessing)
                    Modules[i].Dirty = true; // Dirty state will be resetted to false, but last data will be deleted - forcing a recalculation
                if (!(Modules[i] is INoProcessing) && (Modules[i].Dirty || (forceUpdate && !(Modules[i] is IOnRequestProcessing))))
                {
                    Modules[i].checkOnStateChangedINTERNAL();
                    if (Modules[i].IsInitialized && Modules[i].IsConfigured)
                    {

                        if (firstChanged==null)
                        {
#if UNITY_EDITOR || CURVY_DEBUG
                            DEBUG_ExecutionTime.Start();
#endif
                            firstChanged = Modules[i];
                        }

                        //Debug.Log("Refresh " + Modules[i].ModuleName);
                        Modules[i].doRefresh();
                        //Debug.Log("Updated " + Modules[i].ModuleName);
                    }
                    
                    //else
                    //{
                    //Debug.Log("Early OUT at "+Modules[i].ModuleName);
                    //break;

                    //}
                }
                
            }
            if (firstChanged!=null)
            {
#if UNITY_EDITOR || CURVY_DEBUG
                DEBUG_ExecutionTime.Stop();
                if (!Application.isPlaying)
                    EditorUtility.UnloadUnusedAssetsImmediate();
#endif
                OnRefreshEvent(new CurvyCGEventArgs(this,firstChanged));
            }
        }

        #endregion

        #region ### Protected Members ###

        protected CurvyCGEventArgs OnRefreshEvent(CurvyCGEventArgs e)
        {
            if (OnRefresh != null)
                OnRefresh.Invoke(e);
            return e;
        }

        #endregion
        
        #region ### Privates and Internals ###
        /*! \cond PRIVATE */
        
        void clearModules()
        {
            for (int i = Modules.Count - 1; i >= 0; i--)
            {
                if (Application.isPlaying)
                    Destroy(Modules[i].gameObject);
#if UNITY_EDITOR
            else
                Undo.DestroyObjectImmediate(Modules[i].gameObject);
#endif
            }

            Modules.Clear();
            ModulesByID.Clear();
            m_LastModuleID = 0;
        }

        /// <summary>
        /// Ensures a module name is unique
        /// </summary>
        /// <param name="name">desired name</param>
        /// <returns>unique name</returns>
        public string getUniqueModuleNameINTERNAL(string name)
        {
            string newName = name;
            bool isUnique;
            int c = 1;
            do
            {
                isUnique = true;
                foreach (var mod in Modules)
                {
                    if (mod.ModuleName.Equals(newName, System.StringComparison.CurrentCultureIgnoreCase))
                    {
                        newName = name + (c++).ToString();
                        isUnique = false;
                        break;
                    }
                }

            } while (!isUnique);
            return newName;
        }

        

        /// <summary>
        /// INTERNAL! Don't call this by yourself! 
        /// </summary>
        internal void sortModulesINTERNAL()
        {
            mNeedSort = true;   
        }

        bool doSortModules()
        {
            List<CGModule> unsorted = new List<CGModule>(Modules);
            
            List<CGModule> noAncestor = new List<CGModule>();
            List<CGModule> needNoSort = new List<CGModule>();
            
           
            // initialize
            for (int m = unsorted.Count - 1; m >= 0; m--)
            {
                unsorted[m].initializeSort();
                if (unsorted[m] is INoProcessing)
                {
                    needNoSort.Add(unsorted[m]);
                    unsorted.RemoveAt(m);
                } 
                else if (unsorted[m].SortAncestors == 0)
                {
                    noAncestor.Add(unsorted[m]);
                    unsorted.RemoveAt(m);
                }
            }
            
            Modules.Clear();
            
            // Sort
            int index = 0;
            while (noAncestor.Count > 0)
            {
                // get a module without ancestors
                var mod = noAncestor[0];
                noAncestor.RemoveAt(0);
                // decrement child ancestors and fetch childs without ancestors
                var newModsWithoutAncestors = mod.decrementChilds();
                // Add them to noAncestor list
                noAncestor.AddRange(newModsWithoutAncestors);
                // and remove from unsorted
                for (int i = 0; i < newModsWithoutAncestors.Count; i++)
                    unsorted.Remove(newModsWithoutAncestors[i]);
                // add current module to sorted
                Modules.Add(mod);
                mod.transform.SetSiblingIndex(index++);
            }
            
            // These modules got errors!
            for (int circ=0;circ<unsorted.Count;circ++)
                unsorted[circ].CircularReferenceError = true;

            //Debug.Log("====: NeedNoSort=" + needNoSort.Count + ", Unsorted=" + unsorted.Count);
            //foreach (var m in Modules)
            //    Debug.Log("Sort: " + m.ModuleName);

            Modules.AddRange(unsorted);
            Modules.AddRange(needNoSort);
            
            
            
            mNeedSort = false;
            return (unsorted.Count > 0);
        }

        /*! \endcond */
        #endregion
       
  
    }
}
