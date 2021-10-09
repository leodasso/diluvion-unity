using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ReplaceWithInputName : MonoBehaviour {

    public string symbolToRepace;
    public string actionName;

    TextMeshProUGUI tmProText;
    Text text;

	// Use this for initialization
	void Start () {

        text = GetComponent<Text>();
        tmProText = GetComponent<TextMeshProUGUI>();

        Replace();
	}
	
	/// <summary>
    /// Replaces the given symbol on the attached text with the input name for the
    /// given action.  For example 'press W to proceed'
    /// </summary>
    public void Replace()
    {
        string keyMapName = SpiderWeb.Controls.InputMappingName(actionName);

        if (text)
            text.text = text.text.Replace(symbolToRepace, keyMapName);

        if (tmProText)
            tmProText.text = tmProText.text.Replace(symbolToRepace, keyMapName);
    }
}
