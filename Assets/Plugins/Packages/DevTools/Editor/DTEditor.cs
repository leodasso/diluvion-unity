// =====================================================================
// Copyright 2013-2017 Fluffy Underware
// All rights reserved
// 
// http://www.fluffyunderware.com
// =====================================================================

using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using FluffyUnderware.DevToolsEditor.Extensions;
using FluffyUnderware.DevTools;
using System;
using UnityEditorInternal;
using UnityEditor.AnimatedValues;
using UnityEngine.Events;

namespace FluffyUnderware.DevToolsEditor
{
    public class DTEditor<T> : Editor where T : UnityEngine.Object
    {
        #region ### Public Properties ###

        /// <summary>
        /// Target Script
        /// </summary>
        public virtual T Target
        {
            get
            {
                return (target!=null) ? target as T : null;
            }
        }

        /// <summary>
        /// Whether the target is currently selected or not
        /// </summary>
        public bool TargetIsActive
        {
            get
            {
                if (target is MonoBehaviour)
                    return (target != null && ((MonoBehaviour)target).transform == Selection.activeTransform) ? true : false;
                else
                    return true;
            }
        }

        /// <summary>
        /// Whether the target is part of a prefab
        /// </summary>
        public bool IsPrefab { get; private set; }

        public bool IsInsideInspector {get;private set;}
        

        /// <summary>
        /// The Root node of all inspector fields
        /// </summary>
        public DTGroupNode Node
        {
            get
            {
                return mRootNode;
            }
        }

        /// <summary>
        /// The renderer used to render inspector fields
        /// </summary>
        public IDTInspectorNodeRenderer NodeRenderer
        {
            get
            {
                return mNodeRenderer;
            }
            set
            {
                mNodeRenderer = value;
            }
        }
        /// <summary>
        /// Whether the inspector needs a repaint
        /// </summary>
        public bool NeedRepaint { get; set; }

        #endregion


        DTGroupNode mRootNode;
        IDTInspectorNodeRenderer mNodeRenderer = new DTInspectorNodeDefaultRenderer();
        bool mEnterChildren;

        #region ### Public Methods ###

        


        #endregion


        #region ### Protected Methods (override to change inspector appearance) ###

        protected virtual void OnEnable()
        {
            if (mRootNode == null)
                mRootNode = new DTGroupNode("Root");
            Undo.undoRedoPerformed -= OnUndoRedo;
            Undo.undoRedoPerformed += OnUndoRedo;
            IsPrefab = (target != null) ? PrefabUtility.GetPrefabType(target) == PrefabType.Prefab : false;
        }

        protected virtual void OnDisable()
        {
            if (mRootNode!=null)
                mRootNode.Clear();
            Undo.undoRedoPerformed -= OnUndoRedo;
        }

        protected virtual void OnReadNodes()
        {
        }

        protected virtual void OnSceneGUI() 
        {
        }

        public override void OnInspectorGUI()
        {
            RenderGUI(false);
        }

        public void RenderGUI(bool embedded=false)
        {
            if (Target == null)
                return;
#if UNITY_5_6_OR_NEWER
            serializedObject.UpdateIfRequiredOrScript();
#else
            serializedObject.UpdateIfDirtyOrScript();
#endif

            if (Node.Count == 0)
                ReadNodes();

            NeedRepaint = false;
            
            if (mRootNode)
            {
                DTInspectorNode.IsInsideInspector = !embedded;
                IsInsideInspector = !embedded;
                OnCustomInspectorGUIBefore();
                renderNode(mRootNode);
                OnCustomInspectorGUI();
            }
            if (serializedObject.ApplyModifiedProperties())
                OnModified();
            
            GUI.SetNextControlName("");
            
            if (NeedRepaint)
                Repaint();
        }

        protected virtual void OnModified()
        {
        }

        // <summary>
        /// Add custom GUI code here, rendered before the default inspector
        /// </summary>
        protected virtual void OnCustomInspectorGUIBefore()
        {
        }

        /// <summary>
        /// Add custom GUI code here, rendered after the default inspector
        /// </summary>
        protected virtual void OnCustomInspectorGUI()
        {
        }

