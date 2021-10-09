// =====================================================================
// Copyright 2013-2017 Fluffy Underware
// All rights reserved
// 
// http://www.fluffyunderware.com
// =====================================================================

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Reflection;

namespace FluffyUnderware.Curvy.Generator
{
    /// <summary>
    /// Class defining a module slot
    /// </summary>
    public class CGModuleSlot
    {
        /// <summary>
        /// The Module this Slot belongs to
        /// </summary>
        public CGModule Module { get; internal set; }
        /// <summary>
        /// Gets the SlotInfo Attribute
        /// </summary>
        public SlotInfo Info { get; internal set; }

        /// <summary>
        /// Origin of Link-Wire
        /// </summary>
        public Vector2 Origin { get; set; }
        /// <summary>
        /// Mouse-Hotzone
        /// </summary>
        public Rect DropZone { get; set; }

        /// <summary>
        /// Whether the link is wired or not
        /// </summary>
        public bool IsLinked { get { return LinkedSlots!=null && LinkedSlots.Count > 0; } }
        /// <summary>
        /// Whether the link is wired and all connected modules are configured
        /// </summary>
        public bool IsLinkedAndConfigured
        {
            get
            {
                if (!IsLinked)
                    return false;
                for (int i = 0; i < LinkedSlots.Count; i++)
                    if (!LinkedSlots[i].Module.IsConfigured)
                        return false;
                return true;
            }
        }
        /// <summary>
        /// Gets (IOnRequestProcessing)Module
        /// </summary>
        public IOnRequestProcessing OnRequestModule { get { return Module as IOnRequestProcessing; } }
        /// <summary>
        /// Gets (IOnRequestPath)Module
        /// </summary>
        public IOnRequestPath OnRequestPathModule { get { return Module as IOnRequestPath; } }
        /// <summary>
        /// Gets (IExternalInput)Module
        /// </summary>
        public IExternalInput ExternalInput { get { return Module as IExternalInput; } }
        /// <summary>
        /// All slots of linked modules
        /// </summary>
        public List<CGModuleSlot> LinkedSlots
        {
            get
            {
                if (mLinkedSlots == null)
                    LoadLinkedSlots();
                return mLinkedSlots ?? new List<CGModuleSlot>();
            }
        }
        /// <summary>
        /// Gets the number of connected links, i.e. shortcut to this.Links.Count
        /// </summary>
        public int Count
        {
            get { return LinkedSlots.Count; }
        }

        public string Name
        {
            get { return (Info!=null) ? Info.Name : ""; }
        }

        protected List<CGModuleSlot>mLinkedSlots = null;

        public CGModuleSlot()
        {
            
        }

        public bool HasLinkTo(CGModuleSlot other)
        {
            for (int i = 0; i < LinkedSlots.Count; i++)
                if (LinkedSlots[i] == other)
                    return true;

            return false;
        }

        /// <summary>
        /// Gets a list of all Links' modules
        /// </summary>
        public List<CGModule> GetLinkedModules()
        {
            var res = new List<CGModule>();
            for (int i = 0; i < LinkedSlots.Count; i++)
                res.Add(LinkedSlots[i].Module);
            return res;
        }

        public virtual void LinkTo(CGModuleSlot other)
        {
            if (Module)
            {
                Module.Generator.sortModulesINTERNAL();
                Module.Dirty = true;
            }
            if (other.Module)
                other.Module.Dirty = true;
        }

        public virtual void UnlinkFrom(CGModuleSlot other)
        {
            if (Module)
            {
                Module.Generator.sortModulesINTERNAL();
                Module.Dirty = true;
            }
            if (other.Module)
                other.Module.Dirty = true;
        }

        public virtual void UnlinkAll()
        {
        }

        public void ReInitializeLinkedSlots()
        {
            mLinkedSlots = null;
        }

        public void ReInitializeLinkedTargetModules()
        {
            var mods = GetLinkedModules();
            foreach (var m in mods)
                if (m!=null)
                    m.ReInitializeLinkedSlots();
        }

        

        protected virtual void LoadLinkedSlots() 
        {
        }
        
        public static implicit operator bool(CGModuleSlot a)
        {
            return !object.ReferenceEquals(a, null);
        }

        public override string ToString()
        {
            return string.Format("{0}: {1}.{2}", GetType().Name, Module.name, Name);
        }

    }

