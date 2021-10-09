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
using System.Collections.Generic;
using System.Reflection;
using FluffyUnderware.DevTools.Extensions;

using System.Collections;
using FluffyUnderware.DevTools;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.AnimatedValues;
#endif




namespace FluffyUnderware.Curvy.Generator
{
    /// <summary>
    /// Curvy Generator module base class
    /// </summary>
    [ExecuteInEditMode]
    public class CGModule : DTVersionedMonoBehaviour
    {
#region ### Events ###

        [Group("Events", Expanded = false, Sort = 1000)]
        [SerializeField]
        CurvyCGEvent m_OnBeforeRefresh = new CurvyCGEvent();
        [Group("Events")]
        [SerializeField]
        CurvyCGEvent m_OnRefresh=new CurvyCGEvent();

        public CurvyCGEvent OnBeforeRefresh
        {
            get { return m_OnBeforeRefresh; }
            set
            {
                if (m_OnBeforeRefresh != value)
                    m_OnBeforeRefresh = value;

            }

        }

        public CurvyCGEvent OnRefresh
        {
            get { return m_OnRefresh; }
            set
            {
                if (m_OnRefresh!=value)
                    m_OnRefresh = value;
            
            }
            
        }

        protected CurvyCGEventArgs OnBeforeRefreshEvent(CurvyCGEventArgs e)
        {
            if (OnBeforeRefresh != null)
                OnBeforeRefresh.Invoke(e);
            return e;
        }

        protected CurvyCGEventArgs OnRefreshEvent(CurvyCGEventArgs e)
        {
            if (OnRefresh != null)
                OnRefresh.Invoke(e);
            return e;
        }

#endregion

#region ### Public Fields & Properties ###

#region --- Fields ---

        [SerializeField,HideInInspector]
        string m_ModuleName;
        [SerializeField, HideInInspector]
        bool m_Active = true;

        [Group("Seed Options", Expanded = false, Sort = 1001)]
        [GroupCondition("usesRandom")]
        [FieldAction("CBSeedOptions", ShowBelowProperty=true)]
        [SerializeField]
        bool m_RandomizeSeed = false;
        
        [SerializeField,HideInInspector]
        int m_Seed = (int)System.DateTime.Now.Ticks;

#endregion

        
#region --- API Accessors ---

        public string ModuleName
        {
            get { return name; }
            set
            {
                if (name != value)
                {
                    name = value;
                    renameManagedResourcesINTERNAL();
                }
            }
        }

        public bool Active
        {
            get { return m_Active; }
            set 
            {
                if (m_Active != value)
                {
                    m_Active = value;
                    Dirty = true;
                    Generator.sortModulesINTERNAL();
                }
            }
        }

        public int Seed
        {
            get { return m_Seed; }
            set
            {
                if (m_Seed != value)
                    m_Seed = value;
                Dirty = true;
            }
        }

        public bool RandomizeSeed
        {
            get { return m_RandomizeSeed; }
            set
            {
                if (m_RandomizeSeed != value)
                    m_RandomizeSeed = value;
            }
        }

#endregion

       

        [System.NonSerialized]
        public List<string> UIMessages = new List<string>();

        public CurvyGenerator Generator
        {
            get { return mGenerator; }
        }
        CurvyGenerator mGenerator;



#endregion

#region ### Graph Related ###


        public int UniqueID
        {
            get { return m_UniqueID; }
        }
        [SerializeField,HideInInspector]
        int m_UniqueID;

        /// <summary>
        /// Whether this module has circular reference errors
        /// </summary>
        public bool CircularReferenceError { get; set; }

        /// <summary>
        /// Helper for topology sorting
        /// </summary>
        internal int SortAncestors;
        

        [HideInInspector]
        public CGModuleProperties Properties = new CGModuleProperties();
        [HideInInspector]
        public List<CGModuleLink> InputLinks = new List<CGModuleLink>();
        [HideInInspector]
        public List<CGModuleLink> OutputLinks = new List<CGModuleLink>();