        /// <summary>
        /// Called to initialize a ReorderableList. Override to add custom behaviour
        /// </summary>
        /// <param name="node">field node</param>
        /// <param name="attribute">ArrayEx attribute of the field</param>
        protected virtual void SetupArrayEx(DTFieldNode node, ArrayExAttribute attribute)
        {
            // Defaults
            if (attribute.ShowHeader)
            {
                node.ArrayEx.drawHeaderCallback = (Rect r) =>
                {
                    EditorGUI.LabelField(r, node.GUIContent);
                    if (attribute.DropTarget) {
                        var ev = Event.current;
                        switch (ev.type)
                        {
                            case EventType.DragUpdated:
                                if (r.Contains(ev.mousePosition))
                                {
                                    var fieldType = node.serializedProperty.GetFieldType();
                                    bool allowed = DragAndDrop.objectReferences.Length > 0 && DTEditorUtility.DragDropTypeMatch(fieldType);
                                    DragAndDrop.visualMode = allowed ? DragAndDropVisualMode.Copy : DragAndDropVisualMode.Rejected;
                                }
                                break;
                            case EventType.DragPerform:
                                if (r.Contains(ev.mousePosition))
                                {
                                    var objs = DTEditorUtility.DragDropGetObjectsOfType(node.serializedProperty.GetFieldType());

                                    foreach (var o in objs)
                                    {
                                        int idx = node.serializedProperty.arraySize;
                                        node.serializedProperty.InsertArrayElementAtIndex(idx);
                                        node.serializedProperty.GetArrayElementAtIndex(idx).objectReferenceValue = o;
                                    }
                                    node.serializedObject.ApplyModifiedProperties();
                                }
                                break;
                        }
                    }
                };
            }
            
            node.ArrayEx.drawElementCallback = (Rect r, int index, bool isActive, bool isFocused) =>
            {
                var e = node.ArrayEx.serializedProperty.GetArrayElementAtIndex(index);
                if (e!=null)
                    EditorGUI.PropertyField(r, e);
            };
        }

        /// <summary>
        /// Called when UndoRedo occured
        /// </summary>
        public virtual void OnUndoRedo()
        {
        }

#endregion


        /// <summary>
        /// builds node tree and process parsing attributes
        /// </summary>
        public void ReadNodes()
        {
            DTGroupNode._serializedObject = serializedObject;
            SerializedProperty iterator = serializedObject.GetIterator();
            mRootNode.Clear();
            mEnterChildren = true;

            DTGroupNode baseNode = mRootNode;
            DTGroupNode parentNode = baseNode;
            Stack<string> propertyPathStack = new Stack<string>();
            Stack<DTGroupNode> baseNodeStack = new Stack<DTGroupNode>();
            bool resetParent = false;

            while (iterator.NextVisible(mEnterChildren))
            {
                mEnterChildren = false;
                if (iterator.name != "m_Script" && iterator.name != "InspectorFoldout")
                {
                    // handle baseNode resetting (AsGroup etc...)
                    while (propertyPathStack.Count > 0 && !iterator.propertyPath.StartsWith(propertyPathStack.Peek()))
                    {
                        propertyPathStack.Pop();
                        baseNode = baseNodeStack.Pop();
                        parentNode = baseNode;
                    }


                    var fieldNode = new DTFieldNode(iterator);

                    // get group parsing attributes 
                    var groupParseAttribs = iterator.GetAttributes(typeof(IDTGroupParsingAttribute));
                    groupParseAttribs.Sort();
                    // get field parsing attributes
                    var parsingAttributes = iterator.GetAttributes(typeof(IDTFieldParsingAttribute));

                    foreach (IDTGroupParsingAttribute ga in groupParseAttribs)
                    {

                        if (ga is TabAttribute)
                        {
                            var tabA = (TabAttribute)ga;
                            parentNode = baseNode.EnsurePath(tabA.Path, false);

                            if (!string.IsNullOrEmpty(tabA.TabBarName) && !string.IsNullOrEmpty(tabA.TabName))
                            {
                                if (!parentNode[tabA.TabBarName])
                                    parentNode = (DTGroupNode)parentNode.Add(new DTGroupNode(tabA.TabBarName, null, DTInspectorNode.RenderAsEnum.TabBar));
                                else
                                    parentNode = (DTGroupNode)parentNode[tabA.TabBarName];
                                if (!parentNode[tabA.TabName])
                                    parentNode = (DTGroupNode)parentNode.Add(new DTGroupNode(tabA.TabName, iterator, DTInspectorNode.RenderAsEnum.Tab));
                                else
                                    parentNode = (DTGroupNode)parentNode[tabA.TabName];
                                if (tabA.Sort!=100)
                                    parentNode.SortOrder = tabA.Sort;
                            }
                            else
                                DTLog.LogWarningFormat("[DevTools] Skipping [Tab] on '{0}' because Path is missing TabBar or Tab!", iterator.propertyPath);

                        }
                        else if (ga is SectionAttribute)
                        {
                            var sectionA = (SectionAttribute)ga;
                            parentNode = createGroup(baseNode, sectionA.Path, iterator);
                            if (sectionA.Sort != 100)
                                parentNode.SortOrder = sectionA.Sort;
                        }
                        else if (ga is AsGroupAttribute)
                        {
                            var asGroupA = (AsGroupAttribute)ga;
                            propertyPathStack.Push(fieldNode.SerializedPropertyPath);
                            baseNodeStack.Push(baseNode);
                            parentNode = createGroup((asGroupA.PathIsAbsolute) ? baseNode : parentNode, (asGroupA.Path == null) ? fieldNode.Name : asGroupA.Path + "/" + fieldNode.Name, iterator);
                            baseNode = parentNode;

                        }
                        else if (ga is GroupAttribute)
                        {
                            var groupA = (GroupAttribute)ga;
                            parentNode = createGroup(baseNode, groupA.Path, iterator);
                            if (groupA.Sort != 100)
                                parentNode.SortOrder = groupA.Sort;
                            resetParent = true;
                        }
                    }

                    foreach (IDTFieldParsingAttribute pa in parsingAttributes)
                    {
                        if (pa is Hide)
                        {
                            fieldNode.Visible = false;
                            fieldNode.ContentVisible = false;
                            mEnterChildren = false;
                        }
                        else if (pa is AsGroupAttribute || pa is Inline)
                        {
                            fieldNode.Visible = false;
                            fieldNode.ContentVisible = false;
                            mEnterChildren = true;
                     
                        }
                        else if (pa is ArrayExAttribute)
                        {
                            var arrayA = (ArrayExAttribute)pa;
                            fieldNode.ArrayEx = new ReorderableList(serializedObject, iterator, arrayA.Draggable, arrayA.ShowHeader, arrayA.ShowAdd, arrayA.ShowDelete);
                            SetupArrayEx(fieldNode, arrayA);
                        }
                        else if (pa is SortOrderAttribute)
                        {
                            fieldNode.SortOrder = ((SortOrderAttribute)pa).Sort;
                        }

                    }
                   
                        parentNode.Add(fieldNode);
                    if (resetParent)
                    {
                        parentNode = parentNode.Parent as DTGroupNode;
                        resetParent = false;
                    }
                }
            }
            OnReadNodes();
            Node.Sort();
        }

