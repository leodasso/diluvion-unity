using UnityEngine;

namespace BlendModes
{
	[AddComponentMenu("Image Effects/Camera Overlay")]
	[RequireComponent(typeof(Camera)), ExecuteInEditMode]
	public class CameraOverlay : MonoBehaviour
	{
		/// <summary>
		/// Blend mode of the overlay texture.
		/// </summary>
		public BlendMode BlendMode
		{
			get { return _blendMode; }
			set { SetBlendMode(value); }
		}

		/// <summary>
		/// Texture to use as overlay.
		/// </summary>
		public Texture2D OverlayTexture
		{
			get { return _overlayTexture; }
			set { _overlayTexture = value; }
		}

		/// <summary>
		/// Tint color of the overlay.
		/// </summary>
		public Color TintColor
		{
			get { return _tintColor; }
			set { _tintColor = value; }
		}

		/// <summary>
		/// Material used for overlay texture;
		/// </summary>
		public Material OverlayMaterial
		{
			get
			{
				if (!_overlayMaterial)
					_overlayMaterial = BlendMaterials.GetMaterial(ObjectType.ScreenOverlay, shader: OverlayShader);
				return _overlayMaterial;
			}
			set { _overlayMaterial = value; }
		}

		/// <summary>
		/// Shader used with overlay material.
		/// </summary>
		public Shader OverlayShader
		{
			get
			{
				if (!_overlayShader)
					_overlayShader = Resources.Load<Shader>("BlendModes/Extra/CameraOverlay");
				return _overlayShader;
			}
			set { _overlayShader = value; }
		}

		[SerializeField]
		private Shader _overlayShader;
		[SerializeField]
		private Material _overlayMaterial;
		[SerializeField]
		private BlendMode _blendMode;
		[SerializeField]
		private Texture2D _overlayTexture;
		[SerializeField]
		private Color _tintColor = Color.white;

		private bool isSupported = true;
		private Vector4 screenUv = new Vector4(1, 0, 0, 1);

		public void SetBlendMode (BlendMode blendMode)
		{
			if (blendMode == BlendMode.Normal)
				OverlayMaterial.SetFloat("_IsNormalBlendMode", 1);
			else
			{
				OverlayMaterial = BlendMaterials.GetMaterial(ObjectType.ScreenOverlay, blendMode: blendMode, shader: OverlayShader);
				OverlayMaterial.SetFloat("_IsNormalBlendMode", 0);
			}

			_blendMode = blendMode;
			_overlayTexture = OverlayTexture;
			_tintColor = TintColor;
		}

		private void OnEnable ()
		{
			isSupported = true;
			SetBlendMode(BlendMode);
		}

		private void Start ()
		{
			CheckResources();
		}

		private void OnRenderImage (RenderTexture source, RenderTexture destination)
		{
			if (CheckResources() == false)
			{
				Graphics.Blit(source, destination);
				return;
			}

			#if UNITY_WP8
	    	// WP8 has no OS support for rotating screen with device orientation,
	    	// so we do those transformations ourselves.
			if (Screen.orientation == ScreenOrientation.LandscapeLeft)
				screenUv = new Vector4(0, -1, 1, 0);
			if (Screen.orientation == ScreenOrientation.LandscapeRight)
				screenUv = new Vector4(0, 1, -1, 0);
			if (Screen.orientation == ScreenOrientation.PortraitUpsideDown)
				screenUv = new Vector4(-1, 0, 0, -1);
			#endif

			OverlayMaterial.SetVector("_UV_Transform", screenUv);
			OverlayMaterial.SetColor("_OverlayTintColor", TintColor);
			OverlayMaterial.SetTexture("_OverlayTex", OverlayTexture);
			Graphics.Blit(source, destination, OverlayMaterial);
		}

		private bool CheckResources ()
		{
			isSupported = true;

			if (!SystemInfo.supportsImageEffects)
			{
				SetNotSupported();
				isSupported = false;
			}

			if (!isSupported)
				Debug.LogWarning("Camera overlay image effect has been disabled as it's not supported on the current platform.");

			return isSupported;
		}

		private void SetNotSupported ()
		{
			enabled = false;
			isSupported = false;
			return;
		}
	}
}