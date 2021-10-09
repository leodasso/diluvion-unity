using UnityEngine;
using Diluvion.Ships;

public class CutsceneTrigger : Trigger {

    public GameObject cutscenePrefab;
    public Color transColor = Color.black;

    public override void TriggerAction(Bridge otherBridge)
    {
        base.TriggerAction(otherBridge);

        Diluvion.Cutscene.ShowCutscene(cutscenePrefab, 1, transColor);
    }
}
