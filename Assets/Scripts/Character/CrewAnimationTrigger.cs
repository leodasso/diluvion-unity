using Diluvion.Ships;
using Diluvion;

public class CrewAnimationTrigger : Trigger {

	public CrewAnimationTool animTool;
	public CharacterInfo specificCrew;

	public override void TriggerAction (Bridge otherBridge)
	{
		base.TriggerAction (otherBridge);

		InteriorManager otherInterior = otherBridge.interiorManager;
		otherInterior.TriggerCrewAnimations(animTool, specificCrew);
	}
}