    /// <summary>
    /// Class defining a module's input slot
    /// </summary>
    [System.Serializable]
    public class CGModuleInputSlot : CGModuleSlot
    {
        public InputSlotInfo InputInfo { get { return Info as InputSlotInfo; } }
#if UNITY_EDITOR
        public int LastDataCountINTERNAL { get; set; }
#endif
        public CGModuleInputSlot() : base() { }


        protected override void LoadLinkedSlots()
        {
            if (!Module.Generator.IsInitialized)
                return;
            base.LoadLinkedSlots();
            mLinkedSlots=new List<CGModuleSlot>();
            var lnks=Module.GetInputLinks(this);
            foreach (var l in lnks)
            {
                var mod=Module.Generator.GetModule(l.TargetModuleID, true);
                if (mod)
                {
                    var slot = mod.OutputByName[l.TargetSlotName];
                    // Sanitize missing links
                    if (!slot.Module.GetOutputLink(slot, this))
                    {
                        slot.Module.OutputLinks.Add(new CGModuleLink(slot, this));
                        slot.ReInitializeLinkedSlots();
                    }

                    if (!mLinkedSlots.Contains(slot))
                        mLinkedSlots.Add(slot);
                }
                else
                {
                    Module.InputLinks.Remove(l);
                }
            }
        }

        public override void UnlinkAll()
        {
            var ls = new List<CGModuleSlot>(LinkedSlots);
            foreach (var l in ls)
            {
                UnlinkFrom(l);
            }
        }

        public override void LinkTo(CGModuleSlot outputSlot)
        {
            if (!HasLinkTo(outputSlot))
            {
                Module.InputLinks.Add(new CGModuleLink(this, outputSlot));
                outputSlot.Module.OutputLinks.Add(new CGModuleLink(outputSlot, this));
                if (!LinkedSlots.Contains(outputSlot))
                    LinkedSlots.Add(outputSlot);

                if (!outputSlot.LinkedSlots.Contains(this))
                    outputSlot.LinkedSlots.Add(this);

                base.LinkTo(outputSlot);
            }
        }

        public override void UnlinkFrom(CGModuleSlot outputSlot)
        {
            if (HasLinkTo(outputSlot))
            {
                var l1 = Module.GetInputLink(this, (CGModuleOutputSlot)outputSlot);
                Module.InputLinks.Remove(l1);
                var l2 = outputSlot.Module.GetOutputLink((CGModuleOutputSlot)outputSlot, this);
                outputSlot.Module.OutputLinks.Remove(l2);

                LinkedSlots.Remove(outputSlot);
                outputSlot.LinkedSlots.Remove(this);

                base.UnlinkFrom(outputSlot);
            }
        }

       

        /// <summary>
        /// Gets a linked Output slot
        /// </summary>
        public CGModuleOutputSlot SourceSlot (int index=0)
        {
            return (index<Count && index>=0) ?(CGModuleOutputSlot)LinkedSlots[index] : null;
        }

        /// <summary>
        /// Determines if a particular output slot of another module can link to this slot
        /// </summary>
        /// <param name="source">the slot of the other module that'd like to link to this input slot</param>
        /// <returns>whether linking is allowed or not</returns>
        /// <remarks>By default it checks against datatype, MaxInput and self reference</remarks>
        public virtual bool IsValidTarget(CGModuleOutputSlot source)
        {
            var myInfo = InputInfo;
            return (source.Module != Module &&
                    myInfo.IsValidFrom(source.OutputInfo.DataType) &&
                    ((source.OnRequestModule != null && (myInfo.RequestDataOnly || OnRequestModule != null)) || (source.OnRequestModule == null && !myInfo.RequestDataOnly))
                    );
        }
        /// <summary>
        /// Gets the module connected to the link
        /// </summary>
        /// <param name="index">the link index</param>
        /// <returns>a module</returns>
        CGModule SourceModule(int index)
        {
            return (index<Count && index>=0) ?LinkedSlots[index].Module : null;
        }

        /// <summary>
        /// Gets the data from the module connected to a certain input slot. If more than one module is connected, the first module's data is returned
        /// </summary>
        /// <typeparam name="T">type of requested data</typeparam>
        /// <param name="requests">request parameters</param>
        /// <returns>the data</returns>
        public T GetData<T>(params CGDataRequestParameter[] requests) where T : CGData
        {
            var data=GetData<T>(0, requests);
#if UNITY_EDITOR
            LastDataCountINTERNAL = (data == null || data.Length == 0) ? 0 : data.Length;
#endif
            return (data==null || data.Length == 0) ? null : data[0] as T;
        }

