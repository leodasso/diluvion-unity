using UnityEngine;
using UnityEngine.UI;
using Diluvion;
using SpiderWeb;
using Sirenix.OdinInspector;
using TMPro;

namespace DUI
{
	//Class to create a docking target UI (What comes up when you get close enough to a valid, dockable object)
	public class DUIDock : DUIPanel
	{
		public Image spriteRenderer;
		public Sprite newDock;
		public Sprite oldDock;

		[ReadOnly] public bool isVisible;
		[ReadOnly] public bool inRange;
		[ReadOnly] public bool nearestDock;

		[Space] public Image dockIcon;
		public TextMeshProUGUI dockText;

		[ReadOnly] public DockPort myDock;
		public string dockInputName;
		public Animator dockAnimator;

		Vector3 _newPos;
		static float _dockVisibleRange = 60;
		float _distToPlayer;

		//is the point inside the frustrum
		protected override void Start()
		{
			base.Start();
			SnapAlpha(0);
			alpha = 0;
			string dockName = Controls.InputMappingName(dockInputName);
			dockText.text = dockText.text.Replace("#", dockName);
		}

		public static DUIDock MakeDockHud(DockPort d)
		{
			DUIDock instance = UIManager.Create(UIManager.Get().dockPanel as DUIDock);
			instance.myDock = d;
			return instance;
		}

		//Function to determine if the panel is visible and if so, move it
		void LateUpdate()
		{
			if (!Camera.main || !myDock) return;
			if (!PlayerManager.PlayerDocks()) return;

			if (myDock.dockControl == null)
			{
				alpha = 0;
				return;
			}

			if (!myDock.dockControl.dockActive)
			{
				alpha = 0;
				return;
			}

			// Change sprite to reflect if the player's docked with this yet
			spriteRenderer.sprite = myDock.dockControl.dockedWithPlayer ? oldDock : newDock;
			
			// check visibility of this dock
			isVisible = IsDockVisible();

			// check distance of dock
			_distToPlayer = Vector3.Distance(myDock.transform.position, PlayerManager.PlayerShip().transform.position);
			inRange = _distToPlayer <= _dockVisibleRange;
			
			// check if this is the nearest dock and in range. if so, it'll show as dockable
			nearestDock = PlayerManager.PlayerDocks().NearestDockPort() == myDock;
			if (_distToPlayer > DockControl.dockRange) nearestDock = false;

			// set position of dock
			transform.position = FollowTransform(myDock.transform.position, 60, Camera.main);

			bool animVisible = inRange && isVisible;
			
			dockAnimator.SetBool("nearestDock", nearestDock);
			dockAnimator.SetBool("isVisible", animVisible);
		}

		public void PlayDockAvailableAudio()
		{
			SpiderSound.MakeSound("Play_MUS_Dock_Available", gameObject);
		}



		bool IsDockVisible()
		{
			if (OrbitCam.CamMode() != CameraMode.Normal) return false;
			if (myDock == null) return false;
			if (!myDock.gameObject.activeInHierarchy) return false;
			return Cam.CanIsee(Camera.main, myDock.transform.position);
		}
	}
}