        // These will be filled by reflection in OnEnable()
        public Dictionary<string, CGModuleInputSlot> InputByName { get; private set; }
        public Dictionary<string, CGModuleOutputSlot> OutputByName { get; private set; }
        public List<CGModuleInputSlot> Input { get; private set; }
        public List<CGModuleOutputSlot> Output { get; private set; }
        
        public ModuleInfoAttribute Info
        { 
            get 
            {
                if (mInfo == null)
                    mInfo = getInfo();
                return mInfo;
            }
        }

        ModuleInfoAttribute mInfo;

        //-----

        public bool Dirty
        {
            get { return mDirty; }
            set
            {
                if (mDirty != value)
                {
                    mDirty = value;
                }
                //Debug.Log(ModuleName + " set to " + value);
                if (mDirty)
                    setTreeDirtyState();

                if (this is IOnRequestProcessing || this is INoProcessing)
                {
                    mDirty = false;
                    if (Output!=null)
                        for (int i = 0; i < Output.Count; i++)
                            Output[i].LastRequestParameters = null;
                }
                
            }
        }

        bool mDirty = true;
        bool mInitialized = false;
        bool mIsConfiguredInternal;

        bool mStateChangeDirty;
        bool mLastIsConfiguredState;


#endregion

#region ### Debugging ###
#if UNITY_EDITOR || CURVY_DEBUG
        public System.DateTime DEBUG_LastUpdateTime;
        public TimeMeasure DEBUG_ExecutionTime = new TimeMeasure(5);
#endif

#endregion

#region ### Unity Callbacks (Virtual) ###

        protected virtual void Awake()
        {
            mGenerator = GetComponentInParent<CurvyGenerator>();
        }

        protected virtual void OnEnable()
        {
            if (mGenerator)
            {
                Initialize();
                Generator.sortModulesINTERNAL();
            }
        }

        public void Initialize()
        {
            if (!mGenerator)
                mGenerator = GetComponentInParent<CurvyGenerator>();
            if (!mGenerator)
                Invoke("Delete", 0);
            else
            {
                mInfo = getInfo();
                // Version changed?
                CheckForVersionUpgrade();

                if (string.IsNullOrEmpty(ModuleName))
                    if (string.IsNullOrEmpty(Info.ModuleName))
                        ModuleName = Generator.getUniqueModuleNameINTERNAL(Info.MenuName.Substring(Info.MenuName.LastIndexOf("/") + 1));
                    else
                        ModuleName = Generator.getUniqueModuleNameINTERNAL(Info.ModuleName);

                loadSlots();
                mInitialized = true;
            }
        }

        protected virtual void OnDisable()
        {
        }

        protected virtual void OnDestroy()
        {
            bool realDestroy = true;
#if UNITY_EDITOR
            if (EditorApplication.isPlayingOrWillChangePlaymode && !EditorApplication.isPlaying)
                realDestroy = false;
#endif
            setTreeDirtyStateChange();
            List<Component> res;
            List<string> resNames;
            // Resources
            if (realDestroy)
            {
                if (GetManagedResources(out res, out resNames))
                {
                    for (int i = res.Count - 1; i >= 0; i--)
                        DeleteManagedResource(resNames[i], res[i], "", true);
                }
            }

            // Links
            
            var inSlots = GetInputSlots();
            var outSlots = GetOutputSlots();
            foreach (var s in inSlots)
                s.ReInitializeLinkedTargetModules();
            foreach (var s in outSlots)
                s.ReInitializeLinkedTargetModules();

            if (Generator)
            {
                // Delete module
                Generator.ModulesByID.Remove(UniqueID);
                Generator.Modules.Remove(this);
                Generator.sortModulesINTERNAL();
            }
            mInitialized = false;
        }

#if UNITY_EDITOR
        protected virtual void OnValidate() 
        {
        }