        /// <summary>
        /// Gets the data from all modules connected to a certain input slot.
        /// </summary>
        /// <typeparam name="T">type of requested data</typeparam>
        /// <param name="requests">request parameters</param>
        /// <returns>the data</returns>
        public List<T> GetAllData<T>(params CGDataRequestParameter[] requests) where T : CGData
        {
            var res = new List<T>();
            for (int i = 0; i < Count; i++)
            {
                var data = GetData<T>(i, requests);
                if (data!=null)
                    if (!Info.Array)
                    {
                        res.Add(data[0] as T);
                        break;
                    } else {
                        res.Capacity += data.Length;
                        for (int a=0;a<data.Length;a++)
                            res.Add(data[a] as T);
                        }
            }
#if UNITY_EDITOR
            LastDataCountINTERNAL = res.Count;
#endif
            return res;
        }

        /// <summary>
        /// Gets the data from the module connected to a certain input slot
        /// </summary>
        /// <typeparam name="T">type of requested data</typeparam>
        /// <param name="slotIndex">slot index (if the slot supports multiple inputs)</param>
        /// <param name="requests">request parameters</param>
        /// <returns>the data</returns>
        CGData[] GetData<T>(int slotIndex,params CGDataRequestParameter[] requests) where T: CGData
        {
            var source = SourceSlot(slotIndex);
            if (source)
            {
                // inactive modules don't return anything
                if (!source.Module.Active)
                    return new T[0];
                // Check if we need to provide a copy of the data, using the following rule:
                // If this slot is flagged as ModifiesData AND
                // (Source has multiple outgoing links OR
                // (this has multiple incoming links)
                bool needCopy = (InputInfo.ModifiesData && (source.Module is IOnRequestProcessing || source.Count > 1));// || Links.Count>1);
                //if (needCopy)
                //Debug.Log(Module.ModuleName + " gets new Data!");
                // HANDLE IOnRequestProcessing modules (i.e. modules that provides data on the fly)
                if (source.Module is IOnRequestProcessing)
                {
                    bool needNewData = (source.Data == null || source.Data.Length==0);
                    // Return last data?
                    if (!needNewData && source.LastRequestParameters != null && source.LastRequestParameters.Length == requests.Length)
                    {
                        for (int i = 0; i < requests.Length; i++)
                            if (!requests[i].Equals(source.LastRequestParameters[i]))
                            {
                                needNewData = true;
                                break;
                            }
                    }
                    else
                        needNewData = true;

                    if (needNewData)
                    {
                        
                        source.LastRequestParameters = requests;
#if UNITY_EDITOR || CURVY_DEBUG
                        source.Module.DEBUG_LastUpdateTime = System.DateTime.Now;
                        Module.DEBUG_ExecutionTime.Pause();
                        source.Module.DEBUG_ExecutionTime.Start();
#endif
                        source.SetData(((IOnRequestProcessing)source.Module).OnSlotDataRequest(this, source, requests));
#if UNITY_EDITOR || CURVY_DEBUG
                        source.Module.DEBUG_ExecutionTime.Stop();
                        Module.DEBUG_ExecutionTime.Start();
#endif
                    }
                    if (needCopy)
                        return cloneData<T>(ref source.Data);
                    else
                        return source.Data;
                }
                else // HANDLE regular modules
                {
                    if (needCopy)
                        return cloneData<T>(ref source.Data);
                    else
                        return source.Data; // caching implied
                }
            }   
            return new CGData[0];
        }

        CGData[] cloneData<T>(ref CGData[] source) where T : CGData
        {
            var d = new T[source.Length];
            for (int i = 0; i < source.Length; i++)
                d[i] = source[i].Clone<T>();
            return d;
        }

    }

    /// <summary>
    /// Class defining a module's output slot
    /// </summary>
    [System.Serializable]
    public class CGModuleOutputSlot : CGModuleSlot
    {
        public OutputSlotInfo OutputInfo { get { return Info as OutputSlotInfo; } }
        public CGData[] Data = new CGData[0];
        public CGDataRequestParameter[] LastRequestParameters; // used for caching of Virtual Modules

        public CGModuleOutputSlot() : base() { }

        protected override void LoadLinkedSlots()
        {
            if (!Module.Generator.IsInitialized)
                return;
            base.LoadLinkedSlots();
            mLinkedSlots = new List<CGModuleSlot>();
            var lnks = Module.GetOutputLinks(this);
            foreach (var l in lnks)
            {
                var mod=Module.Generator.GetModule(l.TargetModuleID, true);
                if (mod)
                {
                    var slot = mod.InputByName[l.TargetSlotName];

                    // Sanitize missing links
                    if (!slot.Module.GetInputLink(slot, this))
                    {
                        slot.Module.InputLinks.Add(new CGModuleLink(slot, this));
                        slot.ReInitializeLinkedSlots();
                    }

                    if (!mLinkedSlots.Contains(slot))
                        mLinkedSlots.Add(slot);
                }
                else
                {
                    Module.OutputLinks.Remove(l);
                }
            }
        }

