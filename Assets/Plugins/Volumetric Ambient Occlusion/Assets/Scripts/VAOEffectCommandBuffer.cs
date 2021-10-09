// Copyright (c) 2016-2018 Jakub Boksansky, Adam Pospisil - All Rights Reserved
// Volumetric Ambient Occlusion Unity Plugin 1.9

using UnityEngine;
using UnityEngine.Rendering;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.AnimatedValues;
#endif
using System;
using System.Collections.Generic;
using System.Reflection;


namespace Wilberforce.VAO
{

    [ExecuteInEditMode]
    [RequireComponent(typeof(Camera))]
    [HelpURL("https://projectwilberforce.github.io/vaomanual/")]
    public class VAOEffectCommandBuffer : MonoBehaviour
    {

        #region VAO Parameters

        /// <summary>
        /// Radius of AO calculation
        /// </summary>
        public float Radius = 0.5f;

        /// <summary>
        /// Intensity of AO effect
        /// </summary>
        public float Power = 1.0f;

        /// <summary>
        /// Intensity of far occlusion - higher makes AO more pronounced further from occluding object
        /// </summary>
        public float Presence = 0.1f;

        /// <summary>
        /// Number of samples to use - 2,4,8,16,32 or 64.
        /// In case of adaptive sampling, this is maximum number of samples that will be used.
        /// </summary>
        public int Quality = 16;

        #region Near/Far Occlusion Limits

        /// <summary>
        /// When enabled, radius is limited to uper bound in screen space units to save performance
        /// </summary>
        public bool MaxRadiusEnabled = true;

        /// <summary>
        /// Maximal radius limit wrt. to screen size.
        /// Should be set so that large AO radius for objects close to camera will not cause performance drop
        /// </summary>
        public float MaxRadius = 0.4f;

        /// <summary>
        /// Mode for distance suppression of AO. Relative mode lets user set distance in units relative to far plane.
        /// Absolute mode uses world space units.
        /// </summary>
        public DistanceFalloffModeType DistanceFalloffMode = DistanceFalloffModeType.Off;

        /// <summary>
        /// Distance from which will be AO reduced (in world space units)
        /// </summary>
        public float DistanceFalloffStartAbsolute = 100.0f;

        /// <summary>
        /// Distance from which will be AO reduced (relative to far plane)
        /// </summary>
        public float DistanceFalloffStartRelative = 0.1f;

        /// <summary>
        /// Distance from "falloff start" in which is AO reduced  to zero (in world space units)
        /// </summary>
        public float DistanceFalloffSpeedAbsolute = 30.0f;

        /// <summary>
        /// Distance from "falloff start" in which is AO reduced  to zero (relative to far plane)
        /// </summary>
        public float DistanceFalloffSpeedRelative = 0.1f;

        #endregion

        #region Performance Settings

        /// <summary>
        /// Adaptive sampling setting - off/auto/manual.
        /// Automatic decides nubmer of samples automatically, manual mode takes user parameter
        /// AdaptiveQualityCoefficient into account.
        /// </summary>
        public AdaptiveSamplingType AdaptiveType = AdaptiveSamplingType.EnabledAutomatic;

        /// <summary>
        /// Moves threshold where number of samples starts to decrease due to distance from camera
        /// </summary>
        public float AdaptiveQualityCoefficient = 1.0f;

        /// <summary>
        /// Culling prepass setting - off/greeddy/careful.
        /// Greedy will not calculate AO in areas where no occlusion is estimated.
        /// Careful will use only 4 samples in such areas to prevent loss of detail.
        /// </summary>
        public CullingPrepassModeType CullingPrepassMode = CullingPrepassModeType.Careful;

        /// <summary>
        /// AO resolution downsampling - possible values are: 1 (original resolution), 2 or 4.
        /// </summary>
        public int Downsampling = 1;

        /// <summary>
        /// Setting of hierarchical buffer - off/on/auto.
        /// Auto will turn hierarchical buffer on when radius is large enough based on HierarchicalBufferPixelsPerLevel variable
        /// </summary>
        public HierarchicalBufferStateType HierarchicalBufferState = HierarchicalBufferStateType.Auto;

        #endregion

        #region Rendering Pipeline

        /// <summary>
        /// Enable to use command buffer pipeline instead of image effect implementation
        /// </summary>
        public bool CommandBufferEnabled = true;

        /// <summary>
        /// Enable to use GBuffer normals and depth (can provide higher precision, but available only in deferred rendering path)
        /// </summary>
        public bool UseGBuffer = true;

        /// <summary>
        /// Use higher precision dedicated depth buffer. Can solve problems with some see-thorugh materials in deferred rendering path.
        /// </summary>
        public bool UsePreciseDepthBuffer = true;

        /// <summary>
        /// Rendering stage in which will be VAO effect applied (available only in deferred path)
        /// </summary>
        public VAOCameraEventType VaoCameraEvent = VAOCameraEventType.AfterLighting;

        /// <summary>
        /// Where to take far plane distance from. Either from camera property or from Unity's built-in shader variable.
        /// </summary>
        public FarPlaneSourceType FarPlaneSource = FarPlaneSourceType.Camera;

        #endregion

        #region Luminance Sensitivity

        /// <summary>
        /// Enable luminance sensitivity - suppresion of occlusion based on luminance of shaded surface.
        /// </summary>
        public bool IsLumaSensitive = false;

        /// <summary>
        /// Luminance calculation formula - weighted RGB components (luma) or value component of HSV model.
        /// </summary>
        public LuminanceModeType LuminanceMode = LuminanceModeType.Luma;

        /// <summary>
        /// Threshold of luminance where suppresion is applied
        /// </summary>
        public float LumaThreshold = 0.7f;

        /// <summary>
        /// Controls width of gradual suppresion by luminance
        /// </summary>
        public float LumaKneeWidth = 0.3f;

        /// <summary>
        /// Controls shape of luminance suppresion curve
        /// </summary>
        public float LumaKneeLinearity = 3.0f;

        #endregion

        /// <summary>
        /// Effect Mode - simple (black occlusion), color tint, or colorbleeding
        /// </summary>
        public EffectMode Mode = EffectMode.ColorTint;

        /// <summary>
        /// Color tint applied to occlusion in ColorTint mode
        /// </summary>
        public Color ColorTint = Color.black;

        #region Color Bleeding Settings

        /// <summary>
        /// Intensity of color bleeding
        /// </summary>
        public float ColorBleedPower = 5.0f;

        /// <summary>
        /// Limits saturation of colorbleeding
        /// </summary>
        public float ColorBleedPresence = 1.0f;

        /// <summary>
        /// Format of texture used to store screen image on GPU
        /// </summary>
        public ScreenTextureFormat IntermediateScreenTextureFormat = ScreenTextureFormat.Auto;

        /// <summary>
        /// Enables suppresion of color bleeding from surfaces with same color (hue and saturation filter)
        /// </summary>
        public bool ColorbleedHueSuppresionEnabled = false;

        /// <summary>
        /// Tolerance range for hue filter
        /// </summary>
        public float ColorBleedHueSuppresionThreshold = 7.0f;

        /// <summary>
        /// Hue filter softness
        /// </summary>
        public float ColorBleedHueSuppresionWidth = 2.0f;

        /// <summary>
        /// Tolerance range for saturation filter
        /// </summary>
        public float ColorBleedHueSuppresionSaturationThreshold = 0.5f;

        /// <summary>
        /// Saturation filter softness
        /// </summary>
        public float ColorBleedHueSuppresionSaturationWidth = 0.2f;

        /// <summary>
        /// Limits brightness of self color bleeding
        /// </summary>
        public float ColorBleedHueSuppresionBrightness = 0.0f;

        /// <summary>
        /// Quality of color bleed: 1 - full quality, 2 - half of AO samples, 4 - quarter of AO samples
        /// </summary>
        public int ColorBleedQuality = 2;

        /// <summary>
        /// Controls how strong is suppresion of self-illumination by color bleeding
        /// </summary>
        public ColorBleedSelfOcclusionFixLevelType ColorBleedSelfOcclusionFixLevel = ColorBleedSelfOcclusionFixLevelType.Hard;

        /// <summary>
        /// Enables colorbleeding backfaces (faces behind illuminated surface)
        /// </summary>
        public bool GiBackfaces = false;

        #endregion

        #region Blur Settings

        /// <summary>
        /// Quality of blur step
        /// </summary>
        public BlurQualityType BlurQuality = VAOEffectCommandBuffer.BlurQualityType.Precise;

        /// <summary>
        /// Blur type: standard - 3x3 uniform weighted blur or enhanced - variable size multi-pass blur
        /// </summary>
        public BlurModeType BlurMode = VAOEffectCommandBuffer.BlurModeType.Enhanced;

        /// <summary>
        /// Size of enhanced blur in pixels
        /// </summary>
        public int EnhancedBlurSize = 5;

        /// <summary>
        /// Sharpnesss of enhanced blur (deviation of bell curve used for weighting blur samples)
        /// </summary>
        public float EnhancedBlurDeviation = 1.8f;

        #endregion

        /// <summary>
        /// Will draw only AO component to screen for debugging and fine-tuning purposes
        /// </summary>
        public bool OutputAOOnly = false;

        #endregion

        #region VAO Enums

        public enum EffectMode
        {
            Simple = 1,
            ColorTint = 2,
            ColorBleed = 3
        }

        public enum LuminanceModeType
        {
            Luma = 1,
            HSVValue = 2
        }

        public enum GiBlurAmmount
        {
            Auto = 1,
            Less = 2,
            More = 3
        }

        public enum CullingPrepassModeType
        {
            Off = 0,
            Greedy = 1,
            Careful = 2
        }

        public enum AdaptiveSamplingType
        {
            Disabled = 0,
            EnabledAutomatic = 1,
            EnabledManual = 2
        }

        public enum BlurModeType
        {
            Disabled = 0,
            Basic = 1,
            Enhanced = 2
        }

        public enum BlurQualityType
        {
            Fast = 0,
            Precise = 1
        }

        public enum ColorBleedSelfOcclusionFixLevelType
        {
            Off = 0,
            Soft = 1,
            Hard = 2
        }

        public enum ScreenTextureFormat
        {
            Auto,
            ARGB32,
            ARGBHalf,
            ARGBFloat,
            Default,
            DefaultHDR
        }

        public enum FarPlaneSourceType
        {
            ProjectionParams,
            Camera
        }

        public enum DistanceFalloffModeType
        {
            Off = 0,
            Absolute = 1,
            Relative = 2
        }

        public enum VAOCameraEventType
        {
            AfterLighting, //< Apply To Lighting
            BeforeReflections, //< Apply To AO buffer and Lighting
            BeforeImageEffectsOpaque, //< Apply to Final Image
        }

        public enum HierarchicalBufferStateType
        {
            Off = 0,
            On = 1,
            Auto = 2
        }

        private enum ShaderPass
        {
            CullingPrepass = 0,
            MainPass = 1,
            StandardBlurUniform = 2,
            StandardBlurUniformMultiplyBlend = 3,
            StandardBlurUniformFast = 4,
            StandardBlurUniformFastMultiplyBlend = 5,
            EnhancedBlurFirstPass = 6,
            EnhancedBlurSecondPass = 7,
            EnhancedBlurSecondPassMultiplyBlend = 8,
            Mixing = 9,
            MixingMultiplyBlend = 10,
            BlendBeforeReflections = 11,
            BlendBeforeReflectionsLog = 12,
            DownscaleDepthNormalsPass = 13,
            Copy = 14,
            BlendAfterLightingLog = 15
        }

        #endregion

        #region VAO Private Variables

        #region Performance Optimizations Settings

        /// <summary>
        /// This variable controls when will automatic control turn on hierarchical buffers.
        /// Lowering this number will cause turning on hierarchical buffers for lower radius settings.
        /// </summary>
        public float HierarchicalBufferPixelsPerLevel = 150.0f;

        private int CullingPrepassDownsamplingFactor = 8;

        private float AdaptiveQuality = 0.2f;
        private float AdaptiveMin = 0.0f;
        private float AdaptiveMax = -10.0f;

        #endregion

        #region Command Buffer Variables

        private Dictionary<CameraEvent, CommandBuffer> cameraEventsRegistered = new Dictionary<CameraEvent, CommandBuffer>();
        private bool isCommandBufferAlive = false;

        private Mesh screenQuad;

        private int destinationWidth;
        private int destinationHeight;

        private bool onDestroyCalled = false;

        #endregion

        #region Shader, Material, Camera

        public Shader vaoShader;

        private Camera myCamera = null;
        private bool isSupported;
        private Material VAOMaterial;

        #endregion

        #region Warning Flags

        public bool ForcedSwitchPerformed = false;
        public bool ForcedSwitchPerformedSinglePassStereo = false;
        public bool ForcedSwitchPerformedSinglePassStereoGBuffer = false;
        
        #endregion

        #region Previous controls values

        private int lastDownsampling;
        private CullingPrepassModeType lastcullingPrepassType;
        private int lastCullingPrepassDownsamplingFactor;
        private BlurModeType lastBlurMode;
        private BlurQualityType lastBlurQuality;
        private EffectMode lastMode;
        private bool lastUseGBuffer;
        private bool lastOutputAOOnly;
        private CameraEvent lastCameraEvent;
        private bool lastIsHDR;
        private bool lastIsSPSR;
        private bool isHDR;
        public bool isSPSR;
        private ScreenTextureFormat lastIntermediateScreenTextureFormat;
        private int lastCmdBufferEnhancedBlurSize;
        private bool lastHierarchicalBufferEnabled = false;

        #endregion

        #region VAO Private Data

        private Texture2D noiseTexture;

        private Vector4[] adaptiveSamples = null;
        private Vector4[] carefulCache = null;

        private Vector4[] gaussian = null;
        private Vector4[] gaussianBuffer = new Vector4[17];

        // To prevent error with capping of large array to smaller size in Unity 5.4 - always use largest array filled with trailing zeros.
        private Vector4[] samplesLarge = new Vector4[70];
        int lastSamplesLength = 0;

        private int lastEnhancedBlurSize = 0;

        private float gaussianWeight = 0.0f;
        private float lastDeviation = 0.5f;


        #endregion

        #endregion

        #region Unity Events

        void Start()
        {
            if (vaoShader == null) vaoShader = Shader.Find("Hidden/Wilberforce/VAOShader");

            if (vaoShader == null)
            {
                ReportError("Could not locate VAO Shader. Make sure there is 'VAOShader.shader' file added to the project.");
                isSupported = false;
                enabled = false;
                return;
            }

            if (!SystemInfo.supportsImageEffects || !SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.Depth) || SystemInfo.graphicsShaderLevel < 30)
            {
                if (!SystemInfo.supportsImageEffects) ReportError("System does not support image effects.");
                if (!SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.Depth)) ReportError("System does not support depth texture.");
                if (SystemInfo.graphicsShaderLevel < 30) ReportError("This effect needs at least Shader Model 3.0.");

                isSupported = false;
                enabled = false;
                return;
            }

