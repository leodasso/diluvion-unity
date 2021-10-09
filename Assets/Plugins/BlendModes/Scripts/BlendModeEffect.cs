using UnityEngine;
using UnityEngine.UI;

namespace BlendModes
{
	
	
	/// <summary>
	/// Applies blending effect to an object with a compatible renderer.
	/// </summary>
	[AddComponentMenu("Effects/Blend Mode"), ExecuteInEditMode]
	public class BlendModeEffect : MonoBehaviour
	{
		/// <summary>
		/// If there's issues with blend modes in standalone builds, turn this on.
		/// </summary>
		static bool forceGrabMode = true;
		
		/// <summary>
		/// Stencil ref value used for masking.
		/// Set this to 0, in case you're using stencil buffer for other needs to prevent conflicts.
		/// </summary>
		const int DEFAULT_STENCIL_REF = 0;

		/// <summary>
		/// Current blend mode of the object.
		/// </summary>
		public BlendMode BlendMode
		{
			get { return _blendMode; }
			set { SetBlendMode(value, RenderMode); }
		}

		/// <summary>
		/// Current render mode used for blending.
		/// </summary>
		public RenderMode RenderMode
		{
			get { return _renderMode; }
			set { SetBlendMode(BlendMode, value); }
		}

		/// <summary>
		/// Texture to use with the object.
		/// Have no effect for UI and sprite objects.
		/// </summary>
		public Texture Texture
		{
			get { return _texture; }
			set { _texture = value; SetupTexture(); }
		}

		/// <summary>
		/// Tint color of the object.
		/// Have no effect for UI and sprite objects.
		/// </summary>
		public Color TintColor
		{
			get { return _tintColor; }
			set { _tintColor = value; SetupTintColor(); }
		}

		/// <summary>
		/// Current mask mode of the object.
		/// Disabled: blend with everything (default).
		/// NothingButMask: blend only with objects affected by BlendModeEffectMask.
		/// EverythingButMask: blend with everything, except objects affected by BlendModeEffectMask.
		/// </summary>
		public MaskMode MaskMode
		{
			get { return _maskMode; }
			set { _maskMode = value; SetupStencil(); }
		}

		/// <summary>
		/// Controls what should be rendered in the masked areas.
		/// Cutout: pixels from the masked areas will be discarded (become transparent).
		/// Normal: pixels from the masked areas will be rendered in normal blend mode.
		/// </summary>
		public MaskBehaviour MaskBehaviour
		{
			get { return _maskBehaviour; }
			set { SetupStencil(); _maskBehaviour = value; }
		}

		/// <summary>
		/// Whether the object should be considered as a mask.
		/// </summary>
		public bool EnableAutoMask
		{
			get { return _enableAutoMask; }
			set { SetupStencil(); _enableAutoMask = value; }
		}

		/// <summary>
		/// Type of the object in context of applying blend mode effect.
		/// </summary>
		public ObjectType ObjectType
		{
			get 
			{
				if (GetComponent<Text>()) return ObjectType.UIDefaultFont;
				if (GetComponent<MaskableGraphic>()) return ObjectType.UIDefault;
				if (GetComponent<SpriteRenderer>()) return ObjectType.SpriteDefault;
				if (GetComponent<MeshRenderer>()) return ObjectType.MeshDefault;
				if (GetComponent<ParticleSystem>()) return ObjectType.ParticleDefault;

				return ObjectType.Unknown;
			}
		}

		/// <summary>
		/// Material the object had before Blend Mode effect was applied.
		/// Used for fallback when the component is removed/disabled or normal blend mode is selected.
		/// </summary>
		[HideInInspector]
		public Material OriginalMaterial;

