// =====================================================================
// Copyright 2013-2017 Fluffy Underware
// All rights reserved
// 
// http://www.fluffyunderware.com
// =====================================================================

using UnityEngine;
using UnityEditor;
using FluffyUnderware.Curvy;
using FluffyUnderware.DevTools;
using FluffyUnderware.DevTools.Extensions;
using FluffyUnderware.DevToolsEditor;
using System.Collections.Generic;

namespace FluffyUnderware.CurvyEditor
{
    public class CurvyProject : DTProject
    {
        public const string NAME = "Curvy";
        public const string RELPATH_SHAPEWIZARDSCRIPTS = "/Shapes";
        public const string RELPATH_CGMODULEWIZARDSCRIPTS = "/Generator Modules";
        public const string RELPATH_CGMODULEWIZARDEDITORSCRIPTS = "/Generator Modules/Editor";
        public const string RELPATH_CGTEMPLATES = "/Generator Templates";

        public static CurvyProject Instance
        {
            get
            {
                return (CurvyProject)DT.Project(NAME);
            }
        }

        #region ### Persistent Settings ###

        // Settings from Preferences window not stored in CurvyGlobalManager
        
        public bool SnapValuePrecision = true;
        public bool UseTiny2DHandles = false;
        public bool ShowGlobalToolbar = true;

        // Settings made in the toolbar or somewhere else

        bool mCGAutoModuleDetails=false;
        public bool CGAutoModuleDetails
        {
            get { return mCGAutoModuleDetails; }
            set
            {
                if (mCGAutoModuleDetails != value)
                {
                    mCGAutoModuleDetails = value;
                    SetEditorPrefs<bool>("CGAutoModuleDetails", mCGAutoModuleDetails);
                }
            }
        }

        bool mCGSynchronizeSelection = true;
        public bool CGSynchronizeSelection
        {
            get { return mCGSynchronizeSelection; }
            set
            {
                if (mCGSynchronizeSelection != value)
                {
                    mCGSynchronizeSelection = value;
                    SetEditorPrefs<bool>("CGSynchronizeSelection", mCGSynchronizeSelection);
                }
            }
        }

        bool mCGShowHelp = true;
        public bool CGShowHelp
        {
            get { return mCGShowHelp; }
            set
            {
                if (mCGShowHelp != value)
                {
                    mCGShowHelp = value;
                    SetEditorPrefs<bool>("CGShowHelp", mCGShowHelp);
                }
            }
        }

        int mCGGraphSnapping = 5;
        public int CGGraphSnapping
        {
            get { return mCGGraphSnapping; }
            set
            {
                int v = Mathf.Max(1, value);
                if (mCGGraphSnapping != v)
                {
                    mCGGraphSnapping = v;
                    SetEditorPrefs<int>("CGGraphSnapping", mCGGraphSnapping);
                }
            }
        }

        string mCustomizationRootPath = "Packages/Curvy Customization";
        public string CustomizationRootPath
        {
            get
            {
                return mCustomizationRootPath;
            }
            set
            {
                if (mCustomizationRootPath != value)
                {
                    mCustomizationRootPath = value;
                    SetEditorPrefs<string>("CustomizationRootPath", mCustomizationRootPath);
                }
            }
        }

        CurvyBezierModeEnum mBezierMode = CurvyBezierModeEnum.Direction | CurvyBezierModeEnum.Length;
        public CurvyBezierModeEnum BezierMode
        {
            get { return mBezierMode; }
            set
            {
                if (mBezierMode != value)
                {
                    mBezierMode = value;
                    SetEditorPrefs<CurvyBezierModeEnum>("BezierMode", mBezierMode);
                }
            }
        }

        CurvyAdvBezierModeEnum mAdvBezierMode = CurvyAdvBezierModeEnum.Direction | CurvyAdvBezierModeEnum.Length;
        public CurvyAdvBezierModeEnum AdvBezierMode
        {
            get { return mAdvBezierMode; }
            set
            {
                if (mAdvBezierMode != value)
                {
                    mAdvBezierMode = value;
                    SetEditorPrefs<CurvyAdvBezierModeEnum>("AdvBezierMode", mAdvBezierMode);
                }
            }
        }

