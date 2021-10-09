using System.Collections;
using System.Collections.Generic;
using Diluvion;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DUI
{

	public class TorpedoTubeHud : DUIPanel
	{
		public TorpedoTube torpedoTube;

		[Space]
		public Image reloadBar;
		public Image calibrationBar;
		public Animator tubeDisplayAnimator;

		protected override void Update()
		{
			base.Update();
			if (!torpedoTube) return;
			if (!tubeDisplayAnimator) return;
			
			reloadBar.fillAmount = torpedoTube.reloadProgress;
			calibrationBar.fillAmount = torpedoTube.calibrationProgress;

			if (!tubeDisplayAnimator) return;
			tubeDisplayAnimator.SetBool("isActive", torpedoTube.isActiveTube);
			tubeDisplayAnimator.SetBool("isEmpty", false);
			tubeDisplayAnimator.SetFloat("loadProgress", torpedoTube.reloadProgress);
			tubeDisplayAnimator.SetFloat("calibration", torpedoTube.calibrationProgress);
			tubeDisplayAnimator.SetBool("detonateReady", torpedoTube.tubeState == TorpedoTube.TubeState.DetonateReady);
		}
	}
}