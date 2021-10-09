using System.Collections;
using System.Collections.Generic;
using Diluvion.Ships;
using UnityEngine;
using Sirenix.OdinInspector;

namespace Diluvion
{

	/// <summary>
	/// Has functions for making the player ship see-through. Works with <see cref="SeeThrougher"/>
	/// </summary>
	public class ShipInvisibler : MonoBehaviour
	{
		public bool debugCasting = false;
		
		[ShowIf("debugCasting")]
		public bool castHitting;

		[ShowIf("debugCasting")]
		public float debugCastRadius = .5f;
		
		public bool transparent;
		public float transparentTime;

		public bool aimTransparent;
		public float aimTransparentTime;

		static ShipInvisibler instance;
		static SeeThrougher seeThrougher;
		
		// Setting opacity directly, or setting invisible time and letting update set opacity
		static bool _setDirect;
		
		bool wasTransparent;
		bool wasAimTransparent;

		const float TransparentOpacity = .5f;
		

		static ShipInvisibler Get()
		{
			Camera c = Camera.main;

			if (!c) return null;
			if (instance) return instance;

			instance = c.gameObject.AddComponent<ShipInvisibler>();
			return instance;
		}

		
		/// <summary>
		/// Sets the ship to the given semitransparent opacity for a certain amount of time.
		/// </summary>
		/// <param name="time">Max: 30 seconds</param>
		public static void SetInvisible(float time)
		{
			Get().transparentTime = time;
		}
		

		public static void SetOpacity(float opacity)
		{
			if (Get().transparentTime + Get().aimTransparentTime > 0.1f) return;
			ShipSeeThrougher().SetOpacity(opacity);
		}

		public static void SetInvisibleIfObstructed(float time)
		{
			_setDirect = false;
			Get().aimTransparentTime = Mathf.Clamp(Get().aimTransparentTime, time, 30);
		}

		/// <summary>
		/// Returns the seeThrougher component attached to the player ship, if available.
		/// </summary>
		static SeeThrougher ShipSeeThrougher()
		{
			if (seeThrougher) return seeThrougher;
			
			if (!PlayerManager.PlayerShip())
			{
				//Debug.LogError("No player ship is available.");
				return null;
			}
			
			seeThrougher = PlayerManager.PlayerShip().GetComponent<SeeThrougher>();

			if (!seeThrougher)
			{
				Debug.LogError("Player ship has no SeeThrougher component.");
				return null;
			}

			return seeThrougher;
		}


		void Update()
		{
			
			if (debugCasting) castHitting = CastHitsPlayer(debugCastRadius);
			if (!ShipSeeThrougher()) return;


			// Check if the ship should be aimTransparent - that is, only transparent if the ship is obstructing the reticule.
			if (aimTransparentTime > 0)
			{
				aimTransparentTime -= Time.unscaledDeltaTime;
				aimTransparent = CastHitsPlayer(1);
			}
			else aimTransparent = false;

			// Check if the ship should be transparent. This will override any aim transparency
			if (transparentTime > 0)
			{
				transparentTime -= Time.unscaledDeltaTime;
				transparent = true;
			}
			else transparent = false;

			if (!_setDirect)
			{
				// Lerp opacity for regular transparency.
				if (transparent && !wasTransparent) ShipSeeThrougher().LerpOpacity(TransparentOpacity);
				if (!transparent && wasTransparent) ShipSeeThrougher().LerpOpacity(1);

				if (!transparent)
				{
					if (aimTransparent && !wasAimTransparent) ShipSeeThrougher().LerpOpacity(TransparentOpacity);
					if (!aimTransparent && wasAimTransparent) ShipSeeThrougher().LerpOpacity(1);
				}
			}

			wasAimTransparent = aimTransparent;
			wasTransparent = transparent;
		}
		

		RaycastHit[] _castOutput = new RaycastHit[30];
		bool CastHitsPlayer(float radius = .1f)
		{
            if (PlayerManager.PlayerControls() == null) return false;
			var dir = PlayerManager.PlayerControls().Aimer().transform.forward;
			
			_castOutput = Physics.SphereCastAll(transform.position, radius, dir, 100);

			foreach (var VARIABLE in _castOutput)
			{
				if (VARIABLE.collider == null) continue;
				if (VARIABLE.collider.isTrigger) continue;
				
				Bridge b = VARIABLE.collider.GetComponentInParent<Bridge>();
				if (b != null) return true;
			}

			return false;
		}
	}
}