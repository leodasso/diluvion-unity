// =====================================================================
// Copyright 2013-2017 Fluffy Underware
// All rights reserved
// 
// http://www.fluffyunderware.com
// =====================================================================

using UnityEngine;
using UnityEditor;
using FluffyUnderware.Curvy.Generator;
using FluffyUnderware.DevTools.Extensions;
using FluffyUnderware.DevToolsEditor.Extensions;
using FluffyUnderware.Curvy;
using System.Collections.Generic;
using System;
using FluffyUnderware.DevToolsEditor;
using UnityEditor.AnimatedValues;
using UnityEngine.Events;
using FluffyUnderware.DevTools;

namespace FluffyUnderware.CurvyEditor.Generator
{
    public class CGModuleEditorBase : CurvyEditorBase<CGModule>  
    {
        public CGGraph Graph { get; internal set; }

        public AnimBool ShowDebugStats { get; set; }
        public bool ShowDebugVisuals { get; set; }
        protected bool HasDebugVisuals;

        protected override void OnEnable()
        {
            base.OnEnable();
            ShowDebugStats = new AnimBool(true);
            ShowDebugStats.speed = 3;
            HasDebugVisuals = false;
            EndIPE();
        }

        /// <summary>
        /// Called by the graph when an IPE session starts
        /// </summary>
        internal virtual void BeginIPE() 
        {
        }
        
        /// <summary>
        /// Called by the graph when an IPE session ends
        /// </summary>
        internal virtual void EndIPE() 
        {
        }

        /// <summary>
        /// Called for the IPE Target when the module should TRS it's IPE editor to the given values
        /// </summary>
        internal virtual void OnIPESetTRS(Vector3 position, Quaternion rotation, Vector3 scale) { }

        /// <summary>
        /// Called for the IPE initiator to get the TRS values for the target
        /// </summary>
        internal virtual void OnIPEGetTRS(out Vector3 position, out Quaternion rotation, out Vector3 scale) 
        {
            position = Vector3.zero;
            rotation = Quaternion.identity;
            scale = Vector3.one;
        }
        

        /// <summary>
        /// Scene View GUI
        /// </summary>
        /// <remarks>Called only if the module is initialized and configured</remarks>
        public virtual void OnModuleSceneGUI()
        {
        }

        /// <summary>
        /// Scene View Debug GUI
        /// </summary>
        /// <remarks>Called only when Show Debug Visuals is activated</remarks>
        public virtual void OnModuleSceneDebugGUI()
        {
        }
        /// <summary>
        /// Inspector Debug GUI
        /// </summary>
        /// <remarks>Called only when Show Debug Values is activated </remarks>
        public virtual void OnModuleDebugGUI()
        {
        }

        protected override void OnCustomInspectorGUI()
        {
            base.OnCustomInspectorGUI();
            for (int m = 0; m < Target.UIMessages.Count; m++)
                EditorGUILayout.HelpBox(Target.UIMessages[m], MessageType.Warning);
        }

        protected override void OnReadNodes()
        {
            base.OnReadNodes();
        }
        
        public override void OnInspectorGUI()
        {
            if (!IsPrefab && Target)
            {
                if (!Target.IsInitialized)
                {
                    Target.Initialize();
                }
                DTGroupNode slotSection;
                
                if (DTGUI.IsLayout && IsInsideInspector && Target.Input!=null && Target.Output!=null && (Target.Input.Count > 0 || Target.Output.Count > 0) && !Node.FindNode<DTGroupNode>("Slots", out slotSection))
                {
                    Node.AddSection("Slots", OnShowSlots).SortOrder = 99999;
                    Node.Sort();
                }
                
                if (GUILayout.Button(new GUIContent(CurvyStyles.OpenGraphTexture, "Edit Graph")) && Target.Generator)
                {
                        var win = CGGraph.Open(Target.Generator);
                        win.Sel.Select(Target);
                        win.Canvas.FocusSelection();
                }
                
                base.OnInspectorGUI();

               
                
            }
        }