        void Update()
        {
            if (!Application.isPlaying)
                renameManagedResourcesINTERNAL();
        }
#endif
        void OnDidApplyAnimationProperties()
        {
            Dirty = true;
        }

#endregion

#region ### Virtual Methods & Properties ###

        /// <summary>
        /// Gets whether the module is properly configured i.e. has everything to work like intended
        /// </summary>
        public virtual bool IsConfigured { 
            get
            {
                
                if (!IsInitialized || CircularReferenceError || !Active)
                {
                    mIsConfiguredInternal = false;
                    
                    return false;
                }
                InputSlotInfo myInfo;
                int validTotalLinks = 0;
                
                for (int i = 0; i < Input.Count; i++)
                {
                    myInfo = Input[i].InputInfo;
                    if (Input[i].IsLinked)
                    {
                        for (int link = 0; link < Input[i].Count; link++)
                            if (Input[i].SourceSlot(link) != null){
                                if (Input[i].SourceSlot(link).Module.IsConfigured)
                                {
                                    validTotalLinks++;
                                }
                                else if (!myInfo.Optional)
                                {
                                    mIsConfiguredInternal = false;
                                    
                                    return false;
                                }

                            }
                        
                    } else
                        if (myInfo == null || !myInfo.Optional)
                        {
                            mIsConfiguredInternal = false;
                            
                            return false;
                        }
                }
                
                mIsConfiguredInternal = (validTotalLinks > 0 || Input.Count==0);
                
                return mIsConfiguredInternal;
            }
        }

        /// <summary>
        /// Gets whether the module and all its dependencies are fully initialized
        /// </summary>
        public virtual bool IsInitialized { get { return mInitialized; } }

        /// <summary>
        /// Add Module processing code in here
        /// </summary>
        public virtual void Refresh()
        {
//            Debug.Log(name + ".Refresh()");
            UIMessages.Clear();
        }

        public virtual void Reset() 
        {
            ModuleName = string.IsNullOrEmpty(Info.ModuleName) ? GetType().Name : Info.ModuleName;
        }

        public void ReInitializeLinkedSlots()
        {
            var ins = GetInputSlots();
            var ous = GetOutputSlots();
            for (var i = 0; i < ins.Count; i++)
                ins[i].ReInitializeLinkedSlots();
            for (var i = 0; i < ous.Count; i++)
                ous[i].ReInitializeLinkedSlots();
        }

        /// <summary>
        /// Called when a module's state changes (Link added/removed, Active toggles etc..)
        /// </summary>
        public virtual void OnStateChange() 
        {
//            Debug.Log(name + ".OSC, configured="+IsConfigured);
            Dirty = true;
            
            if (Output != null)
            {
                for (int i = 0; i < Output.Count; i++)
                {
                    Output[i].ClearData();
                    /*
                    if (Output[i].IsLinked)
                    {
                        var modules = Output[i].GetLinkedModules();
                        for (int m = 0; m < modules.Count; m++)
                            if (modules[m] != this || modules[m].CircularReferenceError) // prevent circular reference
                                modules[m].CheckAndRaiseOnStateChangedINTERNAL();
                    }
                     */
                }
            }
#if UNITY_EDITOR
            if (Input != null)
                for (int i = 0; i < Input.Count; i++)
                    Input[i].LastDataCountINTERNAL = 0;
#endif
        }

        /// <summary>
        /// Called after a module was copied to a template
        /// </summary>
        /// <remarks>Use this handle references that can't be templated etc...</remarks>
        public virtual void OnTemplateCreated()
        {
        }
        
#endregion

#region ### Helpers ###

        /// <summary>
        /// Gets a request parameter of a certain type
        /// </summary>
        /// <typeparam name="T">Type derived from PCGDataRequestParameter</typeparam>
        /// <param name="requests">reference to the list of request parameters</param>
        /// <returns>the wanted request parameter or null</returns>
        protected T GetRequestParameter<T>(ref CGDataRequestParameter[] requests) where T : CGDataRequestParameter
        {
            for (int i = 0; i < requests.Length; i++)
                if (requests[i] is T)
                    return (T)requests[i];

            return null;
        }

