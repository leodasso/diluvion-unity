using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DUI;
using Rewired.UI.ControlMapper;
using Sirenix.OdinInspector;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "UI manager", menuName = "Diluvion/UI Manager")]
public class UIManager : ScriptableObject
{
    [BoxGroup("Main", true, true, 4)]
    public GameObject DUI;

    [BoxGroup("Main")] public Vector2 defaultRefResolution = new Vector2(1280, 720);
    
    [BoxGroup("Main")]
    [DisableInEditorMode]
    public List<DUIPanel> allPanelInstances = new List<DUIPanel>();

    [BoxGroup("Main"), Tooltip("Panels that have compatibility requirements.")]
    public List<DUIPanel> panelsWithCompatibility = new List<DUIPanel>();

    #region character ui prefabs
    [TabGroup("Characters")]
    public DUIPanel dialoguePanel;
    [TabGroup("Characters")]
    public DUIPanel dialogHistory;
    [TabGroup("Characters")]
    public DUIPanel hirePanel;
    [TabGroup("Characters")]
    public DUIPanel generalCrewSelect;

    [TabGroup("Characters")] public DUIPanel standaloneCrewSelect;
    [TabGroup("Characters")]
    public DUIPanel boardingPartyPanel;

    [TabGroup("Characters")] 
    public DUIPanel officerLevelUpPanel;

    [TabGroup("Characters")]
    public DUIPanel characterBubbles;

    [TabGroup("Characters")]
    public DUIPanel crewOverview;
    
    [TabGroup("Characters")]
    public DUIPanel chatterPanel;
    #endregion

    #region inventory ui prefabs
    [TabGroup("Inventory")]
    public DUIPanel tradePanel;
    [TabGroup("Inventory")]
    public DUIPanel itemDescription;
    [TabGroup("Inventory")]
    public DUIPanel newItemPanel;
    [TabGroup("Inventory")]
    public DUIPanel itemTakenPopup;
    [TabGroup("Inventory")]
    public DUIPanel moneyPanel;
    [TabGroup("Inventory")] 
    public DUIPanel qtySelectPanel;
    [TabGroup("Inventory")] 
    public DUIPanel itemExchangePanel;
    [TabGroup("Inventory")] 
    public DUIPanel forgeMenu;
    [TabGroup("Inventory")] 
    public DUIPanel lootPanel;
    [TabGroup("Inventory")] 
    public DUIPanel importantItemLayout;
    [TabGroup("Inventory")] 
    public DUIPanel importantItemPanel;
    
    #endregion

    #region quest ui prefabs

    [TabGroup("Quest")]
    public DUIPanel interiorWP;

    [TabGroup("Quest")]
    public DUIPanel mainWaypoint;

    [TabGroup("Quest")] public DUIPanel questUpdateLog;
    
    [TabGroup("Quest")]
    public DUIPanel questShower;
    
    #endregion
    
    #region HUD UI prefabs
    
    [TabGroup("HUD")]
    public DUIPanel sonarContact;

    [TabGroup("HUD")]
    public DUIPanel depthMeter;

    [TabGroup("HUD")] public DUIPanel weaponSwapPanel;

    [TabGroup("HUD")]
    public DUIPanel crushDepthOverlay;

    [TabGroup("HUD")]
    public DUIPanel damageOverlay;
    
    [TabGroup("HUD")]
    public DUIPanel dockPanel;

    [TabGroup("HUD")]
    public DUIPanel resourcesPanel;

    [TabGroup("HUD")]
    public DUIPanel captainTime;

    [TabGroup("HUD")]
    public DUIPanel airPanel;

    [TabGroup("HUD")]
    public DUIPanel tensionPanel;

    [TabGroup("HUD")]
    public DUIPanel throttlePanel;

    [TabGroup("HUD")]
    public DUIReticule basicReticule;

    [TabGroup("HUD")]
    public DUIPanel HPPanel;
    
    #endregion
    
    #region interior ui prefabs

    [TabGroup("Interior")]
    public DUIPanel battlePanel;

    [TabGroup("Interior")]
    public DUIPanel stationDetails;
    [TabGroup("Interior")]
    public DUIPanel stationPreview;
    [TabGroup("Interior")]
    public DUIPanel stationHeader;
    [TabGroup("Interior")]
    public DUIPanel interiorTitle;
    [TabGroup("Interior")]
    public DUIPanel forgeSlotInfo;
    
    [TabGroup("Interior")]
    public DUIPanel shipyard;
    [TabGroup("Interior")]
    public DUIPanel shipComparison;
    [TabGroup("Interior")]
    public DUIPanel shipFullPanel;
    [TabGroup("Interior")]
    public DUIPanel shipHeader;
    [TabGroup("Interior")]
    public DUIPanel shipStats;
    
    [TabGroup("Interior")]
    public DUIPanel cartographerPanel;
    
    #endregion
    
    #region general prefabs
    
    [TabGroup("General"), Space]
    public DUIPanel popup;