            EnsureMaterials();

            if (!VAOMaterial || VAOMaterial.passCount != Enum.GetValues(typeof(ShaderPass)).Length)
            {
                ReportError("Could not create shader.");
                isSupported = false;
                enabled = false;
                return;
            }

            EnsureNoiseTexture();

            if (adaptiveSamples == null) adaptiveSamples = GenerateAdaptiveSamples();

            isSupported = true;
        }

        void OnEnable()
        {
            this.myCamera = GetComponent<Camera>();
            TeardownCommandBuffer();

            // See if there is post processing stuck
#if UNITY_EDITOR
            if (myCamera != null && (CommandBufferEnabled == false || myCamera.actualRenderingPath == RenderingPath.DeferredShading))
            {
                try
                {
#if !UNITY_2017_1_OR_NEWER
                    System.Reflection.Assembly asm = System.Reflection.Assembly.GetExecutingAssembly();
                    Type postStackType = asm.GetType("UnityEngine.PostProcessing.PostProcessingBehaviour");
                    var postStack = GetComponent(postStackType);
                    if (postStack != null)
                    {
                        if (!ForcedSwitchPerformed)
                        {
                            ReportWarning("Post Processing Stack Detected! Switching to command buffer pipeline and GBuffer inputs!");
                            CommandBufferEnabled = true;
                            UseGBuffer = true;
                            ForcedSwitchPerformed = true;
                        }
                    }
#endif
                }
                catch (Exception) { }

                // See if we are in single pass rendering
#if UNITY_5_5_OR_NEWER
                if (myCamera.stereoEnabled && PlayerSettings.stereoRenderingPath == StereoRenderingPath.SinglePass)
                {
                    if (CommandBufferEnabled == false)
                    {
                        if (ForcedSwitchPerformedSinglePassStereo)
                        {
                            ReportWarning("You are running in single pass stereo mode! We recommend switching to command buffer pipeline if you encounter black screen problems.");
                        }
                        else
                        {
                            ReportWarning("You are running in single pass stereo mode! Switching to command buffer pipeline (recommended setting)!");
                            CommandBufferEnabled = true;
                            ForcedSwitchPerformedSinglePassStereo = true;
                        }
                    }

                }
#endif

#if UNITY_2017_1_OR_NEWER
                if (myCamera.stereoEnabled
                    && PlayerSettings.stereoRenderingPath == StereoRenderingPath.SinglePass
                    && myCamera.actualRenderingPath == RenderingPath.DeferredShading)
                {
                    if (!ForcedSwitchPerformedSinglePassStereoGBuffer)
                    {
                        UseGBuffer = true;
                        ForcedSwitchPerformedSinglePassStereoGBuffer = true;
                    }
                }
#endif
            }
#endif

        }

        void OnValidate()
        {
            // Force parameters to be positive
            Radius = Mathf.Clamp(Radius, 0.001f, float.MaxValue);
            Power = Mathf.Clamp(Power, 0, float.MaxValue);
        }

        void OnPreRender()
        {
            EnsureVAOVersion();

            bool forceDepthTexture = false;
            bool forceDepthNormalsTexture = false;

            DepthTextureMode currentMode = myCamera.depthTextureMode;
            if (myCamera.actualRenderingPath == RenderingPath.DeferredShading && UseGBuffer)
            {
                forceDepthTexture = true;
            }
            else
            {
                forceDepthNormalsTexture = true;
            }

            if (UsePreciseDepthBuffer && (myCamera.actualRenderingPath == RenderingPath.Forward || myCamera.actualRenderingPath == RenderingPath.VertexLit))
            {
                forceDepthTexture = true;
                forceDepthNormalsTexture = true;

            }

            if (forceDepthTexture)
            {
                if ((currentMode & DepthTextureMode.Depth) != DepthTextureMode.Depth)
                {
                    myCamera.depthTextureMode |= DepthTextureMode.Depth;
                }
            }

            if (forceDepthNormalsTexture)
            {
                if ((currentMode & DepthTextureMode.DepthNormals) != DepthTextureMode.DepthNormals)
                {
                    myCamera.depthTextureMode |= DepthTextureMode.DepthNormals;
                }
            }

            EnsureMaterials();
            EnsureNoiseTexture();

            TrySetUniforms();
            EnsureCommandBuffer(CheckSettingsChanges());
        }

        void OnPostRender()
        {

#if UNITY_5_6_OR_NEWER

            if (myCamera == null || myCamera.activeTexture == null) return;

            // Check if cmd. buffer was created with correct target texture sizes and rebuild if necessary
            if (this.destinationWidth != myCamera.activeTexture.width || this.destinationHeight != myCamera.activeTexture.height || !isCommandBufferAlive)
            {
                this.destinationWidth = myCamera.activeTexture.width;
                this.destinationHeight = myCamera.activeTexture.height;

                TeardownCommandBuffer();
                EnsureCommandBuffer();
            }
            else
            {
                // Remember destination texture dimensions for use in command buffer (there are different values in camera.pixelWidth/Height which do not work in Single pass stereo)
                this.destinationWidth = myCamera.activeTexture.width;
                this.destinationHeight = myCamera.activeTexture.height;
            }
#endif

        }

        void OnDisable()
        {
            TeardownCommandBuffer();
        }

        void OnDestroy()
        {
            TeardownCommandBuffer();
            onDestroyCalled = true;
        }

        #endregion

        #region Image Effect Implementation

        //[ImageEffectOpaque]
        protected void PerformOnRenderImage(RenderTexture source, RenderTexture destination)
        {
            if (!isSupported || !vaoShader.isSupported)
            {
                enabled = false;
                return;
            }

            if (CommandBufferEnabled)
            {
                return; //< Return here, drawing will be done in command buffer
            }
            else
            {
                TeardownCommandBuffer();
            }

            int screenTextureWidth = source.width / Downsampling;
            int screenTextureHeight = source.height / Downsampling;

            RenderTexture downscaled2Texture = null;
            RenderTexture downscaled4Texture = null;

            if (HierarchicalBufferEnabled)
            {
                RenderTextureFormat hBufferFormat = RenderTextureFormat.RHalf;
                if (Mode == EffectMode.ColorBleed) hBufferFormat = RenderTextureFormat.ARGBHalf;

                downscaled2Texture = RenderTexture.GetTemporary(source.width / 2, source.height / 2, 0, hBufferFormat);
                downscaled2Texture.filterMode = FilterMode.Bilinear;
                downscaled4Texture = RenderTexture.GetTemporary(source.width / 4, source.height / 4, 0, hBufferFormat);
                downscaled4Texture.filterMode = FilterMode.Bilinear;

                Graphics.Blit(null, downscaled2Texture, VAOMaterial, (int)ShaderPass.DownscaleDepthNormalsPass);
                Graphics.Blit(downscaled2Texture, downscaled4Texture);

                if (downscaled2Texture != null) VAOMaterial.SetTexture("depthNormalsTexture2", downscaled2Texture);
                if (downscaled4Texture != null) VAOMaterial.SetTexture("depthNormalsTexture4", downscaled4Texture);
            }

            // Create temporary texture for AO
            RenderTextureFormat aoTextureFormat = RenderTextureFormat.RGHalf;

            if (Mode == EffectMode.ColorBleed)
            {
                aoTextureFormat = isHDR ? RenderTextureFormat.DefaultHDR : RenderTextureFormat.Default;
            }

            RenderTexture vaoTexture = RenderTexture.GetTemporary(screenTextureWidth, screenTextureHeight, 0, aoTextureFormat);
            vaoTexture.filterMode = FilterMode.Bilinear;

            VAOMaterial.SetTexture("noiseTexture", noiseTexture);

            // Culling pre-pass
            RenderTexture cullingPrepassTexture = null;
            RenderTexture cullingPrepassTextureHalfRes = null;
            if (CullingPrepassMode != CullingPrepassModeType.Off)
            {
                RenderTextureFormat prepassFormat = RenderTextureFormat.R8;

                cullingPrepassTexture = RenderTexture.GetTemporary(source.width / CullingPrepassDownsamplingFactor, source.height / CullingPrepassDownsamplingFactor, 0, prepassFormat);
                cullingPrepassTexture.filterMode = FilterMode.Bilinear;
                cullingPrepassTextureHalfRes = RenderTexture.GetTemporary(source.width / (CullingPrepassDownsamplingFactor * 2), source.height / (CullingPrepassDownsamplingFactor * 2), 0, prepassFormat);
                cullingPrepassTextureHalfRes.filterMode = FilterMode.Bilinear;

                Graphics.Blit(source, cullingPrepassTexture, VAOMaterial, (int)ShaderPass.CullingPrepass);
                Graphics.Blit(cullingPrepassTexture, cullingPrepassTextureHalfRes);
            }

            // Main pass
            if (cullingPrepassTextureHalfRes != null) VAOMaterial.SetTexture("cullingPrepassTexture", cullingPrepassTextureHalfRes);

            Graphics.Blit(source, vaoTexture, VAOMaterial, (int)ShaderPass.MainPass);
            VAOMaterial.SetTexture("textureAO", vaoTexture);

            if (BlurMode != BlurModeType.Disabled)
            {
                int blurTextureWidth = source.width;
                int blurTextureHeight = source.height;

                if (BlurQuality == BlurQualityType.Fast)
                {
                    blurTextureHeight /= 2;
                }

                // Blur pass
                if (BlurMode == BlurModeType.Enhanced)
                {
                    RenderTexture tempTexture = RenderTexture.GetTemporary(blurTextureWidth, blurTextureHeight, 0, aoTextureFormat);
                    tempTexture.filterMode = FilterMode.Bilinear;

                    Graphics.Blit(null, tempTexture, VAOMaterial, (int)ShaderPass.EnhancedBlurFirstPass);

                    VAOMaterial.SetTexture("textureAO", tempTexture);
                    Graphics.Blit(source, destination, VAOMaterial, (int)ShaderPass.EnhancedBlurSecondPass);

                    RenderTexture.ReleaseTemporary(tempTexture);
                }
                else
                {
                    int uniformBlurPass = (BlurQuality == BlurQualityType.Fast) ? (int)ShaderPass.StandardBlurUniformFast : (int)ShaderPass.StandardBlurUniform;

                    Graphics.Blit(source, destination, VAOMaterial, uniformBlurPass);
                }
            }
            else
            {
                // Mixing pass
                Graphics.Blit(source, destination, VAOMaterial, (int)ShaderPass.Mixing);
            }

            // Cleanup
            if (vaoTexture != null) RenderTexture.ReleaseTemporary(vaoTexture);
            if (cullingPrepassTexture != null) RenderTexture.ReleaseTemporary(cullingPrepassTexture);
            if (cullingPrepassTextureHalfRes != null) RenderTexture.ReleaseTemporary(cullingPrepassTextureHalfRes);
            if (downscaled2Texture != null) RenderTexture.ReleaseTemporary(downscaled2Texture);
            if (downscaled4Texture != null) RenderTexture.ReleaseTemporary(downscaled4Texture);
        }

        #endregion

        #region Command Buffer Implementation

        private void EnsureCommandBuffer(bool settingsDirty = false)
        {
            if ((!settingsDirty && isCommandBufferAlive) || !CommandBufferEnabled) return;
            if (onDestroyCalled) return;

            try
            {
                CreateCommandBuffer();
                lastCameraEvent = GetCameraEvent(VaoCameraEvent);
                isCommandBufferAlive = true;
            }
            catch (Exception ex)
            {
                ReportError("There was an error while trying to create command buffer. " + ex.Message);
            }
        }

        private void CreateCommandBuffer()
        {
            CommandBuffer commandBuffer;

            VAOMaterial = null;
            EnsureMaterials();

            TrySetUniforms();

            CameraEvent cameraEvent = GetCameraEvent(VaoCameraEvent);

            if (cameraEventsRegistered.TryGetValue(cameraEvent, out commandBuffer))
            {
                commandBuffer.Clear();
            }
            else
            {
                commandBuffer = new CommandBuffer();
                myCamera.AddCommandBuffer(cameraEvent, commandBuffer);

                commandBuffer.name = "Volumetric Ambient Occlusion";

                // Register
                cameraEventsRegistered[cameraEvent] = commandBuffer;
            }

            bool isHwBlending = (!OutputAOOnly && Mode != EffectMode.ColorBleed);

            RenderTargetIdentifier targetTexture = BuiltinRenderTextureType.CameraTarget;
            int? emissionTexture = null;
            int? occlusionTexture = null;

            int cameraWidth = this.destinationWidth;
            int cameraHeight = this.destinationHeight;

            if (cameraWidth <= 0) cameraWidth = myCamera.pixelWidth;
            if (cameraHeight <= 0) cameraHeight = myCamera.pixelHeight;

            int screenTextureWidth = cameraWidth / Downsampling;
            int screenTextureHeight = cameraHeight / Downsampling;

            if (!OutputAOOnly)
            {
                if (!isHDR)
                {
                    if (cameraEvent == CameraEvent.AfterLighting || cameraEvent == CameraEvent.BeforeReflections)
                    {
                        targetTexture = BuiltinRenderTextureType.GBuffer3; //< Emission/lighting buffer

                        emissionTexture = Shader.PropertyToID("emissionTextureRT");
                        commandBuffer.GetTemporaryRT(emissionTexture.Value, cameraWidth, cameraHeight, 0, FilterMode.Bilinear, RenderTextureFormat.ARGB2101010, RenderTextureReadWrite.Linear);

                        // Make a copy of emission buffer for blending
                        commandBuffer.Blit(BuiltinRenderTextureType.GBuffer3, emissionTexture.Value, VAOMaterial, (int)ShaderPass.Copy);

                        commandBuffer.SetGlobalTexture("emissionTexture", emissionTexture.Value);

                        isHwBlending = false;
                    }
                }

                if (cameraEvent == CameraEvent.BeforeReflections || (cameraEvent == CameraEvent.AfterLighting && !isHDR && isSPSR))
                {
                    occlusionTexture = Shader.PropertyToID("occlusionTextureRT");
                    commandBuffer.GetTemporaryRT(occlusionTexture.Value, cameraWidth, cameraHeight, 0, FilterMode.Bilinear, RenderTextureFormat.RHalf, RenderTextureReadWrite.Linear);
                    commandBuffer.SetGlobalTexture("occlusionTexture", occlusionTexture.Value);

                    isHwBlending = false;
                }
            }

            int? screenTexture = null;
            if (Mode == EffectMode.ColorBleed)
            {
                RenderTextureFormat screenTextureFormat = GetRenderTextureFormat(IntermediateScreenTextureFormat, isHDR);

                screenTexture = Shader.PropertyToID("screenTextureRT");
                commandBuffer.GetTemporaryRT(screenTexture.Value, cameraWidth, cameraHeight, 0, FilterMode.Bilinear, screenTextureFormat, RenderTextureReadWrite.Linear);

                // Remember input
                commandBuffer.Blit(BuiltinRenderTextureType.CameraTarget, screenTexture.Value);
            }

            int vaoTexture = Shader.PropertyToID("vaoTextureRT");
            RenderTextureFormat aoTextureFormat = RenderTextureFormat.RGHalf;

            if (Mode == EffectMode.ColorBleed)
            {
                aoTextureFormat = isHDR ? RenderTextureFormat.DefaultHDR : RenderTextureFormat.Default;
            }

            commandBuffer.GetTemporaryRT(vaoTexture, screenTextureWidth, screenTextureHeight, 0, FilterMode.Bilinear, aoTextureFormat, RenderTextureReadWrite.Linear);

            int? cullingPrepassTexture = null;
            int? cullingPrepassTextureHalfRes = null;

            // Prepare hierarchical buffers
            int? downscaled2Texture = null;
            int? downscaled4Texture = null;

            if (HierarchicalBufferEnabled)
            {
                RenderTextureFormat hBufferFormat = RenderTextureFormat.RHalf;
                if (Mode == EffectMode.ColorBleed) hBufferFormat = RenderTextureFormat.ARGBHalf;

                downscaled2Texture = Shader.PropertyToID("downscaled2TextureRT");
                downscaled4Texture = Shader.PropertyToID("downscaled4TextureRT");
                commandBuffer.GetTemporaryRT(downscaled2Texture.Value, cameraWidth / 2, cameraHeight / 2, 0, FilterMode.Bilinear, hBufferFormat, RenderTextureReadWrite.Linear);
                commandBuffer.GetTemporaryRT(downscaled4Texture.Value, cameraWidth / 4, cameraHeight / 4, 0, FilterMode.Bilinear, hBufferFormat, RenderTextureReadWrite.Linear);

                commandBuffer.Blit((Texture)null, downscaled2Texture.Value, VAOMaterial, (int)ShaderPass.DownscaleDepthNormalsPass);
                commandBuffer.Blit(downscaled2Texture.Value, downscaled4Texture.Value);

                if (downscaled2Texture != null) commandBuffer.SetGlobalTexture("depthNormalsTexture2", downscaled2Texture.Value);
                if (downscaled4Texture != null) commandBuffer.SetGlobalTexture("depthNormalsTexture4", downscaled4Texture.Value);
            }

            // Culling pre-pass
            if (CullingPrepassMode != CullingPrepassModeType.Off)
            {
                cullingPrepassTexture = Shader.PropertyToID("cullingPrepassTextureRT");
                cullingPrepassTextureHalfRes = Shader.PropertyToID("cullingPrepassTextureHalfResRT");

                RenderTextureFormat prepassFormat = RenderTextureFormat.R8;

                commandBuffer.GetTemporaryRT(cullingPrepassTexture.Value, screenTextureWidth / CullingPrepassDownsamplingFactor, screenTextureHeight / CullingPrepassDownsamplingFactor, 0, FilterMode.Bilinear, prepassFormat, RenderTextureReadWrite.Linear);
                commandBuffer.GetTemporaryRT(cullingPrepassTextureHalfRes.Value, screenTextureWidth / (CullingPrepassDownsamplingFactor * 2), screenTextureHeight / (CullingPrepassDownsamplingFactor * 2), 0, FilterMode.Bilinear, prepassFormat, RenderTextureReadWrite.Linear);

                if (Mode == EffectMode.ColorBleed)
                {
                    commandBuffer.Blit(screenTexture.Value, cullingPrepassTexture.Value, VAOMaterial, (int)ShaderPass.CullingPrepass);
                }
                else
                {
                    commandBuffer.Blit(targetTexture, cullingPrepassTexture.Value, VAOMaterial, (int)ShaderPass.CullingPrepass);
                }
                commandBuffer.Blit(cullingPrepassTexture.Value, cullingPrepassTextureHalfRes.Value);

                commandBuffer.SetGlobalTexture("cullingPrepassTexture", cullingPrepassTextureHalfRes.Value);
            }

            // Main pass
#if UNITY_5_4_OR_NEWER
            commandBuffer.SetGlobalTexture("noiseTexture", noiseTexture);
#else
            VAOMaterial.SetTexture("noiseTexture", noiseTexture);
#endif

            if (Mode== EffectMode.ColorBleed)
            {
                commandBuffer.Blit(screenTexture.Value, vaoTexture, VAOMaterial, (int)ShaderPass.MainPass);
            }
            else
            {
                commandBuffer.Blit(targetTexture, vaoTexture, VAOMaterial, (int)ShaderPass.MainPass);
            }

            commandBuffer.SetGlobalTexture("textureAO", vaoTexture);

            if (BlurMode != BlurModeType.Disabled)
            {
                int blurTextureWidth = cameraWidth;
                int blurTextureHeight = cameraHeight;

                if (BlurQuality == BlurQualityType.Fast)
                {
                    blurTextureHeight /= 2;
                }

                // Blur pass
                if (BlurMode == BlurModeType.Enhanced)
                {
                    int tempTexture = Shader.PropertyToID("tempTextureRT");
                    commandBuffer.GetTemporaryRT(tempTexture, blurTextureWidth, blurTextureHeight, 0, FilterMode.Bilinear, aoTextureFormat, RenderTextureReadWrite.Linear);

                    commandBuffer.Blit(null, tempTexture, VAOMaterial, (int)ShaderPass.EnhancedBlurFirstPass);
                    commandBuffer.SetGlobalTexture("textureAO", tempTexture);

                    DoMixingBlit(commandBuffer, screenTexture, occlusionTexture, targetTexture, isHwBlending ? (int)ShaderPass.EnhancedBlurSecondPassMultiplyBlend : (int)ShaderPass.EnhancedBlurSecondPass);

                    commandBuffer.ReleaseTemporaryRT(tempTexture);

                }
                else
                {
                    int uniformBlurPass = (BlurQuality == BlurQualityType.Fast) ? (int)ShaderPass.StandardBlurUniformFast : (int)ShaderPass.StandardBlurUniform;
                    int uniformBlurPassBlend = (BlurQuality == BlurQualityType.Fast) ? (int)ShaderPass.StandardBlurUniformFastMultiplyBlend : (int)ShaderPass.StandardBlurUniformMultiplyBlend;

                    DoMixingBlit(commandBuffer, screenTexture, occlusionTexture, targetTexture, isHwBlending ? uniformBlurPassBlend : uniformBlurPass);
                }
            }
            else
            {
                // Mixing pass
                DoMixingBlit(commandBuffer, screenTexture, occlusionTexture, targetTexture, isHwBlending ? (int)ShaderPass.MixingMultiplyBlend : (int)ShaderPass.Mixing);
            }

            if (cameraEvent == CameraEvent.BeforeReflections)
            {

                commandBuffer.SetRenderTarget(new RenderTargetIdentifier[]
                {
                    BuiltinRenderTextureType.GBuffer0,
                    targetTexture
                }, BuiltinRenderTextureType.GBuffer0);

                commandBuffer.DrawMesh(GetScreenQuad(), Matrix4x4.identity, VAOMaterial, 0, isHDR ? (int)ShaderPass.BlendBeforeReflections : (int)ShaderPass.BlendBeforeReflectionsLog);
            }
            else if (cameraEvent == CameraEvent.AfterLighting && !isHDR && isSPSR)
            {
                commandBuffer.SetRenderTarget(targetTexture);
                commandBuffer.DrawMesh(GetScreenQuad(), Matrix4x4.identity, VAOMaterial, 0, (int)ShaderPass.BlendAfterLightingLog);
            }

            // Cleanup
            commandBuffer.ReleaseTemporaryRT(vaoTexture);
            if (screenTexture != null) commandBuffer.ReleaseTemporaryRT(screenTexture.Value);
            if (emissionTexture != null) commandBuffer.ReleaseTemporaryRT(emissionTexture.Value);
            if (occlusionTexture != null) commandBuffer.ReleaseTemporaryRT(occlusionTexture.Value);
            if (cullingPrepassTexture != null) commandBuffer.ReleaseTemporaryRT(cullingPrepassTexture.Value);
            if (cullingPrepassTextureHalfRes != null) commandBuffer.ReleaseTemporaryRT(cullingPrepassTextureHalfRes.Value);
            if (downscaled2Texture != null) commandBuffer.ReleaseTemporaryRT(downscaled2Texture.Value);
            if (downscaled4Texture != null) commandBuffer.ReleaseTemporaryRT(downscaled4Texture.Value);
        }

        private void TeardownCommandBuffer()
        {
            if (!isCommandBufferAlive) return;

            try
            {
                isCommandBufferAlive = false;

                if (myCamera != null)
                {
                    foreach (var e in cameraEventsRegistered)
                    {
                        myCamera.RemoveCommandBuffer(e.Key, e.Value);
                    }
                }

                cameraEventsRegistered.Clear();
                VAOMaterial = null;
                EnsureMaterials();
            }
            catch (Exception ex)
            {
                ReportError("There was an error while trying to destroy command buffer. " + ex.Message);
            }
        }

        #region Command Buffer Utilities

        protected Mesh GetScreenQuad()
        {
            if (screenQuad == null)
            {
                screenQuad = new Mesh()
                {
                    vertices = new Vector3[] { new Vector3(-1, -1, 0), new Vector3(-1, 1, 0), new Vector3(1, 1, 0), new Vector3(1, -1, 0) },
                    triangles = new int[] { 0, 1, 2, 0, 2, 3 },
                    uv = new Vector2[] { new Vector2(0, 1), new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1) }
                };
            }

            return screenQuad;
        }

        private CameraEvent GetCameraEvent(VAOCameraEventType vaoCameraEvent)
        {

            if (myCamera == null) return CameraEvent.BeforeImageEffectsOpaque;
            if (OutputAOOnly) return CameraEvent.BeforeImageEffectsOpaque;
            if (Mode == EffectMode.ColorBleed) return CameraEvent.BeforeImageEffectsOpaque;

            if (myCamera.actualRenderingPath != RenderingPath.DeferredShading)
            {
                return CameraEvent.BeforeImageEffectsOpaque;
            }

            switch (vaoCameraEvent)
            {
                case VAOCameraEventType.AfterLighting:
                    return CameraEvent.AfterLighting;
                case VAOCameraEventType.BeforeImageEffectsOpaque:
                    return CameraEvent.BeforeImageEffectsOpaque;
                case VAOCameraEventType.BeforeReflections:
                    return CameraEvent.BeforeReflections;
                default:
                    return CameraEvent.BeforeImageEffectsOpaque;
            }
        }

        protected void DoMixingBlit(CommandBuffer commandBuffer, int? source, int? primaryTarget, RenderTargetIdentifier secondaryTarget, int pass)
        {
            if (primaryTarget.HasValue)
                DoBlit(commandBuffer, source, primaryTarget.Value, pass);
            else
                DoBlit(commandBuffer, source, secondaryTarget, pass);
        }

        protected void DoBlit(CommandBuffer commandBuffer, int? source, int target, int pass)
        {
            if (source.HasValue)
                commandBuffer.Blit(source.Value, target, VAOMaterial, pass);
            else
                commandBuffer.Blit((Texture)null, target, VAOMaterial, pass);
        }

        protected void DoBlit(CommandBuffer commandBuffer, int? source, RenderTargetIdentifier target, int pass)
        {
            if (source.HasValue)
                commandBuffer.Blit(source.Value, target, VAOMaterial, pass);
            else
                commandBuffer.Blit((Texture)null, target, VAOMaterial, pass);
        }

        #endregion

        #endregion

        #region Shader Utilities

        private void TrySetUniforms()
        {
            if (VAOMaterial == null) return;

            int screenTextureWidth = myCamera.pixelWidth / Downsampling;
            int screenTextureHeight = myCamera.pixelHeight / Downsampling;

            Vector4[] samples = null;
            switch (Quality)
            {
                case 2:
                    VAOMaterial.SetInt("minLevelIndex", 15);
                    samples = samp2;
                    break;
                case 4:
                    VAOMaterial.SetInt("minLevelIndex", 7);
                    samples = samp4;
                    break;
                case 8:
                    VAOMaterial.SetInt("minLevelIndex", 3);
                    samples = samp8;
                    break;
                case 16:
                    VAOMaterial.SetInt("minLevelIndex", 1);
                    samples = samp16;
                    break;
                case 32:
                    VAOMaterial.SetInt("minLevelIndex", 0);
                    samples = samp32;
                    break;
                case 64:
                    VAOMaterial.SetInt("minLevelIndex", 0);
                    samples = samp64;
                    break;
                default:
                    ReportError("Unsupported quality setting " + Quality + " encountered. Reverting to low setting");
                    // Reverting to low
                    VAOMaterial.SetInt("minLevelIndex", 1);
                    Quality = 16;
                    samples = samp16;
                    break;
            }

            if (AdaptiveType != AdaptiveSamplingType.Disabled)
            {
                switch (Quality)
                {
                    case 64: AdaptiveQuality = 0.025f; break;
                    case 32: AdaptiveQuality = 0.025f; break;
                    case 16: AdaptiveQuality = 0.05f; break;
                    case 8: AdaptiveQuality = 0.1f; break;
                    case 4: AdaptiveQuality = 0.2f; break;
                    case 2: AdaptiveQuality = 0.4f; break;
                }
                if (AdaptiveType == AdaptiveSamplingType.EnabledManual)
                {
                    AdaptiveQuality *= AdaptiveQualityCoefficient;
                }
                else
                {
                    AdaptiveQualityCoefficient = 1.0f;
                }
            }

            AdaptiveMax = GetDepthForScreenSize(myCamera, AdaptiveQuality);

            Vector2 texelSize = new Vector2(1.0f / screenTextureWidth, 1.0f / screenTextureHeight);
            float subPixelDepth = GetDepthForScreenSize(myCamera, Mathf.Max(texelSize.x, texelSize.y));

            bool mustFixSrspAfterLighting = GetCameraEvent(VaoCameraEvent) == CameraEvent.AfterLighting && isSPSR && !isHDR;

            // Set shader uniforms
            VAOMaterial.SetMatrix("invProjMatrix", myCamera.projectionMatrix.inverse);
            VAOMaterial.SetVector("screenProjection", -0.5f * new Vector4(myCamera.projectionMatrix.m00, myCamera.projectionMatrix.m11, myCamera.projectionMatrix.m02, myCamera.projectionMatrix.m12));
            VAOMaterial.SetFloat("halfRadiusSquared", (Radius * 0.5f) * (Radius * 0.5f));
            VAOMaterial.SetFloat("halfRadius", Radius * 0.5f);
            VAOMaterial.SetFloat("radius", Radius);
            VAOMaterial.SetInt("sampleCount", Quality);
            VAOMaterial.SetInt("fourSamplesStartIndex", Quality);
            VAOMaterial.SetFloat("aoPower", Power);
            VAOMaterial.SetFloat("aoPresence", Presence);
            VAOMaterial.SetFloat("giPresence", 1.0f - ColorBleedPresence);
            VAOMaterial.SetFloat("LumaThreshold", LumaThreshold);
            VAOMaterial.SetFloat("LumaKneeWidth", LumaKneeWidth);
            VAOMaterial.SetFloat("LumaTwiceKneeWidthRcp", 1.0f / (LumaKneeWidth * 2.0f));
            VAOMaterial.SetFloat("LumaKneeLinearity", LumaKneeLinearity);
            VAOMaterial.SetInt("giBackfaces", GiBackfaces ? 0 : 1);
            VAOMaterial.SetFloat("adaptiveMin", AdaptiveMin);
            VAOMaterial.SetFloat("adaptiveMax", AdaptiveMax);
            VAOMaterial.SetVector("texelSize", (BlurMode == BlurModeType.Basic && BlurQuality == BlurQualityType.Fast) ? texelSize * 0.5f : texelSize);
            VAOMaterial.SetFloat("blurDepthThreshold", Radius);
            VAOMaterial.SetInt("cullingPrepassMode", (int)CullingPrepassMode);
            VAOMaterial.SetVector("cullingPrepassTexelSize", new Vector2(0.5f / (myCamera.pixelWidth / CullingPrepassDownsamplingFactor), 0.5f / (myCamera.pixelHeight / CullingPrepassDownsamplingFactor)));
            VAOMaterial.SetInt("giSelfOcclusionFix", (int)ColorBleedSelfOcclusionFixLevel);
            VAOMaterial.SetInt("adaptiveMode", (int)AdaptiveType);
            VAOMaterial.SetInt("LumaMode", (int)LuminanceMode);
            VAOMaterial.SetFloat("cameraFarPlane", myCamera.farClipPlane);
            VAOMaterial.SetInt("UseCameraFarPlane", FarPlaneSource == FarPlaneSourceType.Camera ? 1 : 0);
            VAOMaterial.SetFloat("maxRadiusEnabled", MaxRadiusEnabled ? 1 : 0);
            VAOMaterial.SetFloat("maxRadiusCutoffDepth", GetDepthForScreenSize(myCamera, MaxRadius));
            VAOMaterial.SetFloat("projMatrix11", myCamera.projectionMatrix.m11);
            VAOMaterial.SetFloat("maxRadiusOnScreen", MaxRadius);
            VAOMaterial.SetFloat("enhancedBlurSize", EnhancedBlurSize / 2);
            VAOMaterial.SetInt("flipY", MustForceFlip(myCamera) ? 1 : 0);
            VAOMaterial.SetInt("useGBuffer", ShouldUseGBuffer() ? 1 : 0);
            VAOMaterial.SetInt("hierarchicalBufferEnabled", HierarchicalBufferEnabled ? 1 : 0);
            VAOMaterial.SetInt("hwBlendingEnabled", (CommandBufferEnabled && Mode != EffectMode.ColorBleed) && GetCameraEvent(VaoCameraEvent) != CameraEvent.BeforeReflections && !mustFixSrspAfterLighting ? 1 : 0);
            VAOMaterial.SetInt("useLogEmissiveBuffer", (CommandBufferEnabled && !isHDR && GetCameraEvent(VaoCameraEvent) == CameraEvent.AfterLighting && !isSPSR) ? 1 : 0);
            VAOMaterial.SetInt("useLogBufferInput", (CommandBufferEnabled && !isHDR && (GetCameraEvent(VaoCameraEvent) == CameraEvent.AfterLighting || GetCameraEvent(VaoCameraEvent) == CameraEvent.BeforeReflections)) ? 1 : 0);
            VAOMaterial.SetInt("outputAOOnly", OutputAOOnly ? 1 : 0);
            VAOMaterial.SetInt("isLumaSensitive", IsLumaSensitive ? 1 : 0);
            VAOMaterial.SetInt("useFastBlur", BlurQuality == BlurQualityType.Fast ? 1 : 0);
            VAOMaterial.SetInt("useDedicatedDepthBuffer", (UsePreciseDepthBuffer && (myCamera.actualRenderingPath == RenderingPath.Forward || myCamera.actualRenderingPath == RenderingPath.VertexLit)) ? 1 : 0);

            float quarterResDistance = GetDepthForScreenSize(myCamera, Mathf.Max(texelSize.x, texelSize.y) * HierarchicalBufferPixelsPerLevel * 2.0f);
            float halfResDistance = GetDepthForScreenSize(myCamera, Mathf.Max(texelSize.x, texelSize.y) * HierarchicalBufferPixelsPerLevel);

            quarterResDistance /= -myCamera.farClipPlane;
            halfResDistance /= -myCamera.farClipPlane;

            VAOMaterial.SetFloat("quarterResBufferMaxDistance", quarterResDistance);
            VAOMaterial.SetFloat("halfResBufferMaxDistance", halfResDistance);

            VAOMaterial.SetInt("minRadiusEnabled", (int)DistanceFalloffMode);
            VAOMaterial.SetFloat("minRadiusCutoffDepth", DistanceFalloffMode == DistanceFalloffModeType.Relative ? Mathf.Abs(subPixelDepth) * -(DistanceFalloffStartRelative * DistanceFalloffStartRelative) : -DistanceFalloffStartAbsolute);
            VAOMaterial.SetFloat("minRadiusSoftness", DistanceFalloffMode == DistanceFalloffModeType.Relative ? Mathf.Abs(subPixelDepth) * (DistanceFalloffSpeedRelative * DistanceFalloffSpeedRelative) : DistanceFalloffSpeedAbsolute);

            VAOMaterial.SetInt("giSameHueAttenuationEnabled", ColorbleedHueSuppresionEnabled ? 1 : 0);
            VAOMaterial.SetFloat("giSameHueAttenuationThreshold", ColorBleedHueSuppresionThreshold);
            VAOMaterial.SetFloat("giSameHueAttenuationWidth", ColorBleedHueSuppresionWidth);
            VAOMaterial.SetFloat("giSameHueAttenuationSaturationThreshold", ColorBleedHueSuppresionSaturationThreshold);
            VAOMaterial.SetFloat("giSameHueAttenuationSaturationWidth", ColorBleedHueSuppresionSaturationWidth);
            VAOMaterial.SetFloat("giSameHueAttenuationBrightness", ColorBleedHueSuppresionBrightness);
            VAOMaterial.SetFloat("subpixelRadiusCutoffDepth", Mathf.Min(0.99f, subPixelDepth / -myCamera.farClipPlane));

            VAOMaterial.SetVector("noiseTexelSizeRcp", new Vector2(screenTextureWidth / 3.0f, screenTextureHeight / 3.0f));

            if (Quality == 4 || (Quality == 8 && (ColorBleedQuality == 2 || ColorBleedQuality == 4)))
            {
                VAOMaterial.SetInt("giBlur", (int)GiBlurAmmount.More);
            }
            else
            {
                VAOMaterial.SetInt("giBlur", (int)GiBlurAmmount.Less);
            }


            if (Mode == EffectMode.ColorBleed)
            {
                VAOMaterial.SetFloat("giPower", ColorBleedPower);
                if (Quality == 2 && ColorBleedQuality == 4)
                    VAOMaterial.SetInt("giQuality", 2);
                else
                    VAOMaterial.SetInt("giQuality", ColorBleedQuality);
            }

            if (CullingPrepassMode != CullingPrepassModeType.Off)
            {
                SetVectorArrayNoBuffer("eightSamples", VAOMaterial, samp8);
            }

            if (AdaptiveType != 0)
            {
                SetSampleSet("samples", VAOMaterial, GetAdaptiveSamples());
            }
            else
            {
                if (CullingPrepassMode == CullingPrepassModeType.Careful)
                {
                    SetSampleSet("samples", VAOMaterial, GetCarefulCullingPrepassSamples(samples, samp4));
                }
                else
                {
                    SetSampleSet("samples", VAOMaterial, samples);
                }
            }

            // If simple -> go black
            if (Mode == VAOEffectCommandBuffer.EffectMode.Simple)
            {
                VAOMaterial.SetColor("colorTint", Color.black);
            }
            else
            {
                VAOMaterial.SetColor("colorTint", ColorTint);
            }

            if (BlurMode == BlurModeType.Enhanced)
            {
                if (gaussian == null || gaussian.Length != EnhancedBlurSize || EnhancedBlurDeviation != lastDeviation)
                {
                    gaussian = GenerateGaussian(EnhancedBlurSize, EnhancedBlurDeviation, out gaussianWeight, false);
                    lastDeviation = EnhancedBlurDeviation;
                }

                VAOMaterial.SetFloat("gaussWeight", gaussianWeight);

                SetVectorArray("gauss", VAOMaterial, gaussian, ref gaussianBuffer, ref lastEnhancedBlurSize, true);
            }

            SetKeywords("WFORCE_VAO_COLORBLEED_OFF", "WFORCE_VAO_COLORBLEED_ON", Mode == EffectMode.ColorBleed);
        }

        private void SetKeywords(string offState, string onState, bool state)
        {
            if (state)
            {
                VAOMaterial.DisableKeyword(offState);
                VAOMaterial.EnableKeyword(onState);
            }
            else
            {
                VAOMaterial.DisableKeyword(onState);
                VAOMaterial.EnableKeyword(offState);
            }
        }

        private void EnsureMaterials()
        {
            if (!VAOMaterial && vaoShader.isSupported)
            {
                VAOMaterial = CreateMaterial(vaoShader);
            }

            if (!vaoShader.isSupported)
            {
                ReportError("Could not create shader (Shader not supported).");
            }
        }

        private static Material CreateMaterial(Shader shader)
        {
            if (!shader) return null;

            Material m = new Material(shader);
            m.hideFlags = HideFlags.HideAndDontSave;

            return m;
        }

        private static void DestroyMaterial(Material mat)
        {
            if (mat)
            {
                DestroyImmediate(mat);
                mat = null;
            }
        }

        private void SetVectorArrayNoBuffer(string name, Material VAOMaterial, Vector4[] samples)
        {
#if UNITY_5_4_OR_NEWER
            VAOMaterial.SetVectorArray(name, samples);
#else
                    for (int i = 0; i < samples.Length; ++i)
                    {
                        VAOMaterial.SetVector(name + i.ToString(), samples[i]);
                    }
#endif
        }

        private void SetVectorArray(string name, Material Material, Vector4[] samples, ref Vector4[] samplesBuffer, ref int lastBufferLength, bool needsUpdate)
        {
#if UNITY_5_4_OR_NEWER

            if (needsUpdate || lastBufferLength != samples.Length)
            {
                Array.Copy(samples, samplesBuffer, samples.Length);
                lastBufferLength = samples.Length;
            }

            Material.SetVectorArray(name, samplesBuffer);
#else
                    for (int i = 0; i < samples.Length; ++i)
                    {
                        Material.SetVector(name + i.ToString(), samples[i]);
                    }
#endif
        }

        private void SetSampleSet(string name, Material VAOMaterial, Vector4[] samples)
        {
            SetVectorArray(name, VAOMaterial, samples, ref samplesLarge, ref lastSamplesLength, false);
        }

        #endregion

        #region VAO Data Utilities

        private Vector4[] GetAdaptiveSamples()
        {
            if (adaptiveSamples == null) adaptiveSamples = GenerateAdaptiveSamples();
            return adaptiveSamples;
        }

        private Vector4[] GetCarefulCullingPrepassSamples(Vector4[] samples, Vector4[] carefulSamples)
        {
            if (carefulCache != null && carefulCache.Length == (samples.Length + carefulSamples.Length)) return carefulCache;

            carefulCache = new Vector4[samples.Length + carefulSamples.Length];

            Array.Copy(samples, 0, carefulCache, 0, samples.Length);
            Array.Copy(carefulSamples, 0, carefulCache, samples.Length, carefulSamples.Length);

            return carefulCache;
        }

        private Vector4[] GenerateAdaptiveSamples()
        {
            Vector4[] result = new Vector4[62];

            Array.Copy(samp32, 0, result, 0, 32);
            Array.Copy(samp16, 0, result, 32, 16);
            Array.Copy(samp8, 0, result, 48, 8);
            Array.Copy(samp4, 0, result, 56, 4);
            Array.Copy(samp2, 0, result, 60, 2);

            return result;
        }

        private void EnsureNoiseTexture()
        {
            if (noiseTexture == null)
            {
                noiseTexture = new Texture2D(3, 3, TextureFormat.RGFloat, false, true);
                noiseTexture.SetPixels(noiseSamples);
                noiseTexture.filterMode = FilterMode.Point;
                noiseTexture.wrapMode = TextureWrapMode.Repeat;
                noiseTexture.Apply();
            }
        }

        private static Vector4[] GenerateGaussian(int size, float d, out float weight, bool normalize = true)
        {
            Vector4[] result = new Vector4[size];
            float norm = 0.0f;

            double twodd = 2.0 * d * d;
            double sqrt2ddpi = Math.Sqrt(twodd * Math.PI);

            float phase = (1.0f / (size + 1));
            for (int i = 0; i < size; i++)
            {
                float u = i / (float)(size + 1);
                u += phase;
                u *= 6.0f;
                float uminus3 = (u - 3.0f);

                float temp = -(float)(-(Math.Exp(-(uminus3 * uminus3) / twodd)) / sqrt2ddpi);

                result[i].x = temp;
                norm += temp;
            }

            if (normalize)
            {
                for (int i = 0; i < size; i++)
                {
                    result[i].x /= norm;
                }
            }

            weight = norm;

            return result;
        }

        #endregion

        #region VAO Implementation Utilities

        private float GetDepthForScreenSize(Camera camera, float sizeOnScreen)
        {
            return -(Radius * camera.projectionMatrix.m11) / sizeOnScreen;
        }

        public bool ShouldUseHierarchicalBuffer()
        {
            if (myCamera == null) return false;

            Vector2 texelSize = new Vector2(1.0f / myCamera.pixelWidth, 1.0f / myCamera.pixelHeight);

            float quarterResDistance = GetDepthForScreenSize(myCamera, Mathf.Max(texelSize.x, texelSize.y) * HierarchicalBufferPixelsPerLevel * 2.0f);
            quarterResDistance /= -myCamera.farClipPlane;

            return quarterResDistance > 0.1f;
        }

        public bool HierarchicalBufferEnabled
        {
            get
            {
                if (HierarchicalBufferState == HierarchicalBufferStateType.On) return true;

                if (HierarchicalBufferState == HierarchicalBufferStateType.Auto)
                {
                    return ShouldUseHierarchicalBuffer();
                }

                return false;
            }
        }

        public bool ShouldUseGBuffer()
        {
            if (myCamera == null) return UseGBuffer;

            if (myCamera.actualRenderingPath != RenderingPath.DeferredShading) return false;

            if (VaoCameraEvent != VAOCameraEventType.BeforeImageEffectsOpaque) return true;
            
            return UseGBuffer;
        }

        protected void EnsureVAOVersion()
        {
            if (CommandBufferEnabled && (this is VAOEffectCommandBuffer) && !(this is VAOEffect)) return;
            if (!CommandBufferEnabled && (this is VAOEffect)) return;

            var allComponents = GetComponents<Component>();
            var parameters = GetParameters();

            int oldComponentIndex = -1;
            Component newComponent = null;

            for (int i = 0; i < allComponents.Length; i++)
            {
                if (CommandBufferEnabled && (allComponents[i] == this))
                {

                    var oldGameObject = gameObject;
                    DestroyImmediate(this);
                    newComponent = oldGameObject.AddComponent<VAOEffectCommandBuffer>();
                    (newComponent as VAOEffectCommandBuffer).SetParameters(parameters);
                    oldComponentIndex = i;
                    break;
                }

                if (!CommandBufferEnabled && ((allComponents[i] == this)))
                {
                    var oldGameObject = gameObject;
                    TeardownCommandBuffer();
                    DestroyImmediate(this);
                    newComponent = oldGameObject.AddComponent<VAOEffect>();
                    (newComponent as VAOEffect).SetParameters(parameters);
                    oldComponentIndex = i;
                    break;
                }
            }

            if (oldComponentIndex >= 0 && newComponent != null)
            {
#if UNITY_EDITOR
                allComponents = newComponent.gameObject.GetComponents<Component>();
                int currentIndex = 0;

                for (int i = 0; i < allComponents.Length; i++)
                {
                    if (allComponents[i] == newComponent)
                    {
                        currentIndex = i;
                        break;
                    }
                }

                for (int i = 0; i < currentIndex - oldComponentIndex; i++)
                {
                    UnityEditorInternal.ComponentUtility.MoveComponentUp(newComponent);
                }
#endif
            }
        }

        private bool CheckSettingsChanges()
        {
            bool settingsDirty = false;

            if (GetCameraEvent(VaoCameraEvent) != lastCameraEvent)
            {
                TeardownCommandBuffer();
                settingsDirty = true;
            }

            if (Downsampling != lastDownsampling)
            {
                lastDownsampling = Downsampling;
                settingsDirty = true;
            }

            if (CullingPrepassMode != lastcullingPrepassType)
            {
                lastcullingPrepassType = CullingPrepassMode;
                settingsDirty = true;
            }

            if (CullingPrepassDownsamplingFactor != lastCullingPrepassDownsamplingFactor)
            {
                lastCullingPrepassDownsamplingFactor = CullingPrepassDownsamplingFactor;
                settingsDirty = true;
            }

            if (BlurMode != lastBlurMode)
            {
                lastBlurMode = BlurMode;
                settingsDirty = true;
            }

            if (Mode != lastMode)
            {
                lastMode = Mode;
                settingsDirty = true;
            }

            if (UseGBuffer != lastUseGBuffer)
            {
                lastUseGBuffer = UseGBuffer;
                settingsDirty = true;
            }

            if (OutputAOOnly != lastOutputAOOnly)
            {
                lastOutputAOOnly = OutputAOOnly;
                settingsDirty = true;
            }

            isHDR = isCameraHDR(myCamera);
            if (isHDR != lastIsHDR)
            {
                lastIsHDR = isHDR;
                settingsDirty = true;
            }

#if UNITY_EDITOR
            isSPSR = isCameraSPSR(myCamera);
            if (isSPSR != lastIsSPSR)
            {
                lastIsSPSR = isSPSR;
                settingsDirty = true;
            }
#endif

            if (lastIntermediateScreenTextureFormat != IntermediateScreenTextureFormat)
            {
                lastIntermediateScreenTextureFormat = IntermediateScreenTextureFormat;
                settingsDirty = true;
            }

            if (lastCmdBufferEnhancedBlurSize != EnhancedBlurSize)
            {
                lastCmdBufferEnhancedBlurSize = EnhancedBlurSize;
                settingsDirty = true;
            }

            if (lastHierarchicalBufferEnabled != HierarchicalBufferEnabled)
            {
                lastHierarchicalBufferEnabled = HierarchicalBufferEnabled;
                settingsDirty = true;
            }

            if (lastBlurQuality != BlurQuality)
            {
                lastBlurQuality = BlurQuality;
                settingsDirty = true;
            }

            return settingsDirty;
        }

        private void WarmupPass(KeyValuePair<string, string>[] input, int i, RenderTexture tempTarget, int passNumber)
        {

            if (i >= 0)
            {
                SetKeywords(input[i].Key, input[i].Value, false);
                WarmupPass(input, i - 1, tempTarget, passNumber);

                SetKeywords(input[i].Key, input[i].Value, true);
                WarmupPass(input, i - 1, tempTarget, passNumber);
            }

            Graphics.Blit(null, tempTarget, VAOMaterial, passNumber);

        }

        private RenderTextureFormat GetRenderTextureFormat(ScreenTextureFormat format, bool isHDR)
        {
            switch (format)
            {
                case ScreenTextureFormat.Default:
                    return RenderTextureFormat.Default;
                case ScreenTextureFormat.DefaultHDR:
                    return RenderTextureFormat.DefaultHDR;
                case ScreenTextureFormat.ARGB32:
                    return RenderTextureFormat.ARGB32;
                case ScreenTextureFormat.ARGBFloat:
                    return RenderTextureFormat.ARGBFloat;
                case ScreenTextureFormat.ARGBHalf:
                    return RenderTextureFormat.ARGBHalf;
                default:
                    return isHDR ? RenderTextureFormat.DefaultHDR : RenderTextureFormat.Default;
            }
        }

        #endregion

        #region Unity Utilities

        private void ReportError(string error)
        {
            if (Debug.isDebugBuild) Debug.LogError("VAO Effect Error: " + error);
        }

        private void ReportWarning(string error)
        {
            if (Debug.isDebugBuild) Debug.LogWarning("VAO Effect Warning: " + error);
        }

        private bool isCameraSPSR(Camera camera)
        {
            if (camera == null) return false;

#if UNITY_5_5_OR_NEWER
#if UNITY_EDITOR
            if (camera.stereoEnabled && PlayerSettings.stereoRenderingPath == StereoRenderingPath.SinglePass)
                return true;
#endif
#endif

            return false;
        }

        private bool isCameraHDR(Camera camera)
        {

#if UNITY_5_6_OR_NEWER
                    if (camera != null) return camera.allowHDR;
#else
            if (camera != null) return camera.hdr;
#endif
            return false;
        }

        private bool MustForceFlip(Camera camera)
        {
#if UNITY_5_6_OR_NEWER
                    return false;
#else
            if (myCamera.stereoEnabled)
            {
                return false;
            }
            if (!CommandBufferEnabled) return false;
            if (camera.actualRenderingPath != RenderingPath.DeferredShading && camera.actualRenderingPath != RenderingPath.DeferredLighting) return true;
            return false;
#endif
        }

        protected List<KeyValuePair<FieldInfo, object>> GetParameters()
        {
            var result = new List<KeyValuePair<FieldInfo, object>>();

            var fields = this.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public);

            foreach (var field in fields)
            {
                result.Add(new KeyValuePair<FieldInfo, object>(field, field.GetValue(this)));
            }

            return result;
        }

        protected void SetParameters(List<KeyValuePair<FieldInfo, object>> parameters)
        {
            foreach (var parameter in parameters)
            {
                parameter.Key.SetValue(this, parameter.Value);
            }
        }

        #endregion

        #region  Data
        private static Color[] noiseSamples = new Color[9]{
                new Color(1.0f, 0.0f, 0.0f),
                new Color(-0.939692f, 0.342022f, 0.0f),
                new Color(0.173644f, -0.984808f, 0.0f),
                new Color(0.173649f, 0.984808f, 0.0f),
                new Color(-0.500003f, -0.866024f, 0.0f),
                new Color(0.766045f, 0.642787f, 0.0f),
                new Color(-0.939694f, -0.342017f, 0.0f),
                new Color(0.766042f, -0.642791f, 0.0f),
                new Color(-0.499999f, 0.866026f, 0.0f)};
        private static Vector4[] samp2 = new Vector4[2] {
                new Vector4(0.4392292f,  0.0127914f, 0.898284f),
                new Vector4(-0.894406f,  -0.162116f, 0.41684f)};
        private static Vector4[] samp4 = new Vector4[4] {
                new Vector4(-0.07984404f,  -0.2016976f, 0.976188f),
                new Vector4(0.4685118f,  -0.8404996f, 0.272135f),
                new Vector4(-0.793633f,  0.293059f, 0.533164f),
                new Vector4(0.2998218f,  0.4641494f, 0.83347f)};
        private static Vector4[] samp8 = new Vector4[8] {
                new Vector4(-0.4999112f,  -0.571184f, 0.651028f),
                new Vector4(0.2267525f,  -0.668142f, 0.708639f),
                new Vector4(0.0657284f,  -0.123769f, 0.990132f),
                new Vector4(0.9259827f,  -0.2030669f, 0.318307f),
                new Vector4(-0.9850165f,  0.1247843f, 0.119042f),
                new Vector4(-0.2988613f,  0.2567392f, 0.919112f),
                new Vector4(0.4734727f,  0.2830991f, 0.834073f),
                new Vector4(0.1319883f,  0.9544416f, 0.267621f)};
        private static Vector4[] samp16 = new Vector4[16] {
                new Vector4(-0.6870962f,  -0.7179669f, 0.111458f),
                new Vector4(-0.2574025f,  -0.6144419f, 0.745791f),
                new Vector4(-0.408366f,  -0.162244f, 0.898284f),
                new Vector4(-0.07098053f,  0.02052395f, 0.997267f),
                new Vector4(0.2019972f,  -0.760972f, 0.616538f),
                new Vector4(0.706282f,  -0.6368136f, 0.309248f),
                new Vector4(0.169605f,  -0.2892981f, 0.942094f),
                new Vector4(0.7644456f,  -0.05826119f, 0.64205f),
                new Vector4(-0.745912f,  0.0501786f, 0.664152f),
                new Vector4(-0.7588732f,  0.4313389f, 0.487911f),
                new Vector4(-0.3806622f,  0.3446409f, 0.85809f),
                new Vector4(-0.1296651f,  0.8794711f, 0.45795f),
                new Vector4(0.1557318f,  0.137468f, 0.978187f),
                new Vector4(0.5990864f,  0.2485375f, 0.761133f),
                new Vector4(0.1727637f,  0.5753375f, 0.799462f),
                new Vector4(0.5883294f,  0.7348878f, 0.337355f)};
        private static Vector4[] samp32 = new Vector4[32] {
                new Vector4(-0.626056f,  -0.7776781f, 0.0571977f),
                new Vector4(-0.1335098f,  -0.9164876f, 0.377127f),
                new Vector4(-0.2668636f,  -0.5663173f, 0.779787f),
                new Vector4(-0.5712572f,  -0.4639561f, 0.67706f),
                new Vector4(-0.6571807f,  -0.2969118f, 0.692789f),
                new Vector4(-0.8896923f,  -0.1314662f, 0.437223f),
                new Vector4(-0.5037534f,  -0.03057539f, 0.863306f),
                new Vector4(-0.1773856f,  -0.2664998f, 0.947371f),
                new Vector4(-0.02786797f,  -0.02453661f, 0.99931f),
                new Vector4(0.173095f,  -0.964425f, 0.199805f),
                new Vector4(0.280491f,  -0.716259f, 0.638982f),
                new Vector4(0.7610048f,  -0.4987299f, 0.414898f),
                new Vector4(0.135136f,  -0.388973f, 0.911284f),
                new Vector4(0.4836829f,  -0.4782286f, 0.73304f),
                new Vector4(0.1905736f,  -0.1039435f, 0.976154f),
                new Vector4(0.4855643f,  0.01388972f, 0.87409f),
                new Vector4(0.5684234f,  -0.2864941f, 0.771243f),
                new Vector4(0.8165832f,  0.01384446f, 0.577062f),
                new Vector4(-0.9814694f,  0.18555f, 0.0478435f),
                new Vector4(-0.5357604f,  0.3316899f, 0.776494f),
                new Vector4(-0.1238877f,  0.03315933f, 0.991742f),
                new Vector4(-0.1610546f,  0.3801286f, 0.910804f),
                new Vector4(-0.5923722f,  0.628729f, 0.503781f),
                new Vector4(-0.05504921f,  0.5483891f, 0.834409f),
                new Vector4(-0.3805041f,  0.8377199f, 0.391717f),
                new Vector4(-0.101651f,  0.9530866f, 0.285119f),
                new Vector4(0.1613653f,  0.2561041f, 0.953085f),
                new Vector4(0.4533991f,  0.2896196f, 0.842941f),
                new Vector4(0.6665574f,  0.4639243f, 0.583503f),
                new Vector4(0.8873722f,  0.4278904f, 0.1717f),
                new Vector4(0.2869751f,  0.732805f, 0.616962f),
                new Vector4(0.4188429f,  0.7185978f, 0.555147f)};
        private static Vector4[] samp64 = new Vector4[64] {
                new Vector4(-0.6700248f,  -0.6370129f, 0.381157f),
                new Vector4(-0.7385408f,  -0.6073685f, 0.292679f),
                new Vector4(-0.4108568f,  -0.8852778f, 0.2179f),
                new Vector4(-0.3058583f,  -0.8047022f, 0.508828f),
                new Vector4(0.01087609f,  -0.7610992f, 0.648545f),
                new Vector4(-0.3629634f,  -0.5480431f, 0.753595f),
                new Vector4(-0.1480379f,  -0.6927805f, 0.70579f),
                new Vector4(-0.9533184f,  -0.276674f, 0.12098f),
                new Vector4(-0.6387863f,  -0.3999016f, 0.65729f),
                new Vector4(-0.891588f,  -0.115146f, 0.437964f),
                new Vector4(-0.775663f,  0.0194654f, 0.630848f),
                new Vector4(-0.5360528f,  -0.1828935f, 0.824134f),
                new Vector4(-0.513927f,  -0.000130296f, 0.857834f),
                new Vector4(-0.4368436f,  -0.2831443f, 0.853813f),
                new Vector4(-0.1794069f,  -0.4226944f, 0.888337f),
                new Vector4(-0.00183062f,  -0.4371257f, 0.899398f),
                new Vector4(-0.2598701f,  -0.1719497f, 0.950211f),
                new Vector4(-0.08650014f,  -0.004176182f, 0.996243f),
                new Vector4(0.006921067f,  -0.001478712f, 0.999975f),
                new Vector4(0.05654667f,  -0.9351676f, 0.349662f),
                new Vector4(0.1168661f,  -0.754741f, 0.64553f),
                new Vector4(0.3534952f,  -0.7472929f, 0.562667f),
                new Vector4(0.1635596f,  -0.5863093f, 0.793404f),
                new Vector4(0.5910167f,  -0.786864f, 0.177609f),
                new Vector4(0.5820105f,  -0.5659724f, 0.5839f),
                new Vector4(0.7254612f,  -0.5323696f, 0.436221f),
                new Vector4(0.4016336f,  -0.4329237f, 0.807012f),
                new Vector4(0.5287027f,  -0.4064075f, 0.745188f),
                new Vector4(0.314015f,  -0.2375291f, 0.919225f),
                new Vector4(0.02922117f,  -0.2097672f, 0.977315f),
                new Vector4(0.4201531f,  -0.1445212f, 0.895871f),
                new Vector4(0.2821195f,  -0.01079273f, 0.959319f),
                new Vector4(0.7152653f,  -0.1972963f, 0.670425f),
                new Vector4(0.8167331f,  -0.1217311f, 0.564029f),
                new Vector4(0.8517836f,  0.01290532f, 0.523735f),
                new Vector4(-0.657816f,  0.134013f, 0.74116f),
                new Vector4(-0.851676f,  0.321285f, 0.414033f),
                new Vector4(-0.603183f,  0.361627f, 0.710912f),
                new Vector4(-0.6607267f,  0.5282444f, 0.533289f),
                new Vector4(-0.323619f,  0.182656f, 0.92839f),
                new Vector4(-0.2080927f,  0.1494067f, 0.966631f),
                new Vector4(-0.4205947f,  0.4184987f, 0.804959f),
                new Vector4(-0.06831062f,  0.3712724f, 0.926008f),
                new Vector4(-0.165943f,  0.5029928f, 0.84821f),
                new Vector4(-0.6137413f,  0.7001954f, 0.364758f),
                new Vector4(-0.3009551f,  0.6550035f, 0.693107f),
                new Vector4(-0.1356791f,  0.6460465f, 0.751143f),
                new Vector4(-0.3677429f,  0.7920387f, 0.487278f),
                new Vector4(-0.08688695f,  0.9677781f, 0.236338f),
                new Vector4(0.07250954f,  0.1327261f, 0.988497f),
                new Vector4(0.5244588f,  0.05565827f, 0.849615f),
                new Vector4(0.2498424f,  0.3364912f, 0.907938f),
                new Vector4(0.2608168f,  0.5340923f, 0.804189f),
                new Vector4(0.3888291f,  0.3207975f, 0.863655f),
                new Vector4(0.6413552f,  0.1619097f, 0.749966f),
                new Vector4(0.8523082f,  0.2647078f, 0.451111f),
                new Vector4(0.5591328f,  0.3038472f, 0.771393f),
                new Vector4(0.9147445f,  0.3917669f, 0.0987938f),
                new Vector4(0.08110893f,  0.7317293f, 0.676752f),
                new Vector4(0.3154335f,  0.7388063f, 0.59554f),
                new Vector4(0.1677455f,  0.9625717f, 0.212877f),
                new Vector4(0.3015989f,  0.9509261f, 0.069128f),
                new Vector4(0.5600207f,  0.5649592f, 0.605969f),
                new Vector4(0.6455291f,  0.7387806f, 0.193637f)};

        #endregion
    }