        bool mShowAboutOnLoad=true;
        public bool ShowAboutOnLoad
        {
            get
            {
                return mShowAboutOnLoad;
            }
            set
            {
                if (mShowAboutOnLoad != value)
                    mShowAboutOnLoad = value;
                SetEditorPrefs<bool>("ShowAboutOnLoad", mShowAboutOnLoad);
            }
        }

        #endregion

      

        

        static Vector2 scroll;
        static bool[] foldouts = new bool[4] { true, true, true, true };

        

        List<int> mShowConIconObjects = new List<int>();


        public CurvyProject()
            : base(NAME, CurvySpline.VERSION)
        {
            Resource = CurvyResource.Instance;
            Undo.undoRedoPerformed -= OnUndoRedo;
            Undo.undoRedoPerformed += OnUndoRedo;
            EditorApplication.update -= OnUpdate;
            EditorApplication.update += OnUpdate;
            EditorApplication.update += checkLaunch;
            EditorApplication.hierarchyWindowChanged -= ScanConnections;
            EditorApplication.hierarchyWindowChanged += ScanConnections;
            EditorApplication.hierarchyWindowItemOnGUI -= OnHierarchyWindowItemOnGUI;
            EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyWindowItemOnGUI;
            ScanConnections();
        }

        /// <summary>
        /// Rebuilds the list of GameObject that needs to show a connection icon in the hierarchy window
        /// </summary>
        /// <remarks>Usually there is no need to call this manually</remarks>
        public void ScanConnections() 
        {
            int old = mShowConIconObjects.Count;
            mShowConIconObjects.Clear();

            var o = GameObject.FindObjectsOfType<CurvyConnection>();
            foreach (var con in o)
            {
                foreach (var cp in con.ControlPoints)
                {
                    if (cp != null)
                        mShowConIconObjects.Add(cp.gameObject.GetInstanceID());
                }
            }

            if (old != mShowConIconObjects.Count)
                EditorApplication.RepaintHierarchyWindow();
        }

        void OnHierarchyWindowItemOnGUI(int instanceid, Rect selectionrect)
        {
            if (mShowConIconObjects.Contains(instanceid))
            {
                GUI.DrawTexture(new Rect(selectionrect.xMax - 14, selectionrect.yMin + 4, 10, 10), CurvyStyles.HierarchyConnectionTexture);
            }
        }

        void checkLaunch() 
        {
            EditorApplication.update -= checkLaunch;
            if (ShowAboutOnLoad)
                AboutWindow.Open();
        }
        
        void OnUpdate()
        {
            
            // check if a deleted Curvy object defines a new object to select
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                CurvySpline._newSelectionInstanceIDINTERNAL = 0;
            
            if (CurvySpline._newSelectionInstanceIDINTERNAL != 0)
            {
                var o=EditorUtility.InstanceIDToObject(CurvySpline._newSelectionInstanceIDINTERNAL);
                if (o!=null && o is Component)
                    DTSelection.SetGameObjects((Component)o);
                CurvySpline._newSelectionInstanceIDINTERNAL = 0;
            }
        }

        void OnUndoRedo()
        {
            var splines = DTSelection.GetAllAs<CurvySpline>();
            var cps = DTSelection.GetAllAs<CurvySplineSegment>();
            foreach (var cp in cps)
                if (!splines.Contains(cp.Spline))
                    splines.Add(cp.Spline);

            foreach (var spl in splines)
            {
                spl.SyncSplineFromHierarchy();
                spl.SetDirtyAll();
                spl.Refresh();
            }
           
        }

        

        public override void ResetPreferences()
        {
            base.ResetPreferences();
            CurvyGlobalManager.DefaultInterpolation = CurvyInterpolation.CatmullRom;
            CurvyGlobalManager.DefaultGizmoColor = new Color(0.71f,0.71f,0.71f);
            CurvyGlobalManager.DefaultGizmoSelectionColor = new Color(0.15f, 0.35f, 0.68f);
            CurvyGlobalManager.GizmoControlPointSize = 0.15f;
            CurvyGlobalManager.GizmoOrientationLength = 1f;
            CurvyGlobalManager.GizmoOrientationColor = new Color(0.75f, 0.75f, 0.4f);
            CurvyGlobalManager.MaxCachePPU = 8;
            CurvyGlobalManager.SceneViewResolution = 0.5f;
            CurvyGlobalManager.SplineLayer = 0;
            CustomizationRootPath = "Packages/Curvy Customization";
            SnapValuePrecision = true;
            UseTiny2DHandles = true;
            ShowGlobalToolbar = true;

            CurvyGlobalManager.SaveRuntimeSettings();
        }