    [TabGroup("General")] 
    public DUIPanel saveGamePanel;

    [TabGroup("General")]
    public DUIPanel notifier;
    [TabGroup("General")]
    public DUIPanel captainsTools;


    [TabGroup("General")]
    public DUIPanel airTradePanel;

    [TabGroup("General")]
    public DUIPanel gameOver;
    [TabGroup("General")]
    public DUIPanel pauseMenu;
    [TabGroup("General")]
    public DUIPanel qualitySettings;

    [TabGroup("General")] 
    public DUIPanel loadGamePanel;
    [TabGroup("General")]
    public DUIPanel gameSaved;
    [TabGroup("General")]
    public DUIPanel worldMap;
    [TabGroup("General")]
    public DUIPanel endCredits;
    [TabGroup("General")]
    public DUIPanel debugWindow;
    [TabGroup("General")]
    public DUIPanel areaName;

    [TabGroup("General")] public DUIPanel shipSelectPanel;

    [TabGroup("General")]
    public DUIPanel tutorialPanel;
    
    #endregion

    static UIManager UImanager;
    GameObject _duiInstance;

    public static UIManager Get ()
    {
        if (UImanager)
            return UImanager;
        UImanager = Resources.Load("managers/UI manager") as UIManager;
        UImanager.allPanelInstances.Clear();
        return UImanager;
    }

    public static bool Exists()
    {
        return UImanager != null;
    }


    /// <summary>
    /// Should the panel be hidden? Checks other panels that are up for incompatabilities.
    /// </summary>
    public static bool ShouldHide (DUIPanel prefab)
    {
        if (!prefab) return false;
        if (!prefab.checkCompatibility) return false;
        
        foreach (DUIPanel p in Get().panelsWithCompatibility)
        {
            if (!p) continue;
            if (!p.IsVisible()) continue;
            
            if (p.incompatiblePrefabs.Contains(prefab))
                return true;
        }
        return false;
    }

    /// <summary>
    /// Instantiates the given UI Panel into the correct Canvas, and returns the instance.
    /// </summary>
    public static T Create<T> (T panelPrefab) where T : DUIPanel
    {
        // find the correct parent canvas
        Transform canvas = null;
        foreach (Canvas c in GetDUI().GetComponentsInChildren<Canvas>())
            if (c.name == panelPrefab.canvasType.ToString())
            {
                canvas = c.transform;
                break;
            }

        if (!canvas)
        {
            Debug.LogError("No canvas was available for " + panelPrefab.name + ". Make sure a canvas named " +
                panelPrefab.canvasType.ToString() + " is a child of the DUI prefab.");
            return null;
        }


        return Create<T>(panelPrefab, canvas);
    }

    /// <summary>
    /// Creates the given UI panel inside the given parent, returns the instance.
    /// </summary>
    public static T Create<T> (T panelPrefab, Transform parent) where T : DUIPanel
    {
        // Clear all previous instances of this instance
        if (panelPrefab.oneInstanceOnly)
            Clear(panelPrefab);

        // instantiate into the canvas
        T panelInstance = Instantiate(panelPrefab, parent, false);
        panelInstance.prefab = panelPrefab;

        // add to list
        Get().allPanelInstances.Add(panelInstance);
        
        // If it has compatibility requirements, add it to that list as well.
        if (panelInstance.incompatiblePrefabs.Count > 0) 
            Get().panelsWithCompatibility.Add(panelInstance);

        // return the instance
        return panelInstance;
    }

    #region control mapper
    static ControlMapper _controlMapperInstance;
    public static ControlMapper GetControlMapper()
    {
        if (_controlMapperInstance) return _controlMapperInstance;

        _controlMapperInstance = GetDUI().GetComponentInChildren<ControlMapper>();
        _controlMapperInstance.ScreenClosedEvent += ResetPauseMenu;
        return _controlMapperInstance;
    }

    public static void ShowControlMapper()
    {
        GetControlMapper().Open();
    }

    public static void HideControlMapper(bool save)
    {
        if (GetControlMapper().isOpen) GetControlMapper().Close(save);
        ResetPauseMenu();
    }
    
    #endregion
    
    static void ResetPauseMenu()
    {
        PauseMenu.Reset();
        global::DUI.QualitySettings.Reset();
    }

    public static EventSystem GetEventSystem()
    {
        if (EventSystem.current != null) return EventSystem.current;
        EventSystem.current = GetDUI().GetComponent<EventSystem>();
        return EventSystem.current;
    }

#region panel getting
    
    /// <summary>
    /// Returns all instances of the given panel prefab.
    /// </summary>
    public static List<DUIPanel> GetPanels (DUIPanel panelPrefab)
    {
        List<DUIPanel> returnList = new List<DUIPanel>();
        foreach (DUIPanel p in Get().allPanelInstances)
        {
            if (!p)
                continue;
            if (p.prefab == panelPrefab)
                returnList.Add(p);
        }

        return returnList;
    }
    
