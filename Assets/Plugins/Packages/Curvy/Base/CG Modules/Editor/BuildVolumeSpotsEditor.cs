// =====================================================================
// Copyright 2013-2017 Fluffy Underware
// All rights reserved
// 
// http://www.fluffyunderware.com
// =====================================================================

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using FluffyUnderware.Curvy.Generator;
using FluffyUnderware.Curvy.Generator.Modules;
using FluffyUnderware.DevToolsEditor;
using FluffyUnderware.DevTools.Extensions;
using FluffyUnderware.DevToolsEditor.Extensions;
using UnityEditorInternal;
using FluffyUnderware.Curvy;

namespace FluffyUnderware.CurvyEditor.Generator.Modules
{
    [CustomEditor(typeof(BuildVolumeSpots))]
    public class BuildVolumeSpotsEditor : CGModuleEditor<BuildVolumeSpots>
    {
        ReorderableList mGroupItemsList;
        CGBoundsGroup mCurrentGroup;


        protected override void OnEnable()
        {
            base.OnEnable();
            HasDebugVisuals = true;
        }

        

        public override void OnModuleSceneDebugGUI()
        {
            var data = Target.SimulatedSpots;
            if (data)
            {
                Handles.matrix = Target.Generator.transform.localToWorldMatrix;
                DTGUI.PushColor(Color.red);
                for (int i=0;i<data.Points.Length;i++)
                {
                    var Q=data.Points[i].Rotation*Quaternion.Euler(-90,0,0);
#if UNITY_5_6_OR_NEWER
                    Handles.ArrowHandleCap(0, data.Points[i].Position, Q, 2,EventType.Repaint);
#else
                    Handles.ArrowCap(0, data.Points[i].Position, Q, 2);
#endif

                    Handles.Label(data.Points[i].Position, data.Points[i].Index.ToString(),EditorStyles.boldLabel);
                }
                DTGUI.PopColor();
                Handles.matrix = Matrix4x4.identity;
            }
        }

        protected override void OnReadNodes()
        {
            base.OnReadNodes();
            ensureGroupTabs();
        }   

        void ensureGroupTabs()
        {
            var tabbar = Node.FindTabBarAt("Default");
            for (int i = 0; i < Target.GroupCount; i++)
            {
                var tabName = string.Format("{0}:{1}", i,Target.Groups[i].Name);
                if (tabbar.Count <= i + 2)
                    tabbar.AddTab(tabName, OnRenderTab);
                else
                {
                    tabbar[i + 2].Name = tabName;
                    tabbar[i + 2].GUIContent.text = tabName;
                }
            }
            for (int i = tabbar.Count-1; i > Target.GroupCount+1; i--)
                tabbar[i].Delete();
            

        }

        void OnRenderTab(DTInspectorNode node)
        {
            int grpIdx = node.Index - 2;
            
            if (grpIdx >= 0 && grpIdx < Target.GroupCount)
            {
                
                var pGroup = serializedObject.FindProperty(string.Format("m_Groups.Array.data[{0}]", grpIdx));
                if (pGroup != null)
                {
                    
                    var group = Target.Groups[grpIdx];
                    var pItems = pGroup.FindPropertyRelative("m_Items");
                    if (pItems != null)
                    {
                        
                        if (mCurrentGroup != null && mCurrentGroup != group)
                            mGroupItemsList = null;
                        if (mGroupItemsList == null)
                        {
                            mCurrentGroup = group;
                            mGroupItemsList = new ReorderableList(pItems.serializedObject,pItems);
                            mGroupItemsList.draggable = true;
                            mGroupItemsList.drawHeaderCallback = (Rect Rect) => { EditorGUI.LabelField(Rect, "Items"); };
                            mGroupItemsList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
                            {
#region ---
                                bool fix = (index < group.FirstRepeating || index > group.LastRepeating);

                                if (fix)
                                    DTHandles.DrawSolidRectangleWithOutline(rect.ShiftBy(0, -1), new Color(0, 0, 0.5f, 0.2f), new Color(0, 0, 0, 0));

                                var prop = pItems.FindPropertyRelative(string.Format("Array.data[{0}]", index));
                                var pIndex = prop.FindPropertyRelative("Index");

                                rect.height = EditorGUIUtility.singleLineHeight;
                                var r = new Rect(rect);
                                EditorGUI.LabelField(new Rect(rect.x, rect.y, 30, rect.height), "#" + index.ToString() + ":");
                                r.x += 30;
                                r.width = rect.width / 2 - 50;
                                var bn = Target.BoundsNames;
                                var bi = Target.BoundsIndices;
                                if (bn.Length == 0)
                                    pIndex.intValue = EditorGUI.IntField(r, "", pIndex.intValue);
                                else
                                    EditorGUI.IntPopup(r, pIndex, bn, bi, new GUIContent(""));

                                r.x += r.width + 10;
                                r.width = rect.width / 2;
                                if (!fix && group.RepeatingOrder == CurvyRepeatingOrderEnum.Random)
                                {
                                    EditorGUI.PropertyField(r, prop.FindPropertyRelative("m_Weight"), new GUIContent(""));
                                }
#endregion
                            };

                            mGroupItemsList.onAddCallback = (ReorderableList l) =>
                            {
                                group.Items.Insert(Mathf.Clamp(l.index + 1, 0, group.ItemCount), new CGBoundsGroupItem());
                                group.LastRepeating++;
                                Target.Dirty = true;
                                EditorUtility.SetDirty(Target);
                            };
                            mGroupItemsList.onRemoveCallback = (ReorderableList l) =>
                            {
                                group.Items.RemoveAt(l.index);
                                group.LastRepeating--;
                                Target.Dirty = true;
                                EditorUtility.SetDirty(Target);
                            };
                        }

                        mGroupItemsList.DoLayoutList();

                        EditorGUILayout.PropertyField(pGroup.FindPropertyRelative("m_RepeatingItems"));
                        EditorGUILayout.PropertyField(pGroup.FindPropertyRelative("m_RepeatingOrder"));
                        EditorGUILayout.PropertyField(pGroup.FindPropertyRelative("m_KeepTogether"));
                        EditorGUILayout.PropertyField(pGroup.FindPropertyRelative("m_SpaceBefore"));
                        EditorGUILayout.PropertyField(pGroup.FindPropertyRelative("m_SpaceAfter"));
                        EditorGUILayout.PropertyField(pGroup.FindPropertyRelative("m_DistributionMode"), new GUIContent("Mode"));
                        EditorGUILayout.PropertyField(pGroup.FindPropertyRelative("m_PositionOffset"));
                        EditorGUILayout.PropertyField(pGroup.FindPropertyRelative("m_Height"));
                        EditorGUILayout.PropertyField(pGroup.FindPropertyRelative("m_RotationMode"));
                        EditorGUILayout.PropertyField(pGroup.FindPropertyRelative("m_RotationOffset"));
                        EditorGUILayout.PropertyField(pGroup.FindPropertyRelative("m_RotationScatter"));
                    }
                }

            } 
              
        }