        /// <summary>
        /// Renders the node tree and process rendering attributes
        /// </summary>
        /// <param name="node"></param>
        void renderNode(DTInspectorNode node)
        {
            if (serializedObject==null)
                return;
            bool guistate = GUI.enabled;
            DTInspectorNode item;
            for (int i = 0; i < node.Items.Count; i++)
            {
                item = node[i];
                item.serializedObject = serializedObject;
                
                if (item.Disabled)
                    GUI.enabled = false;
                if (item is DTFieldNode)
                {
                    
                    DTFieldNode field = (DTFieldNode)item;

                    field.serializedProperty = serializedObject.FindProperty(field.SerializedPropertyPath);
                    if (field.serializedProperty == null)
                        return;
                    field.Calculate();

                    if (field.Visible)
                    {
                        foreach (var act in item.Actions)
                            if (act.Position == ActionAttribute.ActionPositionEnum.Above)
                                NodeRenderer.RenderAction(item, act, this, Target);

                        if (field.ArrayEx != null)
                        {
                            field.ArrayEx.serializedProperty = field.serializedProperty;
                            field.ArrayEx.DoLayoutList();
                        }
                        else
                        {
                            NodeRenderer.RenderField(field);
                            field.raiseOnRender();
                        }

                        foreach (var act in item.Actions)
                            if (act.Position == ActionAttribute.ActionPositionEnum.Below)
                                NodeRenderer.RenderAction(item, act, this, Target);
                    }

                }
                else if (item is DTGroupNode)
                {
                    var group = (DTGroupNode)item;
                    group.Calculate();

                    if (group.Visible)
                    {
                        foreach (var act in item.Actions)
                            if (act.Position == ActionAttribute.ActionPositionEnum.Above)
                                NodeRenderer.RenderAction(item, act, this, Target);
                        
                        if (group.Disabled)
                            GUI.enabled = false;

                        switch (item.RenderAs)
                        {
                            case DTInspectorNode.RenderAsEnum.Section:
                                NodeRenderer.RenderSectionHeader(group);
                                if (group.ContentVisible)
                                {
                                    renderNode(group);
                                    group.raiseOnRender();
                                }
                                NodeRenderer.RenderSectionFooter(group);
                                break;
                            case DTInspectorNode.RenderAsEnum.TabBar:
                                NodeRenderer.RenderTabBarHeader(group, (group.MaxItemsPerRow==-1) ? group.Items.Count:group.MaxItemsPerRow);
                                if (group.SelectedIndex > -1)
                                {
                                    renderNode(group[group.SelectedIndex]);
                                    group[group.SelectedIndex].raiseOnRender();
                                }
                                NodeRenderer.RenderTabBarFooter(group);
                                break;
                            default:
                                if (group.ContentVisible)
                                    renderNode(group);
                                break;
                        }

                        foreach (var act in item.Actions)
                            if (act.Position == ActionAttribute.ActionPositionEnum.Below)
                                NodeRenderer.RenderAction(item, act, this, Target);
                        
                    }

                }
                GUI.enabled = guistate;
                if (item.NeedRepaint)
                {
                    item.NeedRepaint = false;
                    NeedRepaint = true;
                }
            }
        }

        /// <summary>
        /// Creates a parent group for a field, creating the path if neccessary
        /// </summary>
        /// <param name="baseNode">"root" the path is applied to</param>
        /// <param name="path">node path</param>
        /// <param name="forProperty">field property the parent node is for</param>
        /// <returns></returns>
        DTGroupNode createGroup(DTGroupNode baseNode, string path, SerializedProperty forProperty)
        {
            var node = baseNode.EnsurePath(path, false, forProperty);
            return node;
        }

    

    }
    
}