        void OnShowSlots(DTInspectorNode node)
        {
            if (Target)
            {
                var inSlots = Target.Input;
                var outSlots = Target.Output;
                
                EditorGUILayout.BeginVertical(GUI.skin.box);

                if (inSlots.Count > 0)
                {
                    EditorGUILayout.LabelField("Input", EditorStyles.boldLabel);
                    showSlots<CGModuleInputSlot>(inSlots);
                }

                if (outSlots.Count > 0)
                {
                    EditorGUILayout.LabelField("Output", EditorStyles.boldLabel);
                    showSlots<CGModuleOutputSlot>(outSlots);
                }
                EditorGUILayout.EndVertical();
                GUILayout.Space(10);
                
            }
        }

        void showSlots<T>(List<T> slots) where T:CGModuleSlot
        {
            foreach (CGModuleSlot slot in slots)
            {
                var linked = slot.GetLinkedModules();
                if (linked.Count > 1)
                {
                    for (int i=0;i<linked.Count;i++)
                    {
                        var sel = EditorGUILayout.ObjectField((i==0)?slot.Info.Name:" ", linked[i], typeof(CGModule), true);
                        if (sel!=linked[i])
                            DTLog.Log("[Curvy] Linking modules from the inspector isn't supported yet! Use the Graph editor instead!");
                    }
                }
                else
                {
                    var lm = (linked.Count == 0) ? null : linked[0];
                    var sel=EditorGUILayout.ObjectField(slot.Info.Name, lm, typeof(CGModule), true);
                    if (sel != lm)
                        DTLog.Log("[Curvy] Linking modules from the inspector isn't supported yet! Use the Graph editor instead!");
                }
            }
        }

        
        public void OnInspectorDebugGUIINTERNAL(UnityAction onChange)
        {
            if (Target)
            {
                ShowDebugStats.valueChanged.RemoveListener(onChange);
                ShowDebugStats.valueChanged.AddListener(onChange);
                GUILayout.BeginHorizontal(CurvyStyles.Toolbar);
                
                ShowDebugStats.target=GUILayout.Toggle(ShowDebugStats.target,new GUIContent(CurvyStyles.DebugTexture,"Show Values"),EditorStyles.toolbarButton);
                
                if (HasDebugVisuals)
                {
                    EditorGUI.BeginChangeCheck();
                    ShowDebugVisuals = GUILayout.Toggle(ShowDebugVisuals, new GUIContent(CurvyStyles.DebugSceneViewTexture, "Visualize"), EditorStyles.toolbarButton);
                    if (EditorGUI.EndChangeCheck())
                        SceneView.RepaintAll();
                }
                else
                    ShowDebugVisuals = false;
                GUILayout.FlexibleSpace();
                GUILayout.Label(string.Format("{0:0.###} ms", Target.DEBUG_ExecutionTime.AverageMS));
                GUILayout.Label(string.Format("{0:0} %", Target.DEBUG_ExecutionTime.AverageMS/Target.Generator.DEBUG_ExecutionTime.AverageMS*100));
                GUILayout.EndHorizontal();
                GUILayout.Space(2);
                
                if (EditorGUILayout.BeginFadeGroup(ShowDebugStats.faded))
                {
                    OnModuleDebugGUI();
                }
                
            }
        }

        

        void CBSeedOptions()
        {
            if (!Target.RandomizeSeed)
            {
                var r = EditorGUILayout.GetControlRect(true, 16, EditorStyles.numberField, null);
                r.width-=16;
                EditorGUI.PropertyField(r,serializedObject.FindProperty("m_Seed"));
                r.x += r.width;
                r.width = 16;
                if (GUI.Button(r,new GUIContent(CurvyStyles.RndSeedTexture,"Randomize now!"),CurvyStyles.ImageButton))
                {
                    Target.Seed = (int)System.DateTime.Now.Ticks;
                    Target.Generator.Refresh();
                }
            }
        }


    }

    public class CGModuleEditor<T> : CGModuleEditorBase where T : CGModule
    {
        public new T Target
        {
            get
            {
                return target as T;
            }
        }

       
    }
}
