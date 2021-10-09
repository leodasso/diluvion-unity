using Diluvion.Ships;
using Sirenix.OdinInspector;

/// <summary>
/// Triggers the area name UI element to appear
/// </summary>
public class MapArea : Trigger
{
    public LocTerm areaName;


    public override void TriggerAction(Bridge otherBridge)
    {
        base.TriggerAction(otherBridge);
        DUI.AreaName.CreateAreaName(areaName);
    }
}