        public override void LoadPreferences()
        {
            if (GetEditorPrefs<string>("Version", "PreDT") == "PreDT")
            {
                DeletePreDTSettings();
                SavePreferences();
            }
            base.LoadPreferences();
            CurvyGlobalManager.DefaultInterpolation = GetEditorPrefs<CurvyInterpolation>("DefaultInterpolation", CurvyGlobalManager.DefaultInterpolation);
            CurvyGlobalManager.DefaultGizmoColor = GetEditorPrefs<Color>("GizmoColor", CurvyGlobalManager.DefaultGizmoColor);
            CurvyGlobalManager.DefaultGizmoSelectionColor = GetEditorPrefs<Color>("GizmoSelectionColor", CurvyGlobalManager.DefaultGizmoSelectionColor);
            CurvyGlobalManager.GizmoControlPointSize=GetEditorPrefs<float>("GizmoControlPointSize", CurvyGlobalManager.GizmoControlPointSize);
            CurvyGlobalManager.GizmoOrientationLength=GetEditorPrefs<float>("GizmoOrientationLength", CurvyGlobalManager.GizmoOrientationLength);
            CurvyGlobalManager.GizmoOrientationColor = GetEditorPrefs<Color>("GizmoOrientationColor", CurvyGlobalManager.GizmoOrientationColor);
            CurvyGlobalManager.Gizmos = GetEditorPrefs<CurvySplineGizmos>("Gizmos", CurvyGlobalManager.Gizmos);
            SnapValuePrecision = GetEditorPrefs<bool>("SnapValuePrecision", true);
            CurvyGlobalManager.MaxCachePPU = Mathf.Max(1, GetEditorPrefs<int>("MaxCachePPU", CurvyGlobalManager.MaxCachePPU));
            CurvyGlobalManager.SceneViewResolution = Mathf.Clamp01(GetEditorPrefs<float>("SceneViewResolution", CurvyGlobalManager.SceneViewResolution));
            CurvyGlobalManager.HideManager = GetEditorPrefs<bool>("HideManager", CurvyGlobalManager.HideManager);
            UseTiny2DHandles = GetEditorPrefs<bool>("UseTiny2DHandles", UseTiny2DHandles);
            ShowGlobalToolbar = GetEditorPrefs<bool>("ShowGlobalToolbar", ShowGlobalToolbar);
            CurvyGlobalManager.SplineLayer = GetEditorPrefs<int>("SplineLayer", CurvyGlobalManager.SplineLayer);
            CurvyGlobalManager.SaveRuntimeSettings();

            mCGAutoModuleDetails = GetEditorPrefs<bool>("CGAutoModuleDetails", mCGAutoModuleDetails);
            mCGSynchronizeSelection = GetEditorPrefs<bool>("CGSynchronizeSelection", mCGSynchronizeSelection);
            mCGShowHelp = GetEditorPrefs<bool>("CGShowHelp", mCGShowHelp);
            mCGGraphSnapping = GetEditorPrefs<int>("CGGraphSnapping", mCGGraphSnapping);
            mBezierMode=GetEditorPrefs<CurvyBezierModeEnum>("BezierMode", mBezierMode);
            mAdvBezierMode = GetEditorPrefs<CurvyAdvBezierModeEnum>("AdvBezierMode", mAdvBezierMode);
            mCustomizationRootPath = GetEditorPrefs<string>("CustomizationRootPath", mCustomizationRootPath);
            mShowAboutOnLoad=GetEditorPrefs<bool>("ShowAboutOnLoad", mShowAboutOnLoad);
            DT._UseSnapValuePrecision = SnapValuePrecision;
        }