        protected override void SetupArrayEx(DTFieldNode node, DevTools.ArrayExAttribute attribute)
        {
            switch (node.Name)
            {
                case "m_Groups":
                    node.ArrayEx.drawHeaderCallback = (Rect Rect) => { EditorGUI.LabelField(Rect, "Groups"); };
                    node.ArrayEx.drawElementCallback = OnGroupElementGUI;
                    node.ArrayEx.onAddCallback = (ReorderableList l) =>
                    {
                        Target.Groups.Insert(Mathf.Clamp(l.index + 1, 0, Target.GroupCount), new CGBoundsGroup("Group"));
                        Target.LastRepeating++;
                        EditorUtility.SetDirty(Target);
                        ensureGroupTabs();
                    };
                    node.ArrayEx.onRemoveCallback = (ReorderableList l) =>
                    {
                        mGroupItemsList = null;
                        Target.Groups.RemoveAt(l.index);
                        Target.LastRepeating--;
                        EditorUtility.SetDirty(Target);
                        
                        //node[1+l.index].Delete();
                        ensureGroupTabs();
                        GUIUtility.ExitGUI();
                    };
                    node.ArrayEx.onReorderCallback = (ReorderableList l) =>
                    {
                        ensureGroupTabs();
                        GUIUtility.ExitGUI();
                    };
                    break;
            }
        }

        void OnGroupElementGUI(Rect rect, int index, bool isActive, bool isFocused)
        {
            bool fix=(index < Target.FirstRepeating || index>Target.LastRepeating);

            if (fix)
                DTHandles.DrawSolidRectangleWithOutline(rect.ShiftBy(0,-1), new Color(0, 0, 0.5f, 0.2f), new Color(0,0,0,0));

            var prop = serializedObject.FindProperty(string.Format("m_Groups.Array.data[{0}]", index));
            if (prop != null)
            {
                var pName = prop.FindPropertyRelative("m_Name");
                var pRepeatingOrder = serializedObject.FindProperty("m_RepeatingOrder");
                rect.height = EditorGUIUtility.singleLineHeight;
                var r = new Rect(rect);
                EditorGUI.LabelField(new Rect(rect.x, rect.y, 30, rect.height), "#" + index.ToString() + ":");
                r.x += 30;
                r.width = rect.width / 2 - 50;
                EditorGUI.BeginChangeCheck();
                pName.stringValue = EditorGUI.TextField(r, "", pName.stringValue);
                if (EditorGUI.EndChangeCheck())
                {
                    var tab = Node.FindTabBarAt("Default");
                    if (tab && tab.Count > index + 2)
                    {
                        tab[index + 2].Name = string.Format("{0}:{1}", index, pName.stringValue);
                        tab[index + 2].GUIContent.text = string.Format("{0}:{1}", index, pName.stringValue);
                    }
                }



                r.x += r.width + 10;
                r.width = rect.width / 2;
                if (!fix && pRepeatingOrder.intValue == (int)CurvyRepeatingOrderEnum.Random)
                {
                    EditorGUI.PropertyField(r, prop.FindPropertyRelative("m_Weight"), new GUIContent(""));
                }
            }
        }

        protected override void OnCustomInspectorGUIBefore()
        {
            base.OnCustomInspectorGUIBefore();
            EditorGUILayout.HelpBox("Spots: " + Target.Count, MessageType.Info);
        }
        

    }
   
}
