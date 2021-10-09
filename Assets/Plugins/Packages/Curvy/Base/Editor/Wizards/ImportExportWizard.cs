// =====================================================================
// Copyright 2013-2017 Fluffy Underware
// All rights reserved
// 
// http://www.fluffyunderware.com
// =====================================================================

using UnityEngine;
using System.Collections;
using UnityEditor;
using FluffyUnderware.DevToolsEditor;
using FluffyUnderware.Curvy;
using FluffyUnderware.Curvy.Utils;
using FluffyUnderware.DevTools.Extensions;
using System.Linq;
using FluffyUnderware.DevTools;
using UnityEngine.Events;
using FluffyUnderware.Curvy.Components;

namespace FluffyUnderware.CurvyEditor
{

    public class ImportExportWizard : EditorWindow
    {
        CurvyImportExport Cmp;

        public static void Open()
        {
            var win = EditorWindow.GetWindow<ImportExportWizard>(true, "Import/Export");
            win.minSize = new Vector2(500, 340);
        }

        void OnEnable()
        {
            DTSelection.OnSelectionChange += DTSelection_OnSelectionChange;
            Cmp = new GameObject("ImportExport", typeof(CurvyImportExport)).GetComponent<CurvyImportExport>();
            Cmp.hideFlags = HideFlags.DontSave;
        }

        void OnDisable()
        {
            DTSelection.OnSelectionChange -= DTSelection_OnSelectionChange;
            if (Cmp)
                DestroyImmediate(Cmp.gameObject);
        }

        void DTSelection_OnSelectionChange()
        {
            Repaint();
        }

        /*
        public bool UseFile { get { return mExchangeTo == 0; } }
        public bool UseClipboard { get { return mExchangeTo == 1; } }
        
        string txtSerialized = string.Empty;
        string txtSerializedType = string.Empty;
        string txtFile = string.Empty;
        
        CurvySerializationSpace Space = CurvySerializationSpace.WorldSpline;
        
        Vector2 scroll;
        bool mNeedRepaint;
        bool mFormat = true;
        int mExchangeTo;

        IDTInspectorNodeRenderer GUIRenderer = new DTInspectorNodeDefaultRenderer();
        DTGroupNode nFile = new DTGroupNode("Exchange to");
        DTGroupNode nActions = new DTGroupNode("Actions");
        DTGroupNode nLog = new DTGroupNode("Log");


        bool SplinesSelected
        {
            get { return DTSelection.HasComponent<CurvySpline>(true); }
        }

        bool ControlPointsSelected
        {
            get { return DTSelection.HasComponent<CurvySplineSegment>(true); }
        }

        bool canDeserialize
        {
            get
            {
                switch (txtSerializedType)
                {
                    case "SerializedCurvySplineCollection":
                        return !DTSelection.HasComponent<CurvySplineSegment>(true);
                    case "SerializedCurvySplineSegmentCollection":
                        return DTSelection.HasComponent<CurvySpline>() || DTSelection.HasComponent<CurvySplineSegment>();
                    default:
                        return false;
                }
            }
        }

        

        void OnGUI()
        {
            DTInspectorNode.IsInsideInspector = false;
            // Exchange to
            GUIRenderer.RenderSectionHeader(nFile);
            if (nFile.ContentVisible)
            {
                mExchangeTo = GUILayout.SelectionGrid(mExchangeTo, new string[] { "File", "Clipboard" }, 2);
                GUI.enabled = UseFile;
                GUILayout.BeginHorizontal();
                txtFile = EditorGUILayout.TextField(new GUIContent("File"),txtFile);
                GUI.enabled = true;
                if (GUILayout.Button(new GUIContent("...", "Select file"), GUILayout.ExpandWidth(false)))
                {
                    txtFile = EditorUtility.SaveFilePanel("Choose or create file", Application.dataPath, "", "");
                        //EditorUtility.LoadFilePanel("Select file", Application.dataPath, "");
                }
                GUILayout.EndHorizontal();
                GUI.enabled = true;
            }
            GUIRenderer.RenderSectionFooter(nFile);
            mNeedRepaint = mNeedRepaint || nFile.NeedRepaint;
            // Actions
            GUIRenderer.RenderSectionHeader(nActions);
            if (nActions.ContentVisible)
            {
                Space = (CurvySerializationSpace)EditorGUILayout.EnumPopup("Space", Space, GUILayout.Width(250));
                GUILayout.BeginHorizontal();
                GUI.enabled = System.IO.File.Exists(txtFile);
                if (GUILayout.Button("Load"))
                {
                    txtSerialized = System.IO.File.ReadAllText(txtFile).Replace("\n", "");
                    if (canDeserialize)
                        deserialize();
                    else
                        DTLog.LogWarning("[Curvy] Import/Export: File contains no compatible data!");
                }
                GUI.enabled = (ControlPointsSelected || SplinesSelected) && (!string.IsNullOrEmpty(txtFile) || UseClipboard);
                if (GUILayout.Button("Save"))
                {
                    serialize();
                    System.IO.File.WriteAllText(txtFile, txtSerialized);
                    AssetDatabase.Refresh();
                }
                GUI.enabled = true;
                GUILayout.EndHorizontal();

                
            }
            GUIRenderer.RenderSectionFooter(nActions);
            mNeedRepaint = mNeedRepaint || nActions.NeedRepaint;

            // Log
            GUIRenderer.RenderSectionHeader(nLog);
            if (nLog.ContentVisible)
            {
            }
            GUIRenderer.RenderSectionFooter(nLog);

                GUILayout.Space(5);
            mNeedRepaint = mNeedRepaint || nLog.NeedRepaint;
            if (mNeedRepaint)
            {
                Repaint();
                mNeedRepaint = false;
            }

            
        }

        void serialize()
        {
            if (ControlPointsSelected)
            {
                txtSerialized = new SerializedCurvySplineSegmentCollection(DTSelection.GetAllAs<CurvySplineSegment>(),Space).ToJson();
            }
            
            else if (SplinesSelected)
            {
                txtSerialized = new SerializedCurvySplineCollection(DTSelection.GetAllAs<CurvySpline>(), Space).ToJson();
            }
        }

        void deserialize()
        {
            switch (txtSerializedType)
            {
                case "SerializedCurvySplineCollection":
                    var sspl = SerializedCurvySplineCollection.FromJson(txtSerialized);
                    var applyTo = DTSelection.GetAllAs<CurvySpline>().ToArray();
                    if (applyTo.Length>0) 
                        sspl.Deserialize(applyTo, Space);
                    else
                        sspl.Deserialize(DTSelection.GetAs<Transform>(), Space);
                    break;
                case "SerializedCurvySplineSegmentCollection":
                    var scp = SerializedCurvySplineSegmentCollection.FromJson(txtSerialized);
                    CurvySplineSegment cp = DTSelection.GetAs<CurvySplineSegment>();
                    if (cp)
                        scp.Deserialize(cp, Space,cpHandler);
                     else
                    {
                        CurvySpline spl = DTSelection.GetAs<CurvySpline>();
                        scp.Deserialize(spl, Space,cpHandler);
                    }
                    break;
            }
        }

        
        void getType()
        {
            System.Type t = SerializedCurvyObjectHelper.GetJsonSerializedType(txtSerialized.Substring(0));
            txtSerializedType = (t != null) ? t.Name : "Unsupported type";
        }

        */
    }


}

        

   