        public override void SavePreferences()
        {
            base.SavePreferences();
            SetEditorPrefs<CurvyInterpolation>("DefaultInterpolation", CurvyGlobalManager.DefaultInterpolation);
            SetEditorPrefs<Color>("GizmoColor", CurvyGlobalManager.DefaultGizmoColor);
            SetEditorPrefs<Color>("GizmoSelectionColor", CurvyGlobalManager.DefaultGizmoSelectionColor);
            SetEditorPrefs<float>("GizmoControlPointSize", CurvyGlobalManager.GizmoControlPointSize);
            SetEditorPrefs<float>("GizmoOrientationLength", CurvyGlobalManager.GizmoOrientationLength);
            SetEditorPrefs<Color>("GizmoOrientationColor", CurvyGlobalManager.GizmoOrientationColor);
            SetEditorPrefs<CurvySplineGizmos>("Gizmos", CurvyGlobalManager.Gizmos);
            SetEditorPrefs<bool>("SnapValuePrecision", SnapValuePrecision);
            SetEditorPrefs<int>("MaxCachePPU", CurvyGlobalManager.MaxCachePPU);
            SetEditorPrefs<float>("SceneViewResolution", CurvyGlobalManager.SceneViewResolution);
            SetEditorPrefs<bool>("HideManager", CurvyGlobalManager.HideManager);
            SetEditorPrefs<bool>("UseTiny2DHandles", UseTiny2DHandles);
            SetEditorPrefs<bool>("ShowGlobalToolbar", ShowGlobalToolbar);
            SetEditorPrefs<int>("SplineLayer", CurvyGlobalManager.SplineLayer);

            CurvyGlobalManager.SaveRuntimeSettings();
            DT._UseSnapValuePrecision = SnapValuePrecision;
            SetEditorPrefs<string>("CustomizationRootPath", mCustomizationRootPath);
            
        }

        protected override void UpgradePreferences(string oldVersion)
        {
            base.UpgradePreferences(oldVersion);
            // Ensure that About Window will be shown after upgrade
            DeleteEditorPrefs("ShowAboutOnLoad");
            if (oldVersion == "2.0.0")
            {
                if (GetEditorPrefs<float>("GizmoOrientationLength", CurvyGlobalManager.GizmoOrientationLength) == 4)
                    DeleteEditorPrefs("GizmoOrientationLength");
                if (GetEditorPrefs<int>("MaxCachePPU", CurvyGlobalManager.MaxCachePPU) == 4)
                    DeleteEditorPrefs("MaxCachePPU");
            }

        }

        void DeletePreDTSettings()
        {
            DTLog.Log("[Curvy] Removing old preferences");
            EditorPrefs.DeleteKey("Curvy_GizmoColor");
            EditorPrefs.DeleteKey("Curvy_GizmoSelectionColor");
            EditorPrefs.DeleteKey("Curvy_ControlPointSize");
            EditorPrefs.DeleteKey("Curvy_OrientationLength");
            EditorPrefs.DeleteKey("Curvy_Gizmos");
            EditorPrefs.DeleteKey("Curvy_ToolbarLabels");
            EditorPrefs.DeleteKey("Curvy_ToolbarOrientation");
            EditorPrefs.DeleteKey("Curvy_ShowShapeWizardUndoWarning");
            EditorPrefs.DeleteKey("Curvy_KeyBindings");
        }

