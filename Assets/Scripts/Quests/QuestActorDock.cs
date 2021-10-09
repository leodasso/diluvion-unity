using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Diluvion
{

	public class QuestActorDock : QuestActor
	{
		public DockControl dockControl;

		[Tooltip("If inversed, the dock will be active when the quest actor's conditions are NOT met, and inactive when they are.")]
		public bool inverse;

		protected override void OnActivate()
		{
			base.OnActivate();
			dockControl.dockActive = !inverse;
		}

		protected override void OnDeactivate()
		{
			base.OnDeactivate();
			dockControl.dockActive = inverse;
		}
	}
}