        public override void LinkTo(CGModuleSlot inputSlot)
        {
            if (!HasLinkTo(inputSlot))
            {
                if (!inputSlot.Info.Array && inputSlot.IsLinked)
                    inputSlot.UnlinkAll();
                Module.OutputLinks.Add(new CGModuleLink(this, inputSlot));
                inputSlot.Module.InputLinks.Add(new CGModuleLink(inputSlot, this));
                if (!LinkedSlots.Contains(inputSlot))
                    LinkedSlots.Add(inputSlot);
                if (!inputSlot.LinkedSlots.Contains(this))
                    inputSlot.LinkedSlots.Add(this);

                base.LinkTo(inputSlot);
            }
        }

        public override void UnlinkFrom(CGModuleSlot inputSlot)
        {
            if (HasLinkTo(inputSlot))
            {
                var l1 = Module.GetOutputLink(this, (CGModuleInputSlot)inputSlot);
                Module.OutputLinks.Remove(l1);
                
                var l2 = inputSlot.Module.GetInputLink((CGModuleInputSlot)inputSlot, this);
                inputSlot.Module.InputLinks.Remove(l2);

                LinkedSlots.Remove(inputSlot);
                inputSlot.LinkedSlots.Remove(this);

                base.UnlinkFrom(inputSlot);
            }
        }

      

        public bool HasData
        {
            get { return Data != null && Data.Length > 0 && Data[0] != null; }
        }

        public void ClearData()
        {
            Data = new CGData[0];
        }

        public void SetData<T>(List<T> data) where T:CGData
        {
            if (data == null)
                Data = new CGData[0];
            else
            {
                if (!Info.Array && data.Count > 1)
                    Debug.LogWarning("[Curvy] " + Module.GetType().Name + " ("+Info.Name+") only supports a single data item! Either avoid calculating unneccessary data or define the slot as an array!");
                Data = data.ToArray();
            }
        }

        public void SetData(params CGData[] data)
        {
            Data = (data==null) ? new CGData[0] : data;
        }

        public T GetData<T>() where T:CGData
        {
            return (Data.Length==0) ? null : Data[0] as T;
        }

        public T[] GetAllData<T>() where T : CGData
        {
            return Data as T[];
        }
    }

    /// <summary>
    /// Attribute to define slot properties
    /// </summary>
    public class SlotInfo : Attribute, IComparable
    {
        public readonly Type[] DataTypes;
        public string Name; // if empty Field's name will be taken
        public string Tooltip;
        public bool Array;

        protected SlotInfo(string name, params Type[] type)
        {
            DataTypes = type;
            Name = name;
        }
        protected SlotInfo(params Type[] type) : this(null, type) { }

        public int CompareTo(object obj)
        {
            return ((SlotInfo)obj).Name.CompareTo(Name);
        }
    }
    /// <summary>
    /// Attribute to define input sot properties
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class InputSlotInfo : SlotInfo
    {
        public bool RequestDataOnly = false;
        public bool Optional = false;
        /// <summary>
        /// Whether this data is altered by the module
        /// </summary>
        public bool ModifiesData = false;

        public InputSlotInfo(string name, params Type[] type) : base(name, type) { }
        public InputSlotInfo(params Type[] type) : this(null, type) { }

        /// <summary>
        /// Gets whether outType is of same type or a subtype of one of our input types
        /// </summary>
        public bool IsValidFrom(Type outType)
        {
            for (int x = 0; x < DataTypes.Length; x++)
#if NETFX_CORE
                if (outType == DataTypes[x] || outType.GetTypeInfo().IsSubclassOf(DataTypes[x]))
#else
                if (outType == DataTypes[x] || outType.IsSubclassOf(DataTypes[x]))
#endif
                    return true;
            return false;
        }
    }

    /// <summary>
    /// Attribute to define output slot properties
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class OutputSlotInfo : SlotInfo
    {
        public Type DataType
        {
            get
            {
                return DataTypes[0];
            }
        }

        public OutputSlotInfo(Type type) : this(null, type) { }

        public OutputSlotInfo(string name, Type type) : base(name,type) { }
    }
}