        [PreferenceItem("Curvy")]
        public static void PreferencesGUI()
        {
            scroll = EditorGUILayout.BeginScrollView(scroll);
            CurvyGlobalManager.DefaultInterpolation = (CurvyInterpolation)EditorGUILayout.EnumPopup("Default Spline Type", CurvyGlobalManager.DefaultInterpolation);
            Instance.SnapValuePrecision = EditorGUILayout.Toggle(new GUIContent("Snap Value Precision", "Round inspector values"), Instance.SnapValuePrecision);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.TextField(new GUIContent("Customization Root Path", "Base Path for custom Curvy extensions"), Instance.CustomizationRootPath);
            if (GUILayout.Button(new GUIContent("<","Select"), GUILayout.ExpandWidth(false)))
            {
                string path = EditorUtility.OpenFolderPanel("Customization Root Path", Application.dataPath, "");
                if (!string.IsNullOrEmpty(path))
                    Instance.CustomizationRootPath = path.Replace(Application.dataPath+"/","");
            }
            EditorGUILayout.EndHorizontal();
            CurvyGlobalManager.MaxCachePPU = Mathf.Max(1, EditorGUILayout.IntField(new GUIContent("Max Cache PPU", "Max cached points per World Unit"), CurvyGlobalManager.MaxCachePPU));
            CurvyGlobalManager.SceneViewResolution = EditorGUILayout.Slider(new GUIContent("SceneView Resolution","Lower values results in faster SceneView drawing"),CurvyGlobalManager.SceneViewResolution, 0, 1);
            CurvyGlobalManager.HideManager = EditorGUILayout.Toggle(new GUIContent("Hide _CurvyGlobal_", "Hide the global manager in Hierarchy?"), CurvyGlobalManager.HideManager);
            foldouts[0] = EditorGUILayout.Foldout(foldouts[0], "Editor", CurvyStyles.Foldout);
            if (foldouts[0])
            {
                CurvyGlobalManager.DefaultGizmoColor = EditorGUILayout.ColorField("Spline color", CurvyGlobalManager.DefaultGizmoColor);
                CurvyGlobalManager.DefaultGizmoSelectionColor = EditorGUILayout.ColorField("Spline Selection color", CurvyGlobalManager.DefaultGizmoSelectionColor);
                CurvyGlobalManager.GizmoControlPointSize = EditorGUILayout.FloatField("Control Point Size", CurvyGlobalManager.GizmoControlPointSize);
                CurvyGlobalManager.GizmoOrientationLength = EditorGUILayout.FloatField(new GUIContent("Orientation Length", "Orientation gizmo size"), CurvyGlobalManager.GizmoOrientationLength);
                CurvyGlobalManager.GizmoOrientationColor = EditorGUILayout.ColorField(new GUIContent("Orientation Color", "Orientation gizmo color"), CurvyGlobalManager.GizmoOrientationColor);
                Instance.UseTiny2DHandles = EditorGUILayout.Toggle("Use tiny 2D handles", Instance.UseTiny2DHandles);
                CurvyGlobalManager.SplineLayer = EditorGUILayout.LayerField(new GUIContent("Default Spline Layer","Layer to use for splines and Control Points"),CurvyGlobalManager.SplineLayer);
            }
            foldouts[1] = EditorGUILayout.Foldout(foldouts[1], "UI", CurvyStyles.Foldout);
            if (foldouts[1])
            {
                Instance.ShowGlobalToolbar=EditorGUILayout.Toggle(new GUIContent("Show Global Toolbar","Always show Curvy Toolbar"),Instance.ShowGlobalToolbar);
                Instance.ToolbarMode = (DTToolbarMode)EditorGUILayout.EnumPopup(new GUIContent("Toolbar Labels", "Defines Toolbar Display Mode"), Instance.ToolbarMode);
                Instance.ToolbarOrientation = (DTToolbarOrientation)EditorGUILayout.EnumPopup(new GUIContent("Toolbar Orientation", "Defines Toolbar Position"), Instance.ToolbarOrientation);
            }

            foldouts[2] = EditorGUILayout.Foldout(foldouts[2], "Shortcuts", CurvyStyles.Foldout);
            if (foldouts[2])
            {
                var keys = Instance.GetProjectBindings();
                foreach (var binding in keys)
                {
                    if (binding.OnPreferencesGUI()) // save changed bindings
                    {
                        Instance.SetEditorPrefs<string>(binding.Name, binding.ToPrefsString());
                    }
                    GUILayout.Space(2);
                    GUILayout.Box("", GUILayout.Height(1),GUILayout.ExpandWidth(true));
                    GUILayout.Space(2);
                }
            }
            if (GUILayout.Button("Reset to defaults"))
            {
                Instance.ResetPreferences();
            }
            
            EditorGUILayout.EndScrollView();

            if (GUI.changed)
            {
                Instance.SavePreferences();
                DT.ReInitialize(false);
            }
            
        }
    }

    /// <summary>
    /// Class for loading image resources
    /// </summary>
    public class CurvyResource : DTResource
    {
        static CurvyResource _Instance;
        public static CurvyResource Instance
        {
            get 
            {
                if (_Instance == null)
                    _Instance = new CurvyResource();
                return _Instance;
            }
        }
        
        public CurvyResource()
        {
            ResourceDLL = FindResourceDLL("CurvyEditorIcons");
            ResourceNamespace = "";//Assets.Curvy.Editor.Resources.";
        }

        private const string fallbackPackedString = "missing,16,16"; 

        public static Texture2D Load (string packedString)
        {
            Texture2D tex = Instance.LoadPacked (packedString);
            if (tex == null)
            {
                DTLog.LogError ("Loading texture from packed string failed: " + packedString);
                return Instance.LoadPacked (fallbackPackedString);
            }
            else return Instance.LoadPacked(packedString);
        }


        

    }
}