    /// <summary>
    /// Returns all the instances of the given type of DUI panel.
    /// </summary>
    public static List<T> GetPanels<T> () where T : DUIPanel
    {
        List<T> returnList = new List<T>();
        foreach (DUIPanel p in Get().allPanelInstances)
        {
            if (p as T != null)
                returnList.Add(p as T);
        }

        return returnList;
    }
    
    /// <summary>
    /// Get the first instance of the given panel prefab.
    /// </summary>
    public static DUIPanel GetPanel (DUIPanel panelPrefab)
    {
        List<DUIPanel> panels = GetPanels(panelPrefab);
        if (panels.Count > 0)
            return panels[0];
        return null;
    }

    /// <summary>
    /// Get the first instance of the given type
    /// </summary>
    public static T GetPanel<T> () where T : DUIPanel
    {
        List<T> panels = GetPanels<T>();
        if (panels.Count > 0)
            return panels[0];
        return null;
    }
    
#endregion

    /// <summary>
    /// Returns true if there's panels up that prevent the camera from moving.
    /// </summary>
    public static bool Locked ()
    {
        foreach (DUIPanel p in Get().allPanelInstances)
        {
            if (!p)continue;
            if (p.locksCamera) return true;
        }

        return false;
    }

    /// <summary>
    /// returns true if a UI element is up that's interactive, (dialogue, trade panel, etc)
    /// </summary>
    public static bool InteractiveWindowOpen ()
    {
        foreach (DUIView v in GetPanels<DUIView>())
            if (v.usesNavigation)
                return true;
        return false;
    }

    public static void RemoveFromList (DUIPanel panel)
    {
        if (!Exists()) return;
        Get().allPanelInstances.Remove(panel);
        Get().panelsWithCompatibility.Remove(panel);
    }

    /// <summary>
    /// Returns the instance of the DUI object which contains all canvases.
    /// <para>If no instance exists, creates one.</para>
    /// </summary>
    public static GameObject GetDUI () { return Get().GetDUI_i(); }
    GameObject GetDUI_i ()
    {
        if (_duiInstance)
            return _duiInstance;
        _duiInstance = Instantiate(DUI);
        
        // Set the default UI scale
        float scale = PlayerPrefs.GetFloat("_uiScale", 1);
        SetUiScale(scale);
        
        DontDestroyOnLoad(_duiInstance);
        return _duiInstance;
    }

    public static void SetUiScale(float value)
    {
        foreach (var canvasScaler in GetDUI().GetComponentsInChildren<CanvasScaler>())
        {
            canvasScaler.referenceResolution = Get().defaultRefResolution * value;
        }
    }

    /// <summary>
    /// Removes the main DUI parent
    /// </summary>
    public static void ClearAll()
    {
        if (Get()._duiInstance) Destroy(Get()._duiInstance.gameObject);
    }
    
    #region clearing

    /// <summary>
    /// Removes all the instances that have been spawned in.
    /// </summary>
    public static void Clear ()
    {
        foreach (DUIPanel p in Get().allPanelInstances)
        {
            if (!p) continue;
            
            p.DelayedEnd();
        }

        Get().panelsWithCompatibility.Clear();
        Get().allPanelInstances.Clear();
    }

    /// <summary>
    /// Removes all the instances of the given type T.
    /// </summary>
    /// <typeparam name="T">Type of panel to remove</typeparam>
    public static void Clear<T> () where T : DUIPanel
    {
        List<DUIPanel> toRemove = new List<DUIPanel>();
        foreach (DUIPanel p in Get().allPanelInstances)
        {
            if (!p)
                continue;
            if (p as T != null)
                toRemove.Add(p);
        }

        foreach (DUIPanel p in toRemove)
        {
            p.End();
            RemoveFromList(p);
        }
    }

    /// <summary>
    /// Removes all panels that originate from the given panel prefab.
    /// </summary>
    public static void Clear (DUIPanel panelPrefab)
    {
        List<DUIPanel> toRemove = new List<DUIPanel>();
        foreach (DUIPanel p in Get().allPanelInstances)
        {
            if (!p)
                continue;
            if (p.prefab == panelPrefab)
                toRemove.Add(p);
        }

        foreach (DUIPanel p in toRemove)
        {
            p.End();
            RemoveFromList(p);
        }
    }
    
    #endregion

    /// <summary>
    /// Are any UI panels preventing us from transitioning between interior / 3D view?
    /// </summary>
    public static bool CanTransition ()
    {
        foreach (DUIPanel p in Get().allPanelInstances)
        {
            if (!p)
                continue;
            if (!p.CanTransition())
                return false;
        }

        return true;
    }

    /// <summary>
    /// Shows or hides ALL UI panels under duicontroller
    /// </summary>
    public static void ShowAllUI(bool visible)
    {
        GetDUI().GetComponent<DUIController>().SetHidden(!visible);
    }

}

/// <summary>
/// Canvas types. Settings renders on top of panel, which renders on top of HUD.
/// </summary>
public enum CanvasType
{
    HUD,
    panel,
    settings
}