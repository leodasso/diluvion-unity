using System.Collections.Generic;
using Diluvion.Ships;
using Diluvion;

/// <summary>
/// Attaches to any trigger collider. When hit, activates the list of conversations for any character on the player ship.
/// </summary>
public class TriggerConvo : Trigger {

    public List<Convo> convosToTrigger = new List<Convo>();

    public override void TriggerAction(Bridge otherBridge)
    {
        base.TriggerAction(otherBridge);

        if (!PlayerManager.PlayerCrew()) return;
        foreach (Dialogue d in PlayerManager.PlayerCrew().GetComponentsInChildren<Dialogue>())
            d.ShowConversation(convosToTrigger);
    }
}
 