        /// <summary>
        /// Removes a certain request parameter from the requests array
        /// </summary>
        /// <param name="requests">reference to the requests array</param>
        /// <param name="request">the request to remove</param>
        protected void RemoveRequestParameter(ref CGDataRequestParameter[] requests, CGDataRequestParameter request)
        {
            for (int i = 0; i < requests.Length; i++)
                if (requests[i] == request)
                {
                    requests = requests.RemoveAt(i);
                    return;
                }

        }

#endregion

#region ### Public Methods ###
        
        
        public CGModuleLink GetOutputLink(CGModuleOutputSlot outSlot, CGModuleInputSlot inSlot)
        {
            return GetLink(OutputLinks, outSlot, inSlot);
        }

        public List<CGModuleLink> GetOutputLinks(CGModuleOutputSlot outSlot)
        {
            return GetLinks(OutputLinks, outSlot);
        }

        public CGModuleLink GetInputLink(CGModuleInputSlot inSlot, CGModuleOutputSlot outSlot)
        {
            return GetLink(InputLinks, inSlot, outSlot);
        }

        public List<CGModuleLink> GetInputLinks(CGModuleInputSlot inSlot)
        {
            return GetLinks(InputLinks, inSlot);
        }
        

        CGModuleLink GetLink(List<CGModuleLink> lst, CGModuleSlot source, CGModuleSlot target)
        {
            for (int i = 0; i < lst.Count; i++)
                if (lst[i].IsSame(source, target))
                    return lst[i];
            return null;
        }

        List<CGModuleLink> GetLinks(List<CGModuleLink> lst, CGModuleSlot source)
        {
            var res = new List<CGModuleLink>();
            for (int i = 0; i < lst.Count; i++)
                if (lst[i].IsFrom(source))
                    res.Add(lst[i]);
            return res;
        }


        public CGModule CopyTo(CurvyGenerator targetGenerator)
        {
            var newModule = this.DuplicateGameObject<CGModule>(targetGenerator.transform, false);
            newModule.mGenerator = targetGenerator;
            newModule.Initialize();
            newModule.ModuleName = ModuleName;
            newModule.ModuleName = targetGenerator.getUniqueModuleNameINTERNAL(newModule.ModuleName);
            newModule.SetUniqueIdINTERNAL();
            newModule.renameManagedResourcesINTERNAL();
            return newModule;
        }

        public Component AddManagedResource(string resourceName, string context="", int index=-1)
        {
            var res = CGResourceHandler.CreateResource(this, resourceName, context);
            RenameResource(resourceName+context, res, index);
            res.transform.SetParent(transform);
            return res;
        }


        public void DeleteManagedResource(string resourceName,Component res, string context="",bool dontUsePool=false)
        {
            if (res)
                CGResourceHandler.DestroyResource(this, resourceName, res, context, dontUsePool);
        }

        public bool IsManagedResource(Component res)
        {
            return (res && res.transform.parent == transform);//res.gameObject.GetComponentInParent<CurvyGenerator>() == Generator);
        }

        protected void RenameResource(string resourceName, Component resource, int index=-1)
        {
            resource.name=string.Format("{0}_{1}_{2}", ModuleName, UniqueID, resourceName);
            if (index>-1)
                resource.name += string.Format("{0:000}", index);
        }

        protected PrefabPool GetPrefabPool(GameObject prefab)
        {
            return Generator.PoolManager.GetPrefabPool(UniqueID.ToString() + "_" + prefab.name, prefab);
        }

        public List<IPool> GetAllPrefabPools()
        {
            return Generator.PoolManager.FindPools(UniqueID.ToString() + "_");
        }

        public void DeleteAllPrefabPools()
        {
            Generator.PoolManager.DeletePools(UniqueID.ToString() + "_");
        }

