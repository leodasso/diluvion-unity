using UnityEngine;
using System.Collections;
using HeavyDutyInspector;

/// <summary>
/// Changes the game's state so the next 'game over' screen will be replaced with the screen
/// for completing the demo.
/// </summary>
public class EndDemoSetter : Trigger {

    /*
    public bool requireGameState;

    [HideConditional(true, "requireGameState", true)]
    public BuildConfigContainer gameState;


    public override void TriggerAction(Bridge otherBridge)
    {
        // Game config culling
        if ( requireGameState )
            if ( !DiluvionGameState.Get().IsConfig(gameState) ) return;

        base.TriggerAction(otherBridge);
        if ( DUIController.Get() ) DUIController.Get().showDemoEnding = true;
    }
    */
}
