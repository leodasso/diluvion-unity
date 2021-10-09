using UnityEngine;
using Rewired;
using SpiderWeb;
using HeavyDutyInspector;

public class RewiredLocalizeTool : MonoBehaviour {

    [Button("Add action keys", "AddAllTerms", true)] public bool hidden1;

    public static string rewiredActionsLocPrefix = "reInput_";
    public static string rewiredLocPosPrefix = "pos_";
    public static string rewiredLocNegPrefix = "neg_";

    public static string ActionKey(InputAction action)
    {
        return rewiredActionsLocPrefix + action.name;
    }

    public static string ActionPosKey(InputAction action)
    {
        if ( action.type == InputActionType.Button )
            return rewiredActionsLocPrefix + action.name;

        return rewiredActionsLocPrefix + rewiredLocPosPrefix + action.name;
    }

    public static string ActionNegKey(InputAction action)
    {
        if ( action.type == InputActionType.Button )
            return rewiredActionsLocPrefix + action.name;

        return rewiredActionsLocPrefix + rewiredLocNegPrefix + action.name;
    }


    public static string ActionLocalizedName(InputAction action)
    {
        return Localization.GetFromLocLibrary(ActionKey(action), action.descriptiveName);
    }

    public static string ActionLocalizedPosName(InputAction action)
    {
        return Localization.GetFromLocLibrary(ActionPosKey(action), action.positiveDescriptiveName);
    }

    public static string ActionLocalizedNegName(InputAction action)
    {
        return Localization.GetFromLocLibrary(ActionNegKey(action), action.negativeDescriptiveName);
    }

    public static string CategoryLocalizedName(InputCategory category)
    {
        return Localization.GetFromLocLibrary(rewiredActionsLocPrefix + category.name, category.descriptiveName);
    }


    /// <summary>
    /// Adds loc keys for each input action
    /// </summary>
    void AddAllTerms()
    {
        foreach (InputAction action in ReInput.mapping.UserAssignableActions)
        {
            // Add the main name
            Localization.AddToKeyLib(ActionKey(action), action.descriptiveName);

            if (action.type == InputActionType.Axis)
            {
                Localization.AddToKeyLib(ActionNegKey(action), action.negativeDescriptiveName);
                Localization.AddToKeyLib(ActionPosKey(action), action.positiveDescriptiveName);
            }
        }

        foreach (InputCategory category in ReInput.mapping.UserAssignableActionCategories)
        {
            Localization.AddToKeyLib(rewiredActionsLocPrefix + category.name, category.descriptiveName);
        }

    }
}