        public void Delete()
        {
            OnStateChange();
            if (Application.isPlaying)
                Destroy(gameObject);
#if UNITY_EDITOR
            else
                Undo.DestroyObjectImmediate(gameObject);
#endif
        }

        public CGModuleInputSlot GetInputSlot(string name)
        {
            return (InputByName!=null && InputByName.ContainsKey(name)) ? InputByName[name] : null;
        }

        public List<CGModuleInputSlot> GetInputSlots(System.Type filterType = null)
        {
            if (filterType == null)
                return new List<CGModuleInputSlot>(Input);
            else
            {
                var res = new List<CGModuleInputSlot>();
                for (int i = 0; i < Output.Count; i++)
#if NETFX_CORE
                    if (Output[i].Info.DataTypes[0] == filterType || Output[i].Info.DataTypes[0].GetTypeInfo().IsSubclassOf(filterType))
#else
                    if (Output[i].Info.DataTypes[0] == filterType || Output[i].Info.DataTypes[0].IsSubclassOf(filterType))
#endif
                        res.Add(Input[i]);

                return res;
            }
        }

        public CGModuleOutputSlot GetOutputSlot(string name)
        {
            return (OutputByName!=null && OutputByName.ContainsKey(name)) ? OutputByName[name] : null;
        }

        public List<CGModuleOutputSlot> GetOutputSlots(System.Type filterType=null)
        {
            if (filterType == null)
                return new List<CGModuleOutputSlot>(Output);
            else
            {
                var res = new List<CGModuleOutputSlot>();
                for (int i = 0; i < Output.Count; i++)
#if NETFX_CORE
                    if (Output[i].Info.DataTypes[0] == filterType || Output[i].Info.DataTypes[0].GetTypeInfo().IsSubclassOf(filterType))
#else
                    if (Output[i].Info.DataTypes[0] == filterType || Output[i].Info.DataTypes[0].IsSubclassOf(filterType))
#endif
                        res.Add(Output[i]);

                return res;
            }
        }

        public bool GetManagedResources(out List<Component>components, out List<string>componentNames)
        {
            components = new List<Component>();
            componentNames = new List<string>();
            var fields = GetType().GetAllFields(false, true);
            foreach (var f in fields)
            {
                var at = f.GetCustomAttribute<CGResourceManagerAttribute>();
                if (at!= null)
                {
#if NETFX_CORE
                    if (typeof(ICGResourceCollection).GetTypeInfo().IsAssignableFrom(f.FieldType.GetTypeInfo()))
#else
                    if (typeof(ICGResourceCollection).IsAssignableFrom(f.FieldType))
#endif
                    {
                        ICGResourceCollection col = f.GetValue(this) as ICGResourceCollection;
                        if (col!=null)
                        {
                            var items=col.ItemsArray;
                            foreach (var c in items)
                            {
                                if (c.transform.parent == transform)
                                {
                                    components.Add(c);
                                    componentNames.Add(at.ResourceName);
                                }
                            }
                        }
                    }
                    else
                    {
                        Component cmp = f.GetValue(this) as Component;
                        if (cmp && cmp.transform.parent == transform)
                        {
                            components.Add(cmp);
                            componentNames.Add(at.ResourceName);
                        }
                    }
                }
            }

            return (components.Count > 0);
        }

        

#endregion

#region ### Privates and Internals ###
        /*! \cond PRIVATE */
        /*! @name Internal Public
         *  Don't use them unless you know what you're doing!
         */
        //@{

        /*
        public void CheckAndRaiseOnStateChangedINTERNAL()
        {
            
            bool b = mLastIsConfiguredState;
            Debug.Log(name + " check ="+b+":"+IsConfigured);
            if (IsConfigured != b)
                OnStateChange();
            
        }
         */

        public int SetUniqueIdINTERNAL()
        {
            m_UniqueID = ++Generator.m_LastModuleID;
            return m_UniqueID;
        }

