using UnityEngine;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters;
using Diluvion.SaveLoad;
using SpiderWeb;
using DUI;
using Sirenix.OdinInspector;

[System.Serializable]
[CreateAssetMenu(fileName = "new popup", menuName = "Diluvion/Popup")]
public class PopupObject : ScriptableObject
{

    public enum pType { firstTime, everyTime };
    public enum ButtonSetup { okayButton, yesNoButtons }
    
    public bool saves = false;
    [ShowIf("saves")]
    public pType popupType = pType.everyTime;
    public ButtonSetup buttonSetup;

    [BoxGroup("main", false)]
    public string title;

    [BoxGroup("main", false)]
    [TextArea(3, 12)]
    public string mainText;

    [Tooltip("If you need to describe an input in here (press p to poop), enter the action" +
        "name here. Anywhere in the body that you place a #, it will replace the # with the button" +
        "that controls that input."), Space]
    public string inputAction;

    [Button()]
    void TestLoc()
    {
        Debug.Log(LocalizedTitle());
        Debug.Log(LocalizedMainText());
    }

    [ButtonGroup("loc", -1)]
    public void AddLocKeys()
    {

        Localization.AddToKeyLib("popup_" + name + "_title", title);
        Localization.AddToKeyLib("popup_" + name + "_body", mainText);
    }

    public string LocalizedTitle()
    {
        return Localization.GetFromLocLibrary("popup_" + name + "_title", title);
    }

    public string LocalizedMainText()
    {
        // get the text from the loc library
        string mainTextString = Localization.GetFromLocLibrary("popup_" + name + "_body", mainText);
        
        if (!string.IsNullOrEmpty(mainTextString) && Application.isPlaying)
            // replace any hashtags with the name of the control
            mainTextString = mainTextString.Replace("#", Controls.InputMappingName(inputAction));

        return mainTextString;
    }

    /// <summary>
    /// Calls CreateUI(), but is formatted so unity events can use it.
    /// </summary>
    public void ShowPopup()
    {
        CreateUI();
    }

    /// <summary>
    /// Creates the popup UI for this specific popup.
    /// </summary>
    [Button]
    public DUIPopup CreateUI()
    {
        if (!Application.isPlaying)
        {
            Debug.LogError("Popups can only be tested while game is running.");
            return null;
        }
        
        // Check if this popup should only show once
        if (saves && DSave.current != null)
        {
            if (DSave.current.ShownPopup(name) && popupType == pType.firstTime) return null;
            DSave.current.SavePopup(name);
        }
        
        DUIPopup newPopup = UIManager.Create(UIManager.Get().popup as DUIPopup);
        newPopup.Init(this);
        return newPopup;
    }

    /// <summary>
    /// Creates the popup UI, and formats text to replace any [0], [1], [n] symbols with 
    /// the index in the replacements string.
    /// </summary>
    public DUIPopup CreateUI(List<string> replacements)
    {
        DUIPopup newPopup = UIManager.Create(UIManager.Get().popup as DUIPopup);
        newPopup.Init(this, replacements);
        return newPopup;
    }

    /// <summary>
    /// Creates the popup, and gives functions for accept and deny.
    /// </summary>
    public DUIPopup CreateUI(DUIPopup.choiceDelegate accept, DUIPopup.choiceDelegate deny)
    {
        DUIPopup newPopup = CreateUI();
        SetButtonActions(accept, deny, newPopup);
        return newPopup;
    }

    /// <summary>
    /// Creates the popup, and gives functions for accept and deny.
    /// </summary>
    public DUIPopup CreateUI(DUIPopup.choiceDelegate accept, DUIPopup.choiceDelegate deny, List<string> replacements)
    {
        DUIPopup newPopup = CreateUI(replacements);
        SetButtonActions(accept, deny, newPopup);
        return newPopup;
    }

    void SetButtonActions(DUIPopup.choiceDelegate accept, DUIPopup.choiceDelegate deny, DUIPopup popupWindow)
    {
        if (buttonSetup != ButtonSetup.yesNoButtons)
            Debug.LogError("Delegates have been entered into this popup, but it's button setup doesn't allow user choice!", this);

        popupWindow.acceptDelegate += accept;
        popupWindow.denyDelegate += deny;
    }

}
