// =====================================================================
// Copyright 2013-2017 Fluffy Underware
// All rights reserved
// 
// http://www.fluffyunderware.com
// =====================================================================

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;
using FluffyUnderware.DevTools;


namespace FluffyUnderware.DevToolsEditor
{
    public abstract class DTProject : IComparable
    {
        public string Identifier { get; private set; }
        public string Version { get; private set; }
        public DTResource Resource { get; protected set; }
        public bool ShowToolbarInAllSceneViews = false;

        public DTToolbarOrientation ToolbarOrientation
        {
            get { return mTBOrientation; }
            set
            {
                if (mTBOrientation != value)
                {
                    mTBOrientation = value;
                    if (!string.IsNullOrEmpty(Identifier))
                    {
                        SetEditorPrefs<DTToolbarOrientation>("ToolbarOrientation", ToolbarOrientation);
                    }
                    else
                        DTLog.LogError("[DevTools] Project " + this.GetType().Name + "missing identifier!");
                    DTToolbar.RecalcItemSize = true;
                }
            }
        }
        public DTToolbarMode ToolbarMode
        {
            get { return mTBMode; }
            set
            {
                if (mTBMode != value)
                {
                    mTBMode = value;
                    if (!string.IsNullOrEmpty(Identifier))
                    {
                        SetEditorPrefs<DTToolbarMode>("ToolbarMode", ToolbarMode);
                    }
                    else
                        DTLog.LogError("[DevTools] Project " + this.GetType().Name + "missing identifier!");
                    DTToolbar.RecalcItemSize = true;
                }
            }
        }

        Dictionary<string, EditorKeyBinding> AdditionalKeyBindings = new Dictionary<string, EditorKeyBinding>();
        internal List<DTToolbarItem> ToolbarItems = new List<DTToolbarItem>();
        
        static DTToolbarOrientation mTBOrientation = DTToolbarOrientation.Left;
        static DTToolbarMode mTBMode = DTToolbarMode.Full;

        protected DTProject(string identifier, string version)
        {
            Identifier = identifier;
            Version = version;
            LoadPreferences();
        }

        internal void Clear()
        {
            ToolbarItems.Clear();
            AdditionalKeyBindings.Clear();
        }

        public virtual void ResetPreferences()
        {
        }
       
        public virtual void LoadPreferences() 
        {
            if (!string.IsNullOrEmpty(Identifier))
            {
                // Upgrade?
                string ver=GetEditorPrefs<string>("Version", Version);
                if (string.Compare(ver, Version)==-1)// (ver != Version)
                {
                    UpgradePreferences(ver);
                    SetEditorPrefs<string>("Version", Version);
                }
                 
                mTBMode=GetEditorPrefs<DTToolbarMode>("ToolbarMode", ToolbarMode);
                mTBOrientation=GetEditorPrefs<DTToolbarOrientation>("ToolbarOrientation", ToolbarOrientation);

            } else
                DTLog.LogError("[DevTools] Project "+this.GetType().Name+"missing identifier!");
        }

        public virtual void SavePreferences()
        {
            if (!string.IsNullOrEmpty(Identifier))
            {
                SetEditorPrefs<string>("Version",Version);
                SetEditorPrefs<DTToolbarMode>("ToolbarMode", ToolbarMode);
                SetEditorPrefs<DTToolbarOrientation>("ToolbarOrientation", ToolbarOrientation);
            } else
                DTLog.LogError("[DevTools] Project " + this.GetType().Name + "missing identifier!");
        }

        public List<EditorKeyBinding> GetProjectBindings()
        {
            var res = new List<EditorKeyBinding>();
            foreach (var item in ToolbarItems)
                foreach (var binding in item.KeyBindings)
                    if (binding.ShowInPrefs)
                        res.Add(binding);
            res.AddRange(AdditionalKeyBindings.Values);
            res.Sort();
            return res;
        }

        public EditorKeyBinding RegisterKeyBinding(EditorKeyBinding binding)
        {
            EditorKeyBinding b;
            if (AdditionalKeyBindings.TryGetValue(binding.Name,out b))
                return b;
            else {
                setKeyBindingFromPrefs(binding,false);
                AdditionalKeyBindings.Add(binding.Name, binding);
                return binding;
            }
        }

        /// <summary>
        ///  For all EditorKeyBindings registered to ToolbarItems of this project, check if Name is unique
        /// </summary>
        /// <returns></returns>
        internal bool CheckKeyBindingNamesAreUnique()
        {
            
            var dict = new Dictionary<string, object>();
            object exist;
             foreach (var item in ToolbarItems)
                foreach (var binding in item.KeyBindings)
                {
                    if (dict.TryGetValue(binding.Name, out exist)){
                        DTLog.LogError(string.Format("[DevTools] KeyBindings need unique names! ({0} and {1}",item.GetType().Name,exist.GetType().Name));
                        return false;
                    } else
                        dict.Add(binding.Name,item);
                }
            
            return true;
        }

        void setKeyBindingFromPrefs(EditorKeyBinding binding, bool removePrefsIfEqual=true)
        {
            string newBindString = GetEditorPrefs<string>(binding.Name, "");
            if (!string.IsNullOrEmpty(newBindString) ){
                // Same binding? Then remove prefs setting
                if (removePrefsIfEqual && newBindString == binding.ToPrefsString())
                    DeleteEditorPrefs(binding.Name);
                else // load and set new binding
                    binding.Set(newBindString);
            }
        }

        /// <summary>
        /// For all EditorKeyBindings registered to ToolbarItems of this project, manage remappings stored in EditorPrefs
        /// </summary>
        internal void LoadKeyBindingRemappings()
        {
            // Toolbars
            foreach (var item in ToolbarItems)
                for (int i=0;i<item.KeyBindings.Count;i++)
                    setKeyBindingFromPrefs(item.KeyBindings[i]);
        }

        protected virtual void UpgradePreferences(string oldVersion)
        {
            DTLog.Log(string.Format("[{0}] Upgrading project settings from {1} to {2}", Identifier, oldVersion, Version));
        }

        public void SetEditorPrefs<T>(string key, T value)
        {
            DT.SetEditorPrefs<T>(Identifier + "." + key, value);
        }

        public T GetEditorPrefs<T>(string key, T defaultValue)
        {
            return DT.GetEditorPrefs<T>(Identifier + "." + key, defaultValue);
        }

        public bool HasEditorPrefs(string key)
        {
            return EditorPrefs.HasKey(Identifier + "." + key);
        }

        public void DeleteEditorPrefs(string key)
        {
            EditorPrefs.DeleteKey(Identifier + "." + key);
        }

        public T FindItem<T>() where T:DTToolbarItem
        {
            var t = typeof(T);
            for (int i = 0; i < ToolbarItems.Count; i++)
                if (ToolbarItems[i].GetType() == t)
                    return (T)ToolbarItems[i];
            return null;
        }

        internal void SetRadioGroupState(DTToolbarRadioButton active)
        {
            for (int i = 0; i < ToolbarItems.Count; i++)
            {
                DTToolbarRadioButton rad = ToolbarItems[i] as DTToolbarRadioButton;
                if (rad && rad.RadioGroup.Equals(active.RadioGroup, StringComparison.CurrentCultureIgnoreCase))
                    rad.mOn = (rad == active);
            }
        }

        public int CompareTo(object obj)
        {
            DTProject other = (DTProject)obj;
            return Identifier.CompareTo(other.Identifier);
        }

       

    }
}