        /// <summary>
        /// Initializes SortAncestor with number of connected Input links
        /// </summary>
        internal void initializeSort()
        {
            SortAncestors = 0;
            CircularReferenceError = false;
            //if (Active)
            //{
                for (int i = 0; i < Input.Count; i++)
                    if (Input[i].IsLinked)
                        SortAncestors++;
            //}
        }
        /// <summary>
        /// Decrement SortAncestor of linked modules and return a list of childs where SortAncestor==0
        /// </summary>
        /// <returns></returns>
        internal List<CGModule> decrementChilds()
        {
            List<CGModule> noAncestors = new List<CGModule>();
            for (int s = 0; s < Output.Count; s++)
                for (int l = 0; l < Output[s].LinkedSlots.Count; l++)
                {
                    if (--Output[s].LinkedSlots[l].Module.SortAncestors==0)
                        noAncestors.Add(Output[s].LinkedSlots[l].Module);
                }

            return noAncestors;
        }

        internal void doRefresh()
        {
#if UNITY_EDITOR || CURVY_DEBUG
            DEBUG_LastUpdateTime = System.DateTime.Now;
                DEBUG_ExecutionTime.Start();
#endif

            if (RandomizeSeed)
                    setSeed((int)System.DateTime.Now.Ticks);
                else
                    setSeed(Seed);
                OnBeforeRefreshEvent(new CurvyCGEventArgs(this));
                Refresh();
            setSeed((int)System.DateTime.Now.Ticks);

#if UNITY_EDITOR || CURVY_DEBUG
            DEBUG_ExecutionTime.Stop();
#endif
                OnRefreshEvent(new CurvyCGEventArgs(this));
            
            mDirty = false;
        }

        void setSeed(int seed)
        {
#if PRE_UNITY_5_4
            Random.seed=seed;
#else
            Random.InitState(seed);
#endif
        }

        internal ModuleInfoAttribute getInfo()
        {
#if NETFX_CORE
            object[] inf = (object[])GetType().GetTypeInfo().GetCustomAttributes(typeof(ModuleInfoAttribute), true);
#else
            var inf = GetType().GetCustomAttributes(typeof(ModuleInfoAttribute), true);
#endif
            return (inf.Length > 0) ? (ModuleInfoAttribute)inf[0] : null;
        }

        bool usesRandom()
        {
            return (Info != null & Info.UsesRandom);
        }

        void loadSlots()
        {
            // Get list of Slots
            InputByName = new Dictionary<string, CGModuleInputSlot>();
            OutputByName = new Dictionary<string, CGModuleOutputSlot>();
            Input = new List<CGModuleInputSlot>();
            Output = new List<CGModuleOutputSlot>();
            var fields = GetType().GetAllFields();
            //Debug.Log(name + ".loadSlots()");
            foreach (var f in fields)
            {
                if (f.FieldType == typeof(CGModuleInputSlot))
                {
                    var s = (CGModuleInputSlot)f.GetValue(this);
                    s.Module = this;
                    s.Info = getSlotInfo(f);
                    s.ReInitializeLinkedSlots();
                    InputByName.Add(s.Info.Name, s);
                    Input.Add(s);
                }
                else if (f.FieldType == typeof(CGModuleOutputSlot))
                {
                    var s = (CGModuleOutputSlot)f.GetValue(this);
                    s.Module = this;
                    s.Info = getSlotInfo(f);
                    s.ReInitializeLinkedSlots();
                    OutputByName.Add(s.Info.Name, s);
                    Output.Add(s);
                }
            }
        }

