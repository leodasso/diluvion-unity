using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Diluvion;
using SpiderWeb;
using DUI;
using Sirenix.OdinInspector;

/// <summary>
/// Holds all the info needed to show a single tutorial element.
/// </summary>
[CreateAssetMenu(fileName = "new tutorial", menuName = "Diluvion/tutorial")]
public class TutorialObject : ScriptableObject {

    List<CameraMode> invalidCameraModes = new List<CameraMode>();

    [TextArea(2, 5)]
    public string text;

    [TextArea(2, 5), ReadOnly] public string locText;

    [HorizontalGroup("pos"), HideLabel]
    public verticalPos vPos = verticalPos.center;
    
    [HorizontalGroup("pos"), HideLabel]
    public horizontalPos hPos = horizontalPos.center;
    
    [Title("Timing"), MinValue(0), MaxValue(100)]
    public float delayTime = 1;
    
    [HorizontalGroup("timeLimit")]
    public bool useTimeLimit;

    [HorizontalGroup("timeLimit")]
    [ShowIf("useTimeLimit"), HideLabel]
    public float timeLimit = 5;

    [Space] public bool showPointer = true;
    
    [ToggleGroup("completeFromInput")]
    public bool completeFromInput;
    
    [ToggleGroup("completeFromInput")]
    public InputType inputType = InputType.button;
    
    [Tooltip("Name of the rewired input used to pass this tutorial.")]
    [ToggleGroup("completeFromInput")]
    public string inputName = "input";
    
    [ToggleGroup("completeFromInput")]
    public bool twoInputs;
    
    [ShowIf("twoInputs")]
    [ToggleGroup("completeFromInput")]
    public string inputName2 = "input name";

    /// <summary>
    /// Auto generated based on name of object.
    /// </summary>
    [HideInInspector]
    public string locKey = "";

    public static string locPrefix = "GUI/tut_";

    public string LocalizedText()
    {
        return Localization.GetFromLocLibrary(locPrefix + name, "[" + text + "]");
    }


    [Button]
    void RefreshLoc()
    {
        locKey = locPrefix + name;

        locText = LocalizedText();
        
        #if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
        #endif
    }

    public void ShowTutorial()
    {
        TutorialPanel.ShowTutorial(this);

    }

    /// <summary>
    /// Return true if the player pressed the button i wanted them to
    /// </summary>
    public bool InputWasUsed()
    {
        if (!completeFromInput) return false;
        if (inputName.Length < 1) return false;
        if (inputType == InputType.button)
        {
            if (GameManager.Player().GetButtonDown(inputName)) return true;
        }
        else if (Mathf.Abs(GameManager.Player().GetAxis(inputName)) > .02f) return true;
        return false;
    }
}
