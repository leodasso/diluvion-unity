using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using SpiderWeb;

[CreateAssetMenu(menuName = "Diluvion/Loc term")]
public class LocTerm : ScriptableObject
{
    [ToggleLeft, Tooltip("Use a custom localization key. By default, the name of this object is used.")]
    public bool customLocKey;

    [ShowIf("customLocKey")]
    public string locKey = "new key";
    
    [MultiLineProperty(5), OnValueChanged("RefreshPreview")]
    [HideLabel, Title("Text")]
    [Tooltip("The text of this term. The lockey is the name of this object.")]
    public string text;

    [SerializeField, ShowInInspector, ReadOnly, LabelText("Preview"), LabelWidth(50),
     MultiLineProperty(5), Tooltip("Preview of localized text. Text in [] brackets means it's not been localized.")]
    string localizedPreview;

    [Button]
    void RefreshPreview()
    {
        localizedPreview = LocalizedText();
    }

    string LocKey()
    {
        return customLocKey ? locKey : name;
    }

    public string LocalizedText()
    {
        return Localization.GetFromLocLibrary(LocKey(), "[" + text + "]");
    }

    [Button("Add to Loc Library")]
    public void AddTerm()
    {
        Localization.AddToKeyLib(LocKey(), text);
    }

}