        SlotInfo getSlotInfo(FieldInfo f)
        {
            var si = f.GetCustomAttribute<SlotInfo>();
            if (si!=null)
            {
                if (string.IsNullOrEmpty(si.Name))
                    si.Name = f.Name.TrimStart("In").TrimStart("Out");
                for (int x=0;x<si.DataTypes.Length;x++)
#if NETFX_CORE
                    if (!si.DataTypes[x].GetTypeInfo().IsSubclassOf(typeof(CGData)))
#else
                    if (!si.DataTypes[x].IsSubclassOf(typeof(CGData)))
#endif
                        Debug.LogError(string.Format("{0}, Slot '{1}': Data type needs to be subclass of CGData!", GetType().Name, si.Name));
                return si;
            }
            Debug.LogError("The Slot '" + f.Name + "' of type '" + f.DeclaringType.Name + "' needs a SlotInfo attribute!");
            return null;
        }

        void setTreeDirtyStateChange()
        {
            mStateChangeDirty = true;
            if (Output != null)
            {
                for (int i = 0; i < Output.Count; i++)
                {
                    if (Output[i].IsLinked)
                    {
                        var modules = Output[i].GetLinkedModules();
                        for (int m = 0; m < modules.Count; m++)
                            if (modules[m] != this || modules[m].CircularReferenceError) // prevent circular reference
                                modules[m].setTreeDirtyStateChange();
                    }
                }
            }
        }

        void setTreeDirtyState()
        {
//            Debug.Log(name + ".setDirty");
            bool isc=IsConfigured;
            if (mLastIsConfiguredState != isc)
                mStateChangeDirty = true;
            mLastIsConfiguredState = isc;
            if (Output != null)
            {
                for (int i = 0; i < Output.Count; i++)
                {
                    //Output[i].ClearData();
                    /*
                    if (this is IOnRequestProcessing)
                    {
                        Output[i].Data = null;
                    }
                     */
                    if (Output[i].IsLinked)
                    {
                        var modules = Output[i].GetLinkedModules();
                        for (int m = 0; m < modules.Count; m++)
                            if (modules[m] != this || modules[m].CircularReferenceError) // prevent circular reference
                                modules[m].Dirty = true; // recursion will be done by property setter, so no check here!
                    }
                }
            }
            
        }

        public void checkOnStateChangedINTERNAL()
        {
//            Debug.Log(ModuleName+".Check: " + mStateChangeDirty);
            if (mStateChangeDirty)
                OnStateChange();
            mStateChangeDirty = false;
        }
        

        public void renameManagedResourcesINTERNAL()
        {
            var fields = GetType().GetAllFields(false, true);
            foreach (var f in fields)
            {
                var at = f.GetCustomAttribute<CGResourceManagerAttribute>();
                if (at!=null)
                {
                    Component cmp = f.GetValue(this) as Component;
                    if (cmp && cmp.transform.parent == this.transform)
                        RenameResource(at.ResourceName, cmp);
                }
            }
        }


        //@}
        /*! \endcond */
#endregion


    }

    /// <summary>
    /// Attribute defining basic module properties
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Class)]
    public class ModuleInfoAttribute : System.Attribute, System.IComparable
    {
        /// <summary>
        /// Menu-Name of the module (without '/')
        /// </summary>
        public readonly string MenuName;
        /// <summary>
        /// Default Module name
        /// </summary>
        public string ModuleName;
        /// <summary>
        /// Tooltip Info
        /// </summary>
        public string Description;
        
        /// <summary>
        /// Whether the module uses Random, i.e. should show Seed options
        /// </summary>
        public bool UsesRandom;
        
        public ModuleInfoAttribute(string name)
        {
            MenuName = name;
        }

        public int CompareTo(object obj)
        {
            return MenuName.CompareTo(((ModuleInfoAttribute)obj).MenuName);
        }

    }

   
    /// <summary>
    /// CGModule helper class
    /// </summary>
    [System.Serializable]
    public class CGModuleProperties
    {
        public Rect Dimensions;
#if UNITY_EDITOR
        public AnimBool Expanded;
#endif
        public float MinWidth = 250;
        public float LabelWidth;
        public Color BackgroundColor = Color.black;

        public CGModuleProperties()
        {
#if UNITY_EDITOR
            Expanded = new AnimBool(true);
            Expanded.speed = 3;
#endif
        }
    }


}