#if UNITY_EDITOR

    [CustomEditor(typeof(VAOEffectCommandBuffer))]
    public class VAOEffectEditorCmdBuffer : VAOEffectEditor { }

    public class VAOEffectEditor : Editor
    {

        #region Labels

        private readonly GUIContent[] qualityTexts = new GUIContent[6] {
                    new GUIContent("Very Low (2 samples)"),
                    new GUIContent("Low (4 samples)"),
                    new GUIContent("Medium (8 samples)"),
                    new GUIContent("High (16 samples)"),
                    new GUIContent("Very High (32 samples)"),
                    new GUIContent("Ultra (64 samples)")
                };
        private readonly int[] qualityInts = new int[6] { 2, 4, 8, 16, 32, 64 };

        private readonly GUIContent[] qualityTextsAdaptive = new GUIContent[4] {
                    new GUIContent("Low (4 samples) Adaptive"),
                    new GUIContent("Medium (8 samples) Adaptive"),
                    new GUIContent("High (16 samples) Adaptive"),
                    new GUIContent("Very High (32 samples) Adaptive")
                };
        private readonly int[] qualityIntsAdaptive = new int[4] { 4, 8, 16, 32 };

        private readonly GUIContent[] downsamplingTexts = new GUIContent[3] {
                    new GUIContent("Off"),
                    new GUIContent("2x"),
                    new GUIContent("4x")
                };
        private readonly int[] downsamplingInts = new int[3] { 1, 2, 4 };

        private readonly GUIContent[] hzbTexts = new GUIContent[3] {
                    new GUIContent("Off"),
                    new GUIContent("On"),
                    new GUIContent("Auto")
                };
        private readonly int[] hzbInts = new int[3] { 0, 1, 2 };

        private readonly GUIContent[] giTexts = new GUIContent[3] {
                    new GUIContent("Normal"),
                    new GUIContent("Half"),
                    new GUIContent("Quarter")
                };
        private readonly int[] giInts = new int[3] { 1, 2, 4 };

        private readonly GUIContent radiusLabelContent = new GUIContent("Radius:", "Distance of the objects that are still considered for occlusions");
        private readonly GUIContent powerLabelContent = new GUIContent("Power:", "Strength of the occlusion");
        private readonly GUIContent presenceLabelContent = new GUIContent("Presence:", "Increase to make effect more pronounced in corners");
        private readonly GUIContent adaptiveLevelLabelContent = new GUIContent("Adaptive Offset:", "Adjust to fine-tune adaptive sampling quality/performance");
        private readonly GUIContent qualityLabelContent = new GUIContent("Quality:", "Number of samples used");
        private readonly GUIContent downsamplingLabelContent = new GUIContent("Downsampling:", "Reduces the resulting texture size");
        private readonly GUIContent hzbContent = new GUIContent("Hierarchical Buffers:", "Uses downsampled depth and normal buffers for increased performance when using large radius");

        private readonly GUIContent lumaEnabledLabelContent = new GUIContent("Luma Sensitivity:", "Enables luminance sensitivity");
        private readonly GUIContent lumaThresholdLabelContent = new GUIContent("Threshold:", "Sets which bright surfaces are no longer occluded");
        private readonly GUIContent lumaThresholdHDRLabelContent = new GUIContent("Threshold (HDR):", "Sets which bright surfaces are no longer occluded");
        private readonly GUIContent lumaWidthLabelContent = new GUIContent("Falloff Width:", "Controls the weight of the occlusion as it nears the threshold");
        private readonly GUIContent lumaSoftnessLabelContent = new GUIContent("Falloff Softness:", "Controls the gradient of the falloff");

        private readonly GUIContent effectModeLabelContent = new GUIContent("Effect Mode:", "Switches between different effect modes");
        private readonly GUIContent colorTintLabelContent = new GUIContent("Color Tint:", "Choose the color of the occlusion shadows");

        private readonly GUIContent luminanceModeLabelContent = new GUIContent("Mode:", "Switches sensitivity between luma and HSV value");
        private readonly GUIContent optimizationLabelContent = new GUIContent("Performance Settings:", "");
        private readonly GUIContent cullingPrepassTypeLabelContent = new GUIContent("Downsampled Pre-pass:", "Enable to boost performance, especially on lower radius and higher resolution/quality settings. Greedy option is faster but might produce minute detail loss.");
        private readonly GUIContent adaptiveSamplingLabelContent = new GUIContent("Adaptive Sampling:", "Automagically sets progressively lower quality for distant geometry");

        private readonly GUIContent radiusLimitsFoldoutLabelContent = new GUIContent("Near/Far Occlusion Limits:", "Special occlusion behaviour depending on distance from camera");

        private readonly GUIContent pipelineLabelContent = new GUIContent("Rendering Pipeline:", "Unity Rendering pipeline options.");
        private readonly GUIContent commandBufferLabelContent = new GUIContent("Command Buffer:", "Insert effect via command buffer (BeforeImageEffectsOpaque event)");
        private readonly GUIContent gBufferLabelContent = new GUIContent("G-Buffer Depth&Normals:", "Take depth&normals from GBuffer of deferred rendering path, use this to overcome compatibility issues or for better precision");
        private readonly GUIContent dedicatedDepthBufferLabelContent = new GUIContent("High precision depth buffer:", "Uses higher precision depth buffer (forward path only). This may also fix some materials that work normally in deferred path.");
        private readonly GUIContent cameraEventLabelContent = new GUIContent("Cmd. buffer integration stage:", "Where in rendering pipeline is VAO calculated - only available for DEFERRED rendering path. Before Reflecitons is earliest event and AO is influenced by reflections. After Lighting only influences lighting of the scene. On event Before Images Effect Opaque the VAO is applied to final image.");

        private readonly GUIContent blurModeContent = new GUIContent("Blur Mode:", "Switches between different blur effects");
        private readonly GUIContent blurQualityContent = new GUIContent("Blur Quality:", "Switches between faster and less accurate or and slower but more precise blur implementation");
        private readonly GUIContent blurFoldoutContent = new GUIContent("Enhanced Blur Settings", "Lets you control behaviour of the enhanced blur");
        private readonly GUIContent blurSizeContent = new GUIContent("Blur Size:", "Change to adjust the size of area that is averaged");
        private readonly GUIContent blurDeviationContent = new GUIContent("Blur Sharpness:", "Standard deviation for Gaussian blur - smaller deviation means sharper image");

        private readonly GUIContent aoLabelContent = new GUIContent("Output AO only:", "Displays just the occlusion - used for tuning the settings");

        private readonly GUIContent colorBleedLabelContent = new GUIContent("Color Bleed Settings", "Lets you control indirect illumination");
        private readonly GUIContent colorBleedPowerLabelContent = new GUIContent("Power:", "Strength of the color bleed");
        private readonly GUIContent colorBleedPresenceLabelContent = new GUIContent("Presence:", "Smoothly limits maximal saturation of color bleed");
        private readonly GUIContent colorBleedQualityLabelContent = new GUIContent("Quality:", "Samples used for color bleed");
        private readonly GUIContent ColorBleedSelfOcclusionLabelContent = new GUIContent("Dampen Self-Bleeding:", "Limits casting color on itself");
        private readonly GUIContent backfaceLabelContent = new GUIContent("Skip Backfaces:", "Skips surfaces facing other way");
        private readonly GUIContent screenFormatLabelContent = new GUIContent("Intermediate texture format:", "Texture format to use for mixing VAO with scene. Auto is recommended.");
        private readonly GUIContent farPlaneSourceLabelContent = new GUIContent("Far plane source:", "Where to take far plane distance from. Camera is needed for post-processing stack temporal AA compatibility. Use Projection Params option for compatibility with other effects.");
        private readonly GUIContent maxRadiusLabelContent = new GUIContent("Limit Max Radius:", "Maximal radius given as percentage of screen that will be considered for occlusion. Use to avoid performance drop for objects close to camera.");
        private readonly GUIContent maxRadiusSliderContent = new GUIContent("Max Radius:", "Maximal radius given as fraction of the screen that can be considered for occlusion.");
        private readonly GUIContent distanceFalloffModeLabelContent = new GUIContent("Distance Falloff:", "With this enabled occlusion starts to fall off rapidly at certain distance.");
        private readonly GUIContent distanceFalloffAbsoluteLabelContent = new GUIContent("Falloff Start:", "Falloff distance set as an absolute value (same as Far Clipping Plane).");
        private readonly GUIContent distanceFalloffRelativeLabelContent = new GUIContent("Falloff Start:", "Falloff start set relative to occlusion area covering one screen pixel.");
        private readonly GUIContent distanceFalloffSpeedLabelContent = new GUIContent("Falloff Speed:", "How fast the occlusion decreases after the falloff border.");
        private readonly GUIContent colorBleedSameHueAttenuationLabelContent = new GUIContent("Same Color Hue Attenuation", "Attenuates colorbleed thrown on surface of the same color.");
        private readonly GUIContent colorBleedSameHueAttenuationHueFilterLabelContent = new GUIContent("Hue Filter", "Set how much the hue has to differ to be filtered out.");
        private readonly GUIContent colorBleedSameHueAttenuationHueToleranceLabelContent = new GUIContent("Tolerance:", "How much the hue has to differ to be filtered out.");
        private readonly GUIContent colorBleedSameHueAttenuationHueSoftnessLabelContent = new GUIContent("Softness:", "How smooth will the transision be.");
        private readonly GUIContent colorBleedSameHueAttenuationSaturationFilterLabelContent = new GUIContent("Saturation Filter", "Set minimal saturation of color where hue filter will be applied.");
        private readonly GUIContent colorBleedSameHueAttenuationSaturationToleranceLabelContent = new GUIContent("Threshold:", "Saturation threshold when hue filter will be applied.");
        private readonly GUIContent colorBleedSameHueAttenuationSaturationSoftnessLabelContent = new GUIContent("Softness:", "How smooth will the transision be.");
        private readonly GUIContent colorBleedSameHueAttenuationBrightnessLabelContent = new GUIContent("Brightness:", "Limits value component of HSV model of the result.");

        #endregion

        #region Previous Settings Cache

        private float lastLumaThreshold;
        private float lastLumaKneeWidth;
        private float lastLumaKneeLinearity;
        private float lastLumaMaxFx;
        private bool lastLumaSensitive;
        private VAOEffectCommandBuffer.EffectMode lastEffectMode;
        private float lumaMaxFx = 10.0f;

        #endregion

        #region Foldouts

        private bool lumaFoldout = true;
        private bool colorBleedFoldout = false;
        private bool optimizationFoldout = true;
        private bool radiusLimitsFoldout = true;
        private bool pipelineFoldout = true;
        private bool blurFoldout = true;
        private bool aboutFoldout = false;

        #endregion

        #region Luma Graph Widget

        private GraphWidget lumaGraphWidget;
        private GraphWidgetDrawingParameters lumaGraphWidgetParams;

        private GraphWidgetDrawingParameters GetLumaGraphWidgetParameters(VAOEffectCommandBuffer vaoScript)
        {
            if (lumaGraphWidgetParams != null &&
                lastLumaThreshold == vaoScript.LumaThreshold &&
                lastLumaKneeWidth == vaoScript.LumaKneeWidth &&
                lastLumaMaxFx == lumaMaxFx &&
                lastLumaKneeLinearity == vaoScript.LumaKneeLinearity) return lumaGraphWidgetParams;

            lastLumaThreshold = vaoScript.LumaThreshold;
            lastLumaKneeWidth = vaoScript.LumaKneeWidth;
            lastLumaKneeLinearity = vaoScript.LumaKneeLinearity;
            lastLumaMaxFx = lumaMaxFx;

            lumaGraphWidgetParams = new GraphWidgetDrawingParameters()
            {
                GraphSegmentsCount = 128,
                GraphColor = Color.white,
                GraphThickness = 2.0f,
                GraphFunction = ((float x) =>
                {
                    float Y = (x - (vaoScript.LumaThreshold - vaoScript.LumaKneeWidth)) * (1.0f / (2.0f * vaoScript.LumaKneeWidth));
                    x = Mathf.Min(1.0f, Mathf.Max(0.0f, Y));
                    return ((-Mathf.Pow(x, vaoScript.LumaKneeLinearity) + 1));
                }),
                YScale = 0.65f,
                MinY = 0.1f,
                MaxFx = lumaMaxFx,
                GridLinesXCount = 4,
                LabelText = "Luminance sensitivity curve",
                Lines = new List<GraphWidgetLine>()
                        {
                            new GraphWidgetLine() {
                                Color = Color.red,
                                Thickness = 2.0f,
                                From = new Vector3(vaoScript.LumaThreshold / lumaMaxFx, 0.0f, 0.0f),
                                To = new Vector3(vaoScript.LumaThreshold / lumaMaxFx, 1.0f, 0.0f)
                            },
                            new GraphWidgetLine() {
                                Color = Color.blue * 0.7f,
                                Thickness = 2.0f,
                                From = new Vector3((vaoScript.LumaThreshold - vaoScript.LumaKneeWidth) / lumaMaxFx, 0.0f, 0.0f),
                                To = new Vector3((vaoScript.LumaThreshold - vaoScript.LumaKneeWidth) / lumaMaxFx, 1.0f, 0.0f)
                            },
                            new GraphWidgetLine() {
                                Color = Color.blue * 0.7f,
                                Thickness = 2.0f,
                                From = new Vector3((vaoScript.LumaThreshold + vaoScript.LumaKneeWidth) / lumaMaxFx, 0.0f, 0.0f),
                                To = new Vector3((vaoScript.LumaThreshold + vaoScript.LumaKneeWidth) / lumaMaxFx, 1.0f, 0.0f)
                            }
                        }
            };

            return lumaGraphWidgetParams;
        }

        #endregion

        private bool isHDR;
        private Camera camera;

        #region VAO Implementation Utilities

        private float GetRadiusForDepthAndScreenRadius(Camera camera, float pixelDepth, float maxRadiusOnScreen)
        {
            return -(pixelDepth * maxRadiusOnScreen) / camera.projectionMatrix.m11;
        }

        private float GetScreenSizeForDepth(Camera camera, float pixelDepth, float radius)
        {
            return -(radius * camera.projectionMatrix.m11) / pixelDepth;
        }

        private float GetScreenSizeForDepth(Camera camera, float pixelDepth, float radius, bool maxRadiusEnabled, float maxRadiusCutoffDepth, float maxRadiusOnScreen)
        {
            if (maxRadiusEnabled && pixelDepth > maxRadiusCutoffDepth)
            {
                radius = GetRadiusForDepthAndScreenRadius(camera, pixelDepth, maxRadiusOnScreen);
            }

            return GetScreenSizeForDepth(camera, pixelDepth, radius);
        }

        private float GetDepthForScreenSize(Camera camera, float sizeOnScreen, float radius)
        {
            return -(radius * camera.projectionMatrix.m11) / sizeOnScreen;
        }

        #endregion

        #region Unity Utilities

        private void SetIcon()
        {
            try
            {
                Texture2D icon = (Texture2D)Resources.Load("wilberforce_script_icon");
                Type editorGUIUtilityType = typeof(UnityEditor.EditorGUIUtility);
                System.Reflection.BindingFlags bindingFlags = System.Reflection.BindingFlags.InvokeMethod | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic;
                object[] args = new object[] { target, icon };
                editorGUIUtilityType.InvokeMember("SetIconForObject", bindingFlags, null, null, args);
            }
            catch (Exception ex)
            {
                if (Debug.isDebugBuild) Debug.Log("VAO Effect Error: There was an exception while setting icon to VAO script: " + ex.Message);
            }
        }

        void OnEnable()
        {
            if (lumaGraphWidget == null) lumaGraphWidget = new GraphWidget();

            VAOEffectCommandBuffer effect = (target as VAOEffectCommandBuffer);
            camera = effect.GetComponent<Camera>();

            isHDR = isCameraHDR(camera);
            SetIcon();

        }

        private bool isCameraHDR(Camera camera)
        {

#if UNITY_5_6_OR_NEWER
                    if (camera != null) return camera.allowHDR;
#else
            if (camera != null) return camera.hdr;
#endif
            return false;
        }

        #endregion

        override public void OnInspectorGUI()
        {
            var vaoScript = target as VAOEffectCommandBuffer;
            vaoScript.vaoShader = EditorGUILayout.ObjectField("Vao Shader", vaoScript.vaoShader, typeof(Shader), false) as UnityEngine.Shader;

            if (vaoScript.ShouldUseHierarchicalBuffer())
            {
                hzbTexts[2].text = "Auto (currently on)";
            }
            else
            {
                hzbTexts[2].text = "Auto (currently off)";
            }

            EditorGUILayout.Space();

            vaoScript.Radius = EditorGUILayout.FloatField(radiusLabelContent, vaoScript.Radius);
            vaoScript.Power = EditorGUILayout.FloatField(powerLabelContent, vaoScript.Power);
            vaoScript.Presence = EditorGUILayout.Slider(presenceLabelContent, vaoScript.Presence, 0.0f, 1.0f);

            EditorGUILayout.Space();

            if (vaoScript.AdaptiveType == VAOEffectCommandBuffer.AdaptiveSamplingType.Disabled)
            {
                vaoScript.Quality = EditorGUILayout.IntPopup(qualityLabelContent, vaoScript.Quality, qualityTexts, qualityInts);
            }
            else
            {
                vaoScript.Quality = EditorGUILayout.IntPopup(qualityLabelContent, vaoScript.Quality, qualityTextsAdaptive, qualityIntsAdaptive);
            }
            EditorGUILayout.Space();
            EditorGUI.indentLevel++;

            radiusLimitsFoldout = EditorGUI.Foldout(EditorGUILayout.GetControlRect(), radiusLimitsFoldout, radiusLimitsFoldoutLabelContent, true, EditorStyles.foldout);

            if (radiusLimitsFoldout)
            {
                //EditorGUILayout.BeginHorizontal();
                vaoScript.MaxRadiusEnabled = EditorGUILayout.Toggle(maxRadiusLabelContent, vaoScript.MaxRadiusEnabled, customToggleStyle);
                if (vaoScript.MaxRadiusEnabled)
                    vaoScript.MaxRadius = EditorGUILayout.Slider(maxRadiusSliderContent, vaoScript.MaxRadius, 0.05f, 3.0f);

                //EditorGUILayout.EndHorizontal();

                vaoScript.DistanceFalloffMode = (VAOEffectCommandBuffer.DistanceFalloffModeType)EditorGUILayout.EnumPopup(distanceFalloffModeLabelContent, vaoScript.DistanceFalloffMode);
                switch (vaoScript.DistanceFalloffMode)
                {
                    case VAOEffectCommandBuffer.DistanceFalloffModeType.Off:
                        break;
                    case VAOEffectCommandBuffer.DistanceFalloffModeType.Absolute:
                        vaoScript.DistanceFalloffStartAbsolute = EditorGUILayout.FloatField(distanceFalloffAbsoluteLabelContent, vaoScript.DistanceFalloffStartAbsolute);
                        vaoScript.DistanceFalloffSpeedAbsolute = EditorGUILayout.FloatField(distanceFalloffSpeedLabelContent, vaoScript.DistanceFalloffSpeedAbsolute);
                        break;
                    case VAOEffectCommandBuffer.DistanceFalloffModeType.Relative:
                        vaoScript.DistanceFalloffStartRelative = EditorGUILayout.Slider(distanceFalloffRelativeLabelContent, vaoScript.DistanceFalloffStartRelative, 0.0f, 1.0f);
                        vaoScript.DistanceFalloffSpeedRelative = EditorGUILayout.Slider(distanceFalloffSpeedLabelContent, vaoScript.DistanceFalloffSpeedRelative, 0.0f, 1.0f);
                        break;
                    default:
                        break;
                }
                EditorGUILayout.Space();
            }

            optimizationFoldout = EditorGUI.Foldout(EditorGUILayout.GetControlRect(), optimizationFoldout, optimizationLabelContent, true, EditorStyles.foldout);

            if (optimizationFoldout)
            {
                vaoScript.AdaptiveType = (VAOEffectCommandBuffer.AdaptiveSamplingType)EditorGUILayout.EnumPopup(adaptiveSamplingLabelContent, vaoScript.AdaptiveType);
                if (vaoScript.AdaptiveType == VAOEffectCommandBuffer.AdaptiveSamplingType.EnabledManual)
                {
                    vaoScript.AdaptiveQualityCoefficient = EditorGUILayout.Slider(adaptiveLevelLabelContent, vaoScript.AdaptiveQualityCoefficient, 0.5f, 2.0f);
                }
                vaoScript.CullingPrepassMode = (VAOEffectCommandBuffer.CullingPrepassModeType)EditorGUILayout.EnumPopup(cullingPrepassTypeLabelContent, vaoScript.CullingPrepassMode);
                vaoScript.Downsampling = EditorGUILayout.IntPopup(downsamplingLabelContent, vaoScript.Downsampling, downsamplingTexts, downsamplingInts);
                vaoScript.HierarchicalBufferState = (VAOEffectCommandBuffer.HierarchicalBufferStateType)EditorGUILayout.IntPopup(hzbContent, (int)vaoScript.HierarchicalBufferState, hzbTexts, hzbInts);
                EditorGUILayout.Space();
            }
            pipelineFoldout = EditorGUI.Foldout(EditorGUILayout.GetControlRect(), pipelineFoldout, pipelineLabelContent, true, EditorStyles.foldout);
            if (pipelineFoldout)
            {
                vaoScript.CommandBufferEnabled = EditorGUILayout.Toggle(commandBufferLabelContent, vaoScript.CommandBufferEnabled);

                EditorGUILayout.Space();

                EditorGUILayout.LabelField("Deferred rendering path" + (camera.actualRenderingPath == RenderingPath.DeferredShading ? " (active):" : ":"));
                EditorGUI.indentLevel++;
                vaoScript.VaoCameraEvent = (VAOEffectCommandBuffer.VAOCameraEventType)EditorGUILayout.EnumPopup(cameraEventLabelContent, vaoScript.VaoCameraEvent);
                DisplayCamerasEventStateMessage(vaoScript);
                vaoScript.UseGBuffer = EditorGUILayout.Toggle(gBufferLabelContent, vaoScript.UseGBuffer);

                DisplayGBufferStateMessage(vaoScript);

                EditorGUI.indentLevel--;
                EditorGUILayout.Space();

                EditorGUILayout.LabelField("Forward rendering path" + (camera.actualRenderingPath == RenderingPath.Forward ? " (active):" : ":"));
                EditorGUI.indentLevel++;
                vaoScript.UsePreciseDepthBuffer = EditorGUILayout.Toggle(dedicatedDepthBufferLabelContent, vaoScript.UsePreciseDepthBuffer);
                EditorGUI.indentLevel--;

                EditorGUILayout.Space();

                vaoScript.FarPlaneSource = (VAOEffectCommandBuffer.FarPlaneSourceType)EditorGUILayout.EnumPopup(farPlaneSourceLabelContent, vaoScript.FarPlaneSource);
            }


            EditorGUI.indentLevel--;
            EditorGUILayout.Space();

            HeaderWithToggle(lumaEnabledLabelContent.text, ref lumaFoldout, ref vaoScript.IsLumaSensitive);

            if (vaoScript.IsLumaSensitive)
            {
                if (lastLumaSensitive == false) lumaFoldout = true;
            }

            EditorGUILayout.Space();
            EditorGUI.indentLevel++;

            if (lumaFoldout)
            {
                EditorGUI.BeginDisabledGroup(!vaoScript.IsLumaSensitive);

                vaoScript.LuminanceMode = (VAOEffectCommandBuffer.LuminanceModeType)EditorGUILayout.EnumPopup(luminanceModeLabelContent, vaoScript.LuminanceMode);

                lumaMaxFx = 1.0f;
                float tresholdMax = lumaMaxFx;
                float kneeWidthMax = lumaMaxFx;
                GUIContent tresholdLabel = lumaThresholdLabelContent;
                if (camera != null)
                {
                    if (isCameraHDR(camera)) //< Check current setting and update isHDR variable if needed
                    {
                        lumaMaxFx = 10.0f;
                        tresholdMax = lumaMaxFx;
                        kneeWidthMax = lumaMaxFx;
                        tresholdLabel = lumaThresholdHDRLabelContent;

                        if (!isHDR)
                        {
                            vaoScript.LumaThreshold *= 10.0f;
                            vaoScript.LumaKneeWidth *= 10.0f;
                            isHDR = true;
                        }
                    }
                    else
                    {
                        if (isHDR)
                        {
                            vaoScript.LumaThreshold *= 0.1f;
                            vaoScript.LumaKneeWidth *= 0.1f;
                            isHDR = false;
                        }
                    }
                }

                vaoScript.LumaThreshold = EditorGUILayout.Slider(tresholdLabel, vaoScript.LumaThreshold, 0.0f, tresholdMax);
                vaoScript.LumaKneeWidth = EditorGUILayout.Slider(lumaWidthLabelContent, vaoScript.LumaKneeWidth, 0.0f, kneeWidthMax);
                vaoScript.LumaKneeLinearity = EditorGUILayout.Slider(lumaSoftnessLabelContent, vaoScript.LumaKneeLinearity, 1.0f, 10.0f);
                EditorGUILayout.Space();
                lumaGraphWidget.Draw(GetLumaGraphWidgetParameters(vaoScript));

                EditorGUI.EndDisabledGroup();
            }

            EditorGUI.indentLevel--;
            EditorGUILayout.Space();

            vaoScript.Mode = (VAOEffectCommandBuffer.EffectMode)EditorGUILayout.EnumPopup(effectModeLabelContent, vaoScript.Mode);

            EditorGUI.indentLevel++;
            switch (vaoScript.Mode)
            {
                case VAOEffectCommandBuffer.EffectMode.Simple:
                    break;
                case VAOEffectCommandBuffer.EffectMode.ColorTint:
                    EditorGUILayout.Space();
                    vaoScript.ColorTint = EditorGUILayout.ColorField(colorTintLabelContent, vaoScript.ColorTint);
                    break;
                case VAOEffectCommandBuffer.EffectMode.ColorBleed:
                    EditorGUILayout.Space();
                    colorBleedFoldout = EditorGUI.Foldout(EditorGUILayout.GetControlRect(), colorBleedFoldout, colorBleedLabelContent, true, EditorStyles.foldout);

                    if (lastEffectMode != VAOEffectCommandBuffer.EffectMode.ColorBleed) colorBleedFoldout = true;

                    if (colorBleedFoldout)
                    {
                        vaoScript.ColorBleedPower = EditorGUILayout.FloatField(colorBleedPowerLabelContent, vaoScript.ColorBleedPower);
                        vaoScript.ColorBleedPresence = EditorGUILayout.Slider(colorBleedPresenceLabelContent, vaoScript.ColorBleedPresence, 0.0f, 1.0f);

                        if (vaoScript.CommandBufferEnabled)
                        {
                            vaoScript.IntermediateScreenTextureFormat = (VAOEffect.ScreenTextureFormat)EditorGUILayout.EnumPopup(screenFormatLabelContent, vaoScript.IntermediateScreenTextureFormat);
                        }

                        vaoScript.ColorbleedHueSuppresionEnabled = EditorGUILayout.ToggleLeft(colorBleedSameHueAttenuationLabelContent, vaoScript.ColorbleedHueSuppresionEnabled);

                        if (EditorGUILayout.BeginFadeGroup(vaoScript.ColorbleedHueSuppresionEnabled ? 1.0f : 0.0f))
                        {
                            EditorGUI.indentLevel++;
                            EditorGUILayout.LabelField(colorBleedSameHueAttenuationHueFilterLabelContent);
                            EditorGUI.indentLevel++;
                            vaoScript.ColorBleedHueSuppresionThreshold = EditorGUILayout.Slider(colorBleedSameHueAttenuationHueToleranceLabelContent, vaoScript.ColorBleedHueSuppresionThreshold, 0.0f, 50.0f);
                            vaoScript.ColorBleedHueSuppresionWidth = EditorGUILayout.Slider(colorBleedSameHueAttenuationHueSoftnessLabelContent, vaoScript.ColorBleedHueSuppresionWidth, 0.0f, 10.0f);
                            EditorGUI.indentLevel--;
                            EditorGUILayout.LabelField(colorBleedSameHueAttenuationSaturationFilterLabelContent);
                            EditorGUI.indentLevel++;
                            vaoScript.ColorBleedHueSuppresionSaturationThreshold = EditorGUILayout.Slider(colorBleedSameHueAttenuationSaturationToleranceLabelContent, vaoScript.ColorBleedHueSuppresionSaturationThreshold, 0.0f, 1.0f);
                            vaoScript.ColorBleedHueSuppresionSaturationWidth = EditorGUILayout.Slider(colorBleedSameHueAttenuationSaturationSoftnessLabelContent, vaoScript.ColorBleedHueSuppresionSaturationWidth, 0.0f, 1.0f);
                            vaoScript.ColorBleedHueSuppresionBrightness = EditorGUILayout.Slider(colorBleedSameHueAttenuationBrightnessLabelContent, vaoScript.ColorBleedHueSuppresionBrightness, 0.0f, 1.0f);
                            EditorGUI.indentLevel--;
                            EditorGUI.indentLevel--;
                            EditorGUILayout.Space();
                        }
                        EditorGUILayout.EndFadeGroup();


                        vaoScript.ColorBleedQuality = EditorGUILayout.IntPopup(colorBleedQualityLabelContent, vaoScript.ColorBleedQuality, giTexts, giInts);
                        vaoScript.ColorBleedSelfOcclusionFixLevel = (VAOEffectCommandBuffer.ColorBleedSelfOcclusionFixLevelType)EditorGUILayout.EnumPopup(ColorBleedSelfOcclusionLabelContent, vaoScript.ColorBleedSelfOcclusionFixLevel);
                        vaoScript.GiBackfaces = EditorGUILayout.Toggle(backfaceLabelContent, vaoScript.GiBackfaces);
                    }
                    break;
                default:
                    break;
            }
            EditorGUI.indentLevel--;

            EditorGUILayout.Space();

            vaoScript.BlurQuality = (VAOEffectCommandBuffer.BlurQualityType)EditorGUILayout.EnumPopup(blurQualityContent, vaoScript.BlurQuality);
            vaoScript.BlurMode = (VAOEffectCommandBuffer.BlurModeType)EditorGUILayout.EnumPopup(blurModeContent, vaoScript.BlurMode);

            if (vaoScript.BlurMode == VAOEffectCommandBuffer.BlurModeType.Enhanced)
            {
                EditorGUILayout.Space();
                EditorGUI.indentLevel++;
                blurFoldout = EditorGUI.Foldout(EditorGUILayout.GetControlRect(), blurFoldout, blurFoldoutContent, true, EditorStyles.foldout);
                if (blurFoldout)
                {
                    vaoScript.EnhancedBlurSize = EditorGUILayout.IntSlider(blurSizeContent, vaoScript.EnhancedBlurSize, 3, 17);
                    vaoScript.EnhancedBlurDeviation = EditorGUILayout.Slider(blurDeviationContent, vaoScript.EnhancedBlurDeviation, 0.01f, 3.0f);
                }
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.Space();

            vaoScript.OutputAOOnly = EditorGUILayout.Toggle(aoLabelContent, vaoScript.OutputAOOnly);
            aboutFoldout = EditorGUI.Foldout(EditorGUILayout.GetControlRect(), aboutFoldout, "About", true, EditorStyles.foldout);
            if (aboutFoldout)
            {
                EditorGUILayout.HelpBox("Volumetric Ambient Occlusion v1.9 by Project Wilberforce.\n\nThank you for your purchase and if you have any questions, issues or suggestions, feel free to contact us at <projectwilberforce@gmail.com>.", MessageType.Info);
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Contact Support"))
                {
                    Application.OpenURL("mailto:projectwilberforce@gmail.com");
                }
                //if (GUILayout.Button("Rate on Asset Store"))
                //{
                //    Application.OpenURL("https://www.assetstore.unity3d.com/en/#!/account/downloads/search=Volumetric%20Ambient%20Occlusion");
                //}
                if (GUILayout.Button("Asset Store Page"))
                {
                    Application.OpenURL("http://u3d.as/xzs"); //< Official Unity shortened link to asset store page of VAO
                }
                EditorGUILayout.EndHorizontal();
            }
            lastLumaSensitive = vaoScript.IsLumaSensitive;
            lastEffectMode = vaoScript.Mode;

            if (GUI.changed)
            {
                // Force parameters to be positive
                vaoScript.Radius = Mathf.Clamp(vaoScript.Radius, 0.001f, float.MaxValue);
                vaoScript.Power = Mathf.Clamp(vaoScript.Power, 0, float.MaxValue);
                vaoScript.ColorBleedPower = Mathf.Clamp(vaoScript.ColorBleedPower, 0, float.MaxValue);

                if (vaoScript.Quality == 64 && vaoScript.AdaptiveType != VAOEffectCommandBuffer.AdaptiveSamplingType.Disabled)
                {
                    vaoScript.Quality = 32;
                }

                if (vaoScript.EnhancedBlurSize % 2 == 0)
                {
                    vaoScript.EnhancedBlurSize += 1;
                }

                // Mark as dirty
                EditorUtility.SetDirty(target);
            }
            Undo.RecordObject(target, "VAO change");
        }

        void DisplayCamerasEventStateMessage(VAOEffectCommandBuffer vaoScript)
        {
            if (vaoScript.Mode == VAOEffectCommandBuffer.EffectMode.ColorBleed &&
                vaoScript.CommandBufferEnabled &&
                vaoScript.VaoCameraEvent != VAOEffectCommandBuffer.VAOCameraEventType.BeforeImageEffectsOpaque)
            {
                EditorGUILayout.HelpBox("Cannot use selected cmd. buffer integration stage. Only BeforeImageEffectsOpaque is supported in color bleeding mode.", MessageType.Warning);
            }

        }

        void DisplayGBufferStateMessage(VAOEffectCommandBuffer vaoScript)
        {

            if (!vaoScript.UseGBuffer && vaoScript.ShouldUseGBuffer())
            {
                string reason = "";

                if (vaoScript.VaoCameraEvent != VAOEffectCommandBuffer.VAOCameraEventType.BeforeImageEffectsOpaque)
                {
                    reason = " Command buffer integration is different than BeforeImageEffectsOpaque.";
                }

#if UNITY_2017_1_OR_NEWER
                if (camera != null && camera.stereoEnabled
                    && PlayerSettings.stereoRenderingPath == StereoRenderingPath.SinglePass
                    && camera.actualRenderingPath == RenderingPath.DeferredShading)
                {
                    reason = " You are running in single pass stereo mode which requires G-Buffer inputs.";
                }
#endif
                EditorGUILayout.HelpBox("Cannot turn G-Buffer depth&normals off, because current configuration requires it to be enabled." + reason, MessageType.Warning);
            }

        }


        #region Custom header styles

        // Custom header foldout styles
        private static GUIStyle _customFoldoutStyle;
        private static GUIStyle customFoldoutStyle
        {
            get
            {
                if (_customFoldoutStyle == null)
                {
                    _customFoldoutStyle = new GUIStyle(EditorStyles.foldout)
                    {
                        fixedWidth = 12.0f
                    };
                }

                return _customFoldoutStyle;
            }
        }

        private static GUIStyle _customFoldinStyle;
        private static GUIStyle customFoldinStyle
        {
            get
            {
                if (_customFoldinStyle == null)
                {
                    _customFoldinStyle = new GUIStyle(EditorStyles.foldout)
                    {
                        fixedWidth = 12.0f
                    };

                    _customFoldinStyle.normal = _customFoldinStyle.onNormal;
                }

                return _customFoldinStyle;
            }
        }

        private static GUIStyle _customToggleStyle;
        private static GUIStyle customToggleStyle
        {
            get
            {
                if (_customToggleStyle == null)
                {
                    _customToggleStyle = new GUIStyle(EditorStyles.toggle)
                    {
                        fixedWidth = 12.0f
                    };
                }

                return _customToggleStyle;
            }
        }

        private static GUIStyle _customIconStyle;
        private static GUIStyle customIconStyle
        {
            get
            {
                if (_customIconStyle == null)
                {
                    _customIconStyle = new GUIStyle(EditorStyles.label)
                    {
                        fixedWidth = 18.0f,
                        fixedHeight = 18.0f
                    };
                }

                return _customIconStyle;
            }
        }


        private static GUIStyle _customLabelStyle;
        private static GUIStyle customLabelStyle
        {
            get
            {
                if (_customLabelStyle == null)
                {
                    _customLabelStyle = new GUIStyle(EditorStyles.label)
                    {
                        fontStyle = FontStyle.Normal
                    };
                }

                return _customLabelStyle;
            }
        }

        #endregion

        private static void HeaderWithToggle(string label, ref bool isFoldout, ref bool isEnabled, Texture icon = null, bool clickableIcon = true)
        {

            EditorGUILayout.BeginHorizontal();
            isFoldout = GUILayout.Toggle(isFoldout, "", isFoldout ? customFoldinStyle : customFoldoutStyle);
            isEnabled = GUILayout.Toggle(isEnabled, "", customToggleStyle);

            if (icon != null)
            {
                if (clickableIcon)
                    isFoldout = GUILayout.Toggle(isFoldout, icon, customLabelStyle);
                else
                    GUILayout.Label(icon);

                GUILayout.Space(-5.0f);
            }

            isFoldout = GUILayout.Toggle(isFoldout, label, customLabelStyle);
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }

    }

    #region Graph Widget

    public class GraphWidgetLine
    {
        public Vector3 From { get; set; }
        public Vector3 To { get; set; }
        public Color Color { get; set; }
        public float Thickness { get; set; }
    }

    public class GraphWidgetDrawingParameters
    {

        public IList<GraphWidgetLine> Lines { get; set; }

        /// <summary>
        /// Number of line segments that will be used to approximate function shape
        /// </summary>
        public uint GraphSegmentsCount { get; set; }

        /// <summary>
        /// Function to draw (X -> Y) 
        /// </summary>
        public Func<float, float> GraphFunction { get; set; }

        public Color GraphColor { get; set; }
        public float GraphThickness { get; set; }

        public float YScale { get; internal set; }
        public float MinY { get; internal set; }

        public int GridLinesXCount { get; set; }
        public float MaxFx { get; internal set; }

        public string LabelText { get; set; }
    }

    public class GraphWidget
    {
        private Vector3[] transformedLinePoints = new Vector3[2];
        private Vector3[] graphPoints;

        void TransformToRect(Rect rect, ref Vector3 v)
        {
            v.x = Mathf.Lerp(rect.x, rect.xMax, v.x);
            v.y = Mathf.Lerp(rect.yMax, rect.y, v.y);
        }

        private void DrawLine(Rect rect, float x1, float y1, float x2, float y2, Color color)
        {
            transformedLinePoints[0].x = x1;
            transformedLinePoints[0].y = y1;
            transformedLinePoints[1].x = x2;
            transformedLinePoints[1].y = y2;

            TransformToRect(rect, ref transformedLinePoints[0]);
            TransformToRect(rect, ref transformedLinePoints[1]);

            Handles.color = color;
            Handles.DrawPolyLine(transformedLinePoints);
        }

        private void DrawAALine(Rect rect, float thickness, float x1, float y1, float x2, float y2, Color color)
        {
            transformedLinePoints[0].x = x1;
            transformedLinePoints[0].y = y1;
            transformedLinePoints[1].x = x2;
            transformedLinePoints[1].y = y2;

            TransformToRect(rect, ref transformedLinePoints[0]);
            TransformToRect(rect, ref transformedLinePoints[1]);

            Handles.color = color;
            Handles.DrawPolyLine(transformedLinePoints);
        }

        public void Draw(GraphWidgetDrawingParameters drawingParameters)
        {
            Handles.color = Color.white; //< Reset to white to avoid Unity bugs

            Rect bgRect = GUILayoutUtility.GetRect(128, 70);
            Handles.DrawSolidRectangleWithOutline(bgRect, Color.grey, Color.black);

            // Draw grid lines
            Color gridColor = Color.black * 0.1f;
            DrawLine(bgRect, 0.0f, drawingParameters.MinY + drawingParameters.YScale,
                             1.0f, drawingParameters.MinY + drawingParameters.YScale, gridColor);

            DrawLine(bgRect, 0.0f, drawingParameters.MinY,
                             1.0f, drawingParameters.MinY, gridColor);

            float gridXStep = 1.0f / (drawingParameters.GridLinesXCount + 1);
            float gridX = gridXStep;
            for (int i = 0; i < drawingParameters.GridLinesXCount; i++)
            {
                DrawLine(bgRect, gridX, 0.0f,
                                 gridX, 1.0f, gridColor);

                gridX += gridXStep;
            }

            if (drawingParameters.GraphSegmentsCount > 0)
            {
                if (graphPoints == null || graphPoints.Length < drawingParameters.GraphSegmentsCount + 1)
                    graphPoints = new Vector3[drawingParameters.GraphSegmentsCount + 1];

                float x = 0.0f;
                float xStep = 1.0f / drawingParameters.GraphSegmentsCount;

                for (int i = 0; i < drawingParameters.GraphSegmentsCount + 1; i++)
                {
                    float y = drawingParameters.GraphFunction(x * drawingParameters.MaxFx);

                    y *= drawingParameters.YScale;
                    y += drawingParameters.MinY;

                    graphPoints[i].x = x;
                    graphPoints[i].y = y;
                    TransformToRect(bgRect, ref graphPoints[i]);
                    x += xStep;
                }

                Handles.color = drawingParameters.GraphColor;
                Handles.DrawAAPolyLine(drawingParameters.GraphThickness, graphPoints);
            }

            if (drawingParameters != null && drawingParameters.Lines != null)
            {
                foreach (var line in drawingParameters.Lines)
                {
                    DrawAALine(bgRect, line.Thickness, line.From.x, line.From.y, line.To.x, line.To.y, line.Color);
                }
            }

            // Label
            Vector3 labelPosition = new Vector3(0.01f, 0.99f);
            TransformToRect(bgRect, ref labelPosition);
            Handles.Label(labelPosition, drawingParameters.LabelText, EditorStyles.miniLabel);

        }

    }

    #endregion

#endif
}