		/// <summary>
		/// Current material of the object.
		/// </summary>
		public Material Material
		{
			get
			{
				switch (ObjectType)
				{
					case ObjectType.UIDefault:
						return GetComponent<MaskableGraphic>().material;
					case ObjectType.UIDefaultFont:
						return GetComponent<Text>().material;
					case ObjectType.SpriteDefault:
						return GetComponent<SpriteRenderer>().sharedMaterial;
					case ObjectType.MeshDefault:
						return GetComponent<MeshRenderer>().sharedMaterial;
					case ObjectType.ParticleDefault:
						return GetComponent<ParticleSystem>()
							.GetComponent<Renderer>().sharedMaterial;
					default:
						return null;
				}
			}
			set
			{
				switch (ObjectType)
				{
					case ObjectType.UIDefault:
						GetComponent<MaskableGraphic>().material = value;
						break;
					case ObjectType.UIDefaultFont:
						GetComponent<Text>().material = value;
						break;
					case ObjectType.SpriteDefault:
						GetComponent<SpriteRenderer>().sharedMaterial = value;
						break;
					case ObjectType.MeshDefault:
						GetComponent<MeshRenderer>().sharedMaterial = value;
						break;
					case ObjectType.ParticleDefault:
						GetComponent<ParticleSystem>()
							.GetComponent<Renderer>().sharedMaterial = value;
						break;
				}
			}
		}

		[SerializeField]
		private BlendMode _blendMode;
		[SerializeField]
		private RenderMode _renderMode;
		[SerializeField]
		private Texture _texture;
		[SerializeField]
		private Color _tintColor = Color.white;
		[SerializeField]
		private MaskMode _maskMode = MaskMode.Disabled;
		[SerializeField]
		private MaskBehaviour _maskBehaviour = MaskBehaviour.Cutout;
		[SerializeField]
		private bool _enableAutoMask = false;

		/// <summary>
		/// Sets specific blend mode to the object.
		/// </summary>
		/// <param name="blendMode">Blend mode.</param>
		/// <param name="renderMode">Render mode to use for blending.</param>
		public void SetBlendMode (BlendMode blendMode, RenderMode renderMode = RenderMode.Grab)
		{
			_blendMode = blendMode;
			
			// disallow color dodge mode
			if (blendMode == BlendMode.ColorDodge) _blendMode = BlendMode.Overlay;
			
			if (forceGrabMode) renderMode = RenderMode.Grab;
			else _renderMode = renderMode;

			if (ObjectType == ObjectType.Unknown) return;

			Material = BlendMaterials.GetMaterial(ObjectType, renderMode, _blendMode);

			SetupTexture();
			SetupTintColor();
			SetupStencil();
		}

		public void OnEnable ()
		{
			// Swap with CameraOverlay script when applying to a camera.
			if (GetComponent<Camera>())
			{
				gameObject.AddComponent<CameraOverlay>();
				if (Application.isEditor) DestroyImmediate(this);
				else Destroy(this);
				return;
			}

			SetBlendMode(BlendMode, RenderMode);
		}

		public void OnDisable ()
		{
			// Return assigned texture to the original material.
			var curTex = Texture;
			Material = OriginalMaterial;
			if (Material && curTex) Material.mainTexture = curTex;
		}

		private void SetupTintColor ()
		{
			if (!Material) return;

			if (ObjectType == ObjectType.MeshDefault || ObjectType == ObjectType.ParticleDefault)
				Material.color = TintColor;
		}

		private void SetupTexture ()
		{
			if (!Material) return;

			if (ObjectType == ObjectType.MeshDefault || ObjectType == ObjectType.ParticleDefault)
				Material.mainTexture = Texture;
		}

		private void SetupStencil ()
		{
			if (!Material) return;

			Material.SetFloat("_StencilRef", DEFAULT_STENCIL_REF);

			// _StencilComp: Always (0), Never (1), Equal (3), NotEqual (6)

			float blendStencilComp = -1;
			float normalStencilComp = -1;
			float maskStencilComp = EnableAutoMask ? 0 : 1;

			if (BlendMode == BlendMode.Normal)
			{
				blendStencilComp = 1;
				normalStencilComp = 0;
			}
			else
			{
				blendStencilComp = MaskMode == MaskMode.Disabled ? 0 : MaskMode == MaskMode.NothingButMask ? 3 : 6;

				if (MaskMode == MaskMode.Disabled) normalStencilComp = 1;
				else
				{
					if (MaskBehaviour == MaskBehaviour.Cutout) normalStencilComp = 1;
					else 
					{
						if (MaskMode == MaskMode.NothingButMask) normalStencilComp = 6;
						else if (MaskMode == MaskMode.EverythingButMask) normalStencilComp = 3;
					}
				}
			}

			Material.SetFloat("_BlendStencilComp", blendStencilComp);
			Material.SetFloat("_NormalStencilComp", normalStencilComp);
			Material.SetFloat("_MaskStencilComp", maskStencilComp);
		}
	}
}
