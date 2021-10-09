// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Copyright (c) 2016-2018 Jakub Boksansky, Adam Pospisil - All Rights Reserved
// Volumetric Ambient Occlusion Unity Plugin 1.9

Shader "Hidden/Wilberforce/VAOShader"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
	}
	CGINCLUDE
		#pragma target 3.0

		#pragma multi_compile WFORCE_VAO_COLORBLEED_OFF WFORCE_VAO_COLORBLEED_ON	
		
		#include "UnityCG.cginc"
		
		// ========================================================================
		// Uniform definitions 
		// ========================================================================

		// Unity built-ins
		sampler2D _MainTex;
		float4 _MainTex_TexelSize;
		float4 _ProjInfo;
		float4 _MainTex_ST;

		sampler2D _CameraDepthNormalsTexture;
		float4 _CameraDepthNormalsTexture_ST;
		sampler2D _CameraGBufferTexture2;
		sampler2D _CameraDepthTexture;
		
		// Culling pre-pass
		sampler2D cullingPrepassTexture;
		uniform int cullingPrepassMode;
		uniform float2 cullingPrepassTexelSize;

		// Projection matrices
		uniform float4x4 invProjMatrix;
		uniform float4 screenProjection;

		// Radius setting
		uniform float halfRadius;
		uniform float halfRadiusSquared;
		uniform float radius;

		// Sample set
		uniform int sampleCount;
		uniform int fourSamplesStartIndex;
		uniform float4 samples[70];
		uniform float4 eightSamples[70];
		
		// AO appearance
		uniform float aoPower;
		uniform float aoPresence;
		uniform float4 colorTint;
		
		// AO radius limits
		uniform int maxRadiusEnabled;
		uniform float maxRadiusCutoffDepth;
		
		uniform int minRadiusEnabled;
		uniform float minRadiusCutoffDepth;
		uniform float minRadiusSoftness;

		uniform float subpixelRadiusCutoffDepth;

		// GI appearance
		uniform float giPower;
		uniform float giPresence;
		uniform float giSameHueAttenuationBrightness;
		uniform int giQuality;
		uniform int giBackfaces;
		uniform	int giBlur;
		uniform int giSelfOcclusionFix;

		uniform int giSameHueAttenuationEnabled;
		uniform float giSameHueAttenuationThreshold;
		uniform float giSameHueAttenuationWidth;
		uniform float giSameHueAttenuationSaturationThreshold;
		uniform float giSameHueAttenuationSaturationWidth;

		// Adaptive sampling settings
		uniform float adaptiveMin;
		uniform float adaptiveMax;
		uniform int adaptiveMode;
		uniform int minLevelIndex;
		uniform float quarterResBufferMaxDistance;
		uniform float halfResBufferMaxDistance;

		static const int adaptiveLengths[16] = { 32, 16, 16, 8, 8, 8, 8, 4, 4, 4, 4, 4, 4, 4, 4, /*2, */4 };
		static const int adaptiveStarts[16] = { 0, 32, 32, 48, 48, 48, 48, 56, 56, 56, 56, 56, 56, 56, 56, /*60,*/ 56 };
		
        // Hierarchical Buffers 
        sampler2D depthNormalsTexture2;
        sampler2D depthNormalsTexture4;

		// Luma settings
		uniform float LumaThreshold;
		uniform float LumaKneeWidth;
		uniform float LumaTwiceKneeWidthRcp;
		uniform float LumaKneeLinearity;
		uniform int LumaMode;
		
		// Blur settings
		uniform int enhancedBlurSize;
		uniform float4 gauss[99];
		uniform float gaussWeight;
		uniform float blurDepthThreshold;

		// Utilities
		sampler2D textureAO;
		uniform float2 texelSize;
		uniform float cameraFarPlane;
		uniform int UseCameraFarPlane;
		uniform float projMatrix11;
		uniform float maxRadiusOnScreen;
		sampler2D noiseTexture;		
		uniform float2 noiseTexelSizeRcp;
		uniform int flipY;
		uniform int useGBuffer;
		uniform int hierarchicalBufferEnabled;
		uniform int hwBlendingEnabled;
		uniform int useLogEmissiveBuffer;
		uniform int useLogBufferInput;
		uniform int outputAOOnly;
		uniform int useFastBlur;
		uniform int isLumaSensitive;
		uniform int useDedicatedDepthBuffer;
		sampler2D emissionTexture;		
		sampler2D occlusionTexture;		
		
		// ========================================================================
		// Structs definitions 
		// ========================================================================

		struct v2fShed {
			float4 pos : SV_POSITION;
			#ifdef UNITY_SINGLE_PASS_STEREO
			float4 shed2 : TEXCOORD2;
			#endif
			float4 shed : TEXCOORD1;
			float2 uv : TEXCOORD0;
		};
		
		struct v2fSingle {
			float4 pos : SV_POSITION;
			float2 uv : TEXCOORD0;
		};

		struct v2fDouble {
			float4 pos : SV_POSITION;
			float2 uv[2] : TEXCOORD0;
		};

		struct AoOutput
		{
			half4 albedoAO : SV_Target0;
			half4 emissionLighting : SV_Target1;
		};

		// ========================================================================
		// Defines
		// ========================================================================

		#ifdef SHADER_API_D3D9
			#define WFORCE_VAO_MAIN_PASS_RETURN_TYPE half4
			#define WFORCE_VAO_WHITE half4(1.0f, 1.0f, 1.0f, 1.0f)
			#define WFORCE_VAO_BLACK half4(0.0f, 1.0f, 1.0f, 1.0f)
		#else
			#ifdef WFORCE_VAO_COLORBLEED_OFF
				#define WFORCE_VAO_MAIN_PASS_RETURN_TYPE half2
				#define WFORCE_VAO_WHITE half2(1.0f, 1.0f)
				#define WFORCE_VAO_BLACK half2(0.0f, 1.0f)
			#else
				#define WFORCE_VAO_MAIN_PASS_RETURN_TYPE half4
				#define WFORCE_VAO_WHITE half4(1.0f, 1.0f, 1.0f, 1.0f)
				#define WFORCE_VAO_BLACK half4(0.0f, 1.0f, 1.0f, 1.0f)
			#endif
		#endif
		
		#ifndef SHADER_API_GLCORE
		#ifndef SHADER_API_OPENGL
		#ifndef SHADER_API_GLES
		#ifndef SHADER_API_GLES3
		#ifndef SHADER_API_VULKAN
			#define WFORCE_VAO_OPENGL_OFF
		#endif
		#endif
		#endif
		#endif
		#endif
		
		// ========================================================================
		// Helper functions
		// ========================================================================
		
		float3 RGBToHSV(float3 rgb)
		{
			// result.x = Hue			[0.0, 360.0] in degrees
			// result.y = Saturation	[0.0, 1.0]
			// result.z = Value			[0.0, 1.0]

			float3 result = float3(0.0f, 0.0f, 0.0f);

			float cMax = max(rgb.r, max(rgb.g, rgb.b));
			float cMin = min(rgb.r, min(rgb.g, rgb.b));

			float delta = cMax - cMin;

			if (cMax > 0.000001f) {
				result.y = delta / cMax;
					
				if (rgb.r == cMax) {
					result.x = (rgb.g - rgb.b) / delta;			// between yellow & magenta
				} else if (rgb.g == cMax) {
					result.x = 2.0f + (rgb.b - rgb.r) / delta;	// between cyan & yellow
				} else {
					result.x = 4.0f + (rgb.r - rgb.g) / delta;	// between magenta & cyan
				}

				result.x *= 60.0f;			

				if(result.x < 0.0f) result.x += 360.0f;

				result.z = cMax;

				return result;

			} else {
				// Undefined (grey - saturation is zero)
				return float3(0.0f, 0.0f, cMax);
			}

		}
			
		float3 fetchNormal(float2 uv) {
			float3 normal = mul((float3x3)unity_WorldToCamera, tex2Dlod(_CameraGBufferTexture2, float4(uv, 0.0f, 0.0f)).xyz * 2.0f - 1.0f);
			normal.z = -normal.z;

			return normal;
		}

		float fetchDepth(float2 uv, float farPlane) {
			if (useGBuffer == 0) {
				return -DecodeFloatRG(tex2Dlod(_CameraDepthNormalsTexture, float4(uv, 0.0f, 0.0f)).zw) * farPlane;
			} else {
				return -Linear01Depth(tex2Dlod(_CameraDepthTexture, float4(uv, 0.0f, 0.0f)).r) * farPlane;
			}
		}

		void fetchDepthNormal(float2 uv, out float depth, out float3 normal) {
			if (useGBuffer == 0) {
				DecodeDepthNormal(tex2Dlod(_CameraDepthNormalsTexture, float4(uv, 0.0f, 0.0f)), depth, normal);

				if (useDedicatedDepthBuffer != 0) {
					depth = Linear01Depth(tex2Dlod(_CameraDepthTexture, float4(uv, 0.0f, 0.0f)).r);
				}

			} else {
				depth = Linear01Depth(tex2Dlod(_CameraDepthTexture, float4(uv, 0.0f, 0.0f)).r);
				normal = fetchNormal(uv);
			}
		}

		inline void decodeDepth(float4 enc, out float depth)
		{
			depth = DecodeFloatRG(enc.zw);
		}

		float getFarPlane(int useCameraFarPlane) {
			if (useCameraFarPlane != 0) {
				return cameraFarPlane;
			} else {
				return _ProjectionParams.z;
			}
		}
		
		float luma(float3 color) {
			return 0.299f * color.r + 0.587f * color.g + 0.114f * color.b;
		}

		float getRadiusForDepthAndScreenRadius(float pixelDepth, float maxRadiusOnScreen) {
			return -(pixelDepth * maxRadiusOnScreen) / projMatrix11;
		}
		
		#ifdef WFORCE_VAO_COLORBLEED_ON
		void applyLuma(float3 mainColor, float accIn, out float acc, float3 giIn, out float3 gi) {
		#else
		void applyLuma(float3 mainColor, float accIn, out float acc) {
		#endif	
			
			float Y;
			if (LumaMode == 1) {
				Y = luma(mainColor);
			} else {
				Y = max(max(mainColor.r, mainColor.g), mainColor.b);
			}

			Y = (Y - (LumaThreshold - LumaKneeWidth)) * LumaTwiceKneeWidthRcp;
			float x = min(1.0f, max(0.0f, Y));
			float n = ((-pow(x, LumaKneeLinearity) + 1));
			acc = lerp(1.0f, accIn, n);
				
			#ifdef WFORCE_VAO_COLORBLEED_ON
				gi = lerp(float3(1.0f, 1.0f, 1.0f), giIn, n);
			#endif
		}
		
		float3 processGi(float3 mainColor, float3 giIn, float ao) {
			float3 gi = giIn;

			float sMax = max(gi.r, max(gi.g, gi.b));
			float sMin = min(gi.r, min(gi.g, gi.b));
			float sat = 0.0f;
			if (sMax > 0.01f) sat = (sMax - sMin) / sMax;
			float _satMapped = 1.0f - (sat*giPresence);
			gi = lerp(float3(1.0f, 1.0f, 1.0f), gi, _satMapped);

			gi *= ao / max(max(gi.x, gi.y),gi.z); //< Tatry su krute

			// Sme hue attenuation 
			if (giSameHueAttenuationEnabled != 0) {

				float giHue = RGBToHSV(gi).x;
				float3 mainHSV = RGBToHSV(mainColor);
				float mainHue = mainHSV.x;
				float mainValue = mainHSV.y;
				float hueDiff = abs(giHue - mainHue);
				float3 giHueSuppressed = lerp(float3(1, 1, 1), gi, smoothstep(giSameHueAttenuationThreshold - giSameHueAttenuationWidth,  giSameHueAttenuationThreshold + giSameHueAttenuationWidth, hueDiff));
				gi = lerp(gi, giHueSuppressed, smoothstep(giSameHueAttenuationSaturationThreshold - giSameHueAttenuationSaturationWidth,  giSameHueAttenuationSaturationThreshold + giSameHueAttenuationSaturationWidth, mainValue));
				gi = lerp(gi, gi * (1.0f / max(max(gi.x, gi.y),gi.z)), giSameHueAttenuationBrightness);

			}

			return gi;
		}

		#ifdef WFORCE_VAO_COLORBLEED_OFF
		half downscaleDepthNormals(v2fDouble input) {
		#else
		half4 downscaleDepthNormals(v2fDouble input) {
		#endif

			#ifdef WFORCE_VAO_COLORBLEED_OFF

				float depth;

				if (useGBuffer == 0 && useDedicatedDepthBuffer == 0) {
					decodeDepth(tex2Dlod(_CameraDepthNormalsTexture, float4(input.uv[1], 0.0f, 0.0f)), depth);
				} else {
					depth = Linear01Depth(tex2Dlod(_CameraDepthTexture, float4(input.uv[1], 0.0f, 0.0f)).r);
				}
				
				return depth;

			#else

				float depth;
				float3 normal;

				fetchDepthNormal(input.uv[1], depth, normal);
				
				return EncodeDepthNormal(depth, normal);

			#endif
		}

		void calculateColorBleed(float2 sampleScreenSpacePosition, float sampleDepth, float3 pixelViewSpaceNormal, float3 sampleViewSpaceNormal, float3 sampleViewSpacePosition, float3 pixelViewSpacePosition, float3 farPlane, int ii, int j, out float3 gi, out float giCount) {

			gi = float3(0.0f, 0.0f, 0.0f);
			giCount = 0.0f;

			float cosineNormals = dot(sampleViewSpaceNormal, pixelViewSpaceNormal);

#if SHADER_API_D3D9
			if ((giQuality == 1) ||
				((((j + 1) / giQuality) * giQuality == (j + 1)))) {
#else
			if (giQuality == 1 || (giQuality == 2 && (((ii + 1) & 1) == 0)) || (giQuality == 4 && (((ii + 1) & 3) == 0))) {
#endif
				sampleViewSpacePosition.z = sampleDepth;
				float3 cameraRay = sampleViewSpacePosition - pixelViewSpacePosition;
				float sampleDistance = length(cameraRay);
				cameraRay = normalize(cameraRay);

				if (sampleDistance > radius || cosineNormals > 0.95f) {
					gi = float3(1, 1, 1);
				}
				else {

					float weight = pow(1.0f - (sampleDistance / radius), 2);

					if (giBackfaces == 0) {
						float _alpha = min(0.0f, -dot(sampleViewSpaceNormal, cameraRay));
						weight *= (_alpha + 1.0f) / 2.0f;
					}

					if (giSelfOcclusionFix == 2) {
						weight *= max(0.0f, dot(pixelViewSpaceNormal, cameraRay));
					}
					else if (giSelfOcclusionFix == 1) {
						if (dot(pixelViewSpaceNormal, cameraRay) < 0.0f) weight = 0.0f;
					}

					if (weight < 0.1f) {
						gi = float3(1, 1, 1);
					}
					else {

#if UNITY_UV_STARTS_AT_TOP
						if (_MainTex_TexelSize.y < 0)
							sampleScreenSpacePosition.y = 1.0f - sampleScreenSpacePosition.y;
#endif

#if UNITY_VERSION < 560
#ifdef WFORCE_VAO_OPENGL_OFF
						if (flipY != 0) {
							sampleScreenSpacePosition.y = 1.0f - sampleScreenSpacePosition.y;
						}
#endif
#endif

						float3 color = tex2Dlod(_MainTex, float4(sampleScreenSpacePosition.xy, 0, 0)).rgb;

						gi = lerp(float3(1, 1, 1), color, weight);
					}
				}

				giCount = 1.0f;
			}
			}

#ifdef WFORCE_VAO_COLORBLEED_OFF
		void getSampleDepthNormalTopLevel(float2 sampleScreenSpacePosition, out float sampleDepth) {
#else
		void getSampleDepthNormalTopLevel(float2 sampleScreenSpacePosition, out float sampleDepth, out float3 sampleViewSpaceNormal) {
			sampleViewSpaceNormal = float3(1.0f, 1.0f, 1.0f); //< Satisfy OpenGL compiler
#endif

			if (useGBuffer == 0) {

#ifdef WFORCE_VAO_COLORBLEED_ON
				DecodeDepthNormal(tex2Dlod(_CameraDepthNormalsTexture, float4(sampleScreenSpacePosition.xy, 0, 0)), sampleDepth, sampleViewSpaceNormal);
#else
				if (useDedicatedDepthBuffer == 0) decodeDepth(tex2Dlod(_CameraDepthNormalsTexture, float4(sampleScreenSpacePosition.xy, 0, 0)), sampleDepth);
#endif

				if (useDedicatedDepthBuffer != 0) {
					sampleDepth = Linear01Depth(tex2Dlod(_CameraDepthTexture, float4(sampleScreenSpacePosition.xy, 0, 0)).r);
				}

			}
			else {
				sampleDepth = Linear01Depth(tex2Dlod(_CameraDepthTexture, float4(sampleScreenSpacePosition.xy, 0, 0)).r);

#ifdef WFORCE_VAO_COLORBLEED_ON
				sampleViewSpaceNormal = fetchNormal(sampleScreenSpacePosition.xy);
#endif

			}

		}

#ifdef WFORCE_VAO_COLORBLEED_OFF
		void getSampleDepthNormal(float2 sampleScreenSpacePosition, float farPlane, int level, out float sampleDepth) {
#else
		void getSampleDepthNormal(float2 sampleScreenSpacePosition, float farPlane, int level, out float sampleDepth, out float3 sampleViewSpaceNormal) {
#endif

			if (hierarchicalBufferEnabled != 0) {

				if (level == 4) {

#ifdef WFORCE_VAO_COLORBLEED_OFF
					sampleDepth = tex2Dlod(depthNormalsTexture4, float4(sampleScreenSpacePosition.xy, 0, 0));
#else
					DecodeDepthNormal(tex2Dlod(depthNormalsTexture4, float4(sampleScreenSpacePosition, 0.0f, 0.0f)), sampleDepth, sampleViewSpaceNormal);
#endif

				}
				else if (level == 2) {

#ifdef WFORCE_VAO_COLORBLEED_OFF
					sampleDepth = tex2Dlod(depthNormalsTexture2, float4(sampleScreenSpacePosition.xy, 0, 0));
#else
					DecodeDepthNormal(tex2Dlod(depthNormalsTexture2, float4(sampleScreenSpacePosition, 0.0f, 0.0f)), sampleDepth, sampleViewSpaceNormal);
#endif

				}
				else {
#ifdef WFORCE_VAO_COLORBLEED_OFF
					getSampleDepthNormalTopLevel(sampleScreenSpacePosition, sampleDepth);
#else
					getSampleDepthNormalTopLevel(sampleScreenSpacePosition, sampleDepth, sampleViewSpaceNormal);
#endif
				}

			}
			else {

#ifdef WFORCE_VAO_COLORBLEED_OFF
				getSampleDepthNormalTopLevel(sampleScreenSpacePosition, sampleDepth);
#else
				getSampleDepthNormalTopLevel(sampleScreenSpacePosition, sampleDepth, sampleViewSpaceNormal);
#endif

			}

			if (sampleScreenSpacePosition.x < 0.0f || sampleScreenSpacePosition.x > 1.0f
				|| sampleScreenSpacePosition.y < 0.0f || sampleScreenSpacePosition.y > 1.0f) {

				// Handle out of screen areas manually (we can't set clamp color for built-in depth buffers)
				sampleDepth = 1.0f;

#ifdef WFORCE_VAO_COLORBLEED_ON
				sampleViewSpaceNormal = float3(0, 0, -1);
#endif
			}

			sampleDepth = -sampleDepth * farPlane;
		}

    float4 PerformObjectToClipPos(float4 vertex)
    {
      #if UNITY_VERSION >= 540
			 return UnityObjectToClipPos(vertex);
			#else
       //return mul(UNITY_MATRIX_MVP, vertex);
			 return UnityObjectToClipPos(vertex);
			#endif
    }

		// ========================================================================
		// Vertex shaders 
		// ========================================================================

		v2fShed vertShed(appdata_img v)
		{
			v2fShed o;
	
			o.pos = PerformObjectToClipPos(v.vertex);
			#ifdef UNITY_SINGLE_PASS_STEREO
				o.uv = UnityStereoScreenSpaceUVAdjust(v.texcoord, _CameraDepthNormalsTexture_ST);
			#else
				o.uv = TRANSFORM_TEX(v.texcoord, _CameraDepthNormalsTexture);
			#endif

			#if UNITY_UV_STARTS_AT_TOP
			if (_MainTex_TexelSize.y < 0)
				o.uv.y = 1.0f - o.uv.y;
			#endif
				
			#ifdef UNITY_SINGLE_PASS_STEREO
				float2 tempUV1 = float2(o.uv.x * 2.0f, o.uv.y);
				float2 tempUV2 = float2(o.uv.x * 2.0f - 1.0f, o.uv.y);
				o.shed = mul(invProjMatrix, float4(tempUV1 * 2.0f - 1.0f, 1.0f, 1.0f));
				o.shed /= o.shed.w;
				o.shed2 = mul(invProjMatrix, float4(tempUV2 * 2.0f - 1.0f, 1.0f, 1.0f));
				o.shed2 /= o.shed2.w;
			#else
				o.shed = mul(invProjMatrix, float4(o.uv* 2.0f - 1.0f, 1.0f, 1.0f));
				o.shed /= o.shed.w;
			#endif

			return o;
		}
			
		v2fSingle vertSingle(appdata_img v)
		{
			v2fSingle o;
			o.pos = PerformObjectToClipPos(v.vertex);

			#ifdef UNITY_SINGLE_PASS_STEREO
				o.uv = UnityStereoScreenSpaceUVAdjust(v.texcoord, _CameraDepthNormalsTexture_ST);
			#else
				o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
			#endif
			return o;
		}

		v2fDouble vertDouble(appdata_img v)
		{
			v2fDouble o;
		  o.pos = PerformObjectToClipPos(v.vertex);

			#ifdef UNITY_SINGLE_PASS_STEREO
			float2 temp = UnityStereoScreenSpaceUVAdjust(v.texcoord, _CameraDepthNormalsTexture_ST);
			#else
			float2 temp = TRANSFORM_TEX(v.texcoord, _MainTex);
			#endif
			o.uv[0] = temp;
			o.uv[1] = temp;
				
			#if UNITY_UV_STARTS_AT_TOP
			if (_MainTex_TexelSize.y < 0)
				o.uv[1].y = 1.0f - o.uv[1].y;
			#endif

			#if UNITY_VERSION < 560
			#ifdef WFORCE_VAO_OPENGL_OFF
			if (flipY != 0) {
				o.uv[0].y = 1.0f - o.uv[1].y;
			}
			#endif
			#endif

			return o;
		}

		v2fDouble vertDoubleTexCopy(appdata_img v)
		{
			v2fDouble o;
			o.pos = v.vertex;

			#ifdef UNITY_SINGLE_PASS_STEREO
			float2 temp = UnityStereoTransformScreenSpaceTex(v.texcoord);
			#else
			float2 temp = v.texcoord;
			#endif
			
			o.uv[0] = temp;
			o.uv[1] = temp;
				
			#if UNITY_UV_STARTS_AT_TOP
			if (_MainTex_TexelSize.y < 0)
				o.uv[1].y = 1.0f - o.uv[1].y;
			#endif

			#if UNITY_VERSION < 560
			#ifdef WFORCE_VAO_OPENGL_OFF
			if (flipY != 0) {
				o.uv[0].y = o.uv[1].y;
			}
			#endif
			#endif

			return o;
		}

		// ========================================================================
		// Fragment Shaders 
		// ========================================================================

		WFORCE_VAO_MAIN_PASS_RETURN_TYPE vao(v2fShed i, int isCullingPrepass, int cullingPrepassType, int adaptive, float4 kernel[70], int kernelLength) {
			float pixelDepth = 0.0f;
			float sampleDepth = 0.0f;
			float3 pixelViewSpaceNormal = float3(0.0f, 0.0f, 0.0f);
			float3 sampleViewSpaceNormal = float3(0.0f, 0.0f, 0.0f);
			float kernelWeight = 0.0f;
			int start = 0;
			float distanceFalloffFactor = 0.0f;
			float acc = 0.0f;
			int ii = 0;
			float aoEstimation = 0.0f;
			
			WFORCE_VAO_MAIN_PASS_RETURN_TYPE whiteResult = WFORCE_VAO_WHITE;

			#ifndef WFORCE_VAO_COLORBLEED_OFF
				float3 gi = float3(0.0f, 0.0f, 0.0f);
				float giCount = 0.0f;
			#endif
			
			// Greedy Culling pre-pass check
			if (isCullingPrepass == 0) {

				if (cullingPrepassType != 0) {
					aoEstimation = tex2Dlod(cullingPrepassTexture, float4(i.uv, 0, 0)).r;
					// Greedy culling prepass
					if (cullingPrepassType == 1 && aoEstimation >= 1.0f) return whiteResult;
				}
			}
			
			float2 rotationAngleCosSin = tex2Dlod(noiseTexture, float4(i.uv * noiseTexelSizeRcp, 0.0f, 0.0f));
			
			fetchDepthNormal(i.uv, pixelDepth, pixelViewSpaceNormal);
			
			if (pixelDepth > subpixelRadiusCutoffDepth) return whiteResult; 
			
			float3 pixelViewSpacePosition = (i.shed.rgb * pixelDepth);
			#ifdef UNITY_SINGLE_PASS_STEREO
			if (i.uv.x > .5f) {
				pixelViewSpacePosition = (i.shed2.rgb * pixelDepth);
			}
			#endif
				
			// Careful culling prepass  check and adaptive sampling
			if (isCullingPrepass == 0) {

				if (cullingPrepassType == 2 && aoEstimation >= 1.0f) {
					// Careful culling prepass
					start = fourSamplesStartIndex;
					kernelLength = 4;
				} else {

					// Adaptive sampling
					if (adaptive != 0) {

						int adaptiveLevel = (int) (max(0.0f, min((((pixelViewSpacePosition.z - adaptiveMin) / (adaptiveMax - adaptiveMin)) ) * 15.0f, 15.0f)));
						adaptiveLevel = max(minLevelIndex, adaptiveLevel);
						kernelLength = adaptiveLengths[adaptiveLevel];
						start = adaptiveStarts[adaptiveLevel];
					}
				}
			}
			
			// Hierarchical buffer level
			int mipLevel = 1;

			if (hierarchicalBufferEnabled != 0) {
				if (pixelDepth < quarterResBufferMaxDistance)
					mipLevel = 4;
				else if (pixelDepth < halfResBufferMaxDistance)
					mipLevel = 2;
			}

			float3 tangentSphereCenterViewSpacePosition = pixelViewSpacePosition + (pixelViewSpaceNormal * halfRadius);

			// Distance falloff
			if (minRadiusEnabled && tangentSphereCenterViewSpacePosition.z < minRadiusCutoffDepth) {
				distanceFalloffFactor = smoothstep(minRadiusCutoffDepth, minRadiusCutoffDepth - minRadiusSoftness, tangentSphereCenterViewSpacePosition.z);
				if (distanceFalloffFactor == 1.0f) return whiteResult;
			}

			// Max radius limit
			if (maxRadiusEnabled && tangentSphereCenterViewSpacePosition.z > maxRadiusCutoffDepth) {
				radius = getRadiusForDepthAndScreenRadius(pixelViewSpacePosition.z, maxRadiusOnScreen);
				halfRadius = 0.5f * radius;
				halfRadiusSquared = halfRadius * halfRadius;
			}
			
			float2x2 rotationMatrix = float2x2(rotationAngleCosSin.x, rotationAngleCosSin.y, -rotationAngleCosSin.y, rotationAngleCosSin.x);

			float farPlane = getFarPlane(UseCameraFarPlane);
			int upTo = min(16, kernelLength);
			int j = start;
			int end = start + kernelLength;

			#if SHADER_API_D3D9	
			for (j = start; j < start + kernelLength; j++) {
			#else 

			#ifdef WFORCE_VAO_OPENGL_OFF
			for (j = start; j < start + kernelLength; j+=16) {
			#else
			while (ii + j <= end) {
			#endif
					
				[unroll(16)]
				for (ii = 0; ii < upTo; ii++) {
			#endif
			
					float2 sam = kernel[ii + j];
				
					float3 sampleViewSpacePosition = tangentSphereCenterViewSpacePosition + (float3(mul(rotationMatrix,(sam.xy * halfRadius)), 0));
					float2 sampleScreenSpacePosition = ((sampleViewSpacePosition.xy * screenProjection.xy + 
									sampleViewSpacePosition.z * screenProjection.zw) / sampleViewSpacePosition.z) + 0.5f;

          #if UNITY_VERSION >= 540
			        sampleScreenSpacePosition = UnityStereoScreenSpaceUVAdjust(sampleScreenSpacePosition, _CameraDepthNormalsTexture_ST);
  			  #else                                                                                                                        			
              sampleScreenSpacePosition = TRANSFORM_TEX(sampleScreenSpacePosition, _CameraDepthNormalsTexture);  					
  			  #endif
									
					#ifdef WFORCE_VAO_COLORBLEED_OFF
						getSampleDepthNormal(sampleScreenSpacePosition, farPlane, mipLevel, sampleDepth);
					#else
						getSampleDepthNormal(sampleScreenSpacePosition, farPlane, mipLevel, sampleDepth, sampleViewSpaceNormal);
					#endif

					float3 ray = normalize(sampleViewSpacePosition);
					float tca = dot(tangentSphereCenterViewSpacePosition, ray);
					float d2 = dot(tangentSphereCenterViewSpacePosition, tangentSphereCenterViewSpacePosition) - tca * tca; 
					float diff = halfRadiusSquared - d2;
					if (diff < 0.0f) continue;
					float thc = sqrt(diff); 
					float entryDepth = tca - thc; 
					float exitDepth = tca + thc; 

					entryDepth = (entryDepth*ray).z;
					exitDepth = (exitDepth*ray).z;

					float pipeLength = entryDepth - exitDepth;

					kernelWeight += pipeLength;

					if (sampleDepth > entryDepth) {
						acc += pipeLength * max(0.0f, (1.0f - (0.5f * halfRadius / (max(0.5f, (sampleDepth - entryDepth))))));
					}
					else if (sampleDepth < exitDepth) {
						acc += pipeLength;
					}
					else {
						acc += entryDepth - sampleDepth;
					}

					// Colorbleeding calculation
					#ifdef WFORCE_VAO_COLORBLEED_ON
						
						float3 sampleGi = float3(0.0f, 0.0f, 0.0f);
						float sampleGiCount = 0.0f;

						calculateColorBleed(sampleScreenSpacePosition, sampleDepth, pixelViewSpaceNormal, sampleViewSpaceNormal, sampleViewSpacePosition, pixelViewSpacePosition, farPlane, ii, j, sampleGi, sampleGiCount);
						
						gi += sampleGi;
						giCount += sampleGiCount;

					#endif

			#ifndef SHADER_API_D3D9	
				}
				#ifndef WFORCE_VAO_OPENGL_OFF
				j+= 16;
				#endif
			#endif

			}

			// Finish early in culling pre-pass
			if (isCullingPrepass != 0) {

				#ifdef WFORCE_VAO_COLORBLEED_OFF

						if (acc == kernelWeight) 
							return WFORCE_VAO_WHITE;
						else
							return WFORCE_VAO_BLACK;
				#else
					
					float prepassResult = 1.0f;
					
					if (acc != kernelWeight) 
						prepassResult = 0.0f;

					if (giCount > 0.0f) gi = gi / giCount; 

					if (gi.r < 1.0f || gi.g < 1.0f || gi.b < 1.0f) 
						prepassResult = 0.0f;

					return half4(prepassResult, 1.0f, 1.0f, 1.0f);
				#endif
			}

			if (kernelWeight > 0.0f){
				acc = acc / kernelWeight;
			} else {
				acc = 1.0f;
			}

			float accFullPresence = 1.0f - sqrt(1.0f-(acc*acc));
			if (acc > 0.999f) accFullPresence = 1.0f;

			// Presence
			acc = lerp(acc, accFullPresence, aoPresence);

			// Power
			acc = pow(acc, aoPower);

			// Distance falloff			
			acc = lerp(acc, 1.0f, distanceFalloffFactor);
			
			#ifdef WFORCE_VAO_COLORBLEED_ON
				if (giCount > 0.0f) gi = gi / giCount; 
				gi = pow(gi, giPower); 
			#endif

			// Luma sensitivity
			#ifdef WFORCE_VAO_COLORBLEED_OFF

				if (isLumaSensitive != 0) {
					if (hwBlendingEnabled != 0) {
						float3 mainColor = tex2Dlod(_MainTex, float4(i.uv, 0, 0)).rgb;
						
						if (useLogBufferInput != 0)
							mainColor = -log2(mainColor);

						float accIn = acc;
						applyLuma(mainColor, accIn, acc);
					}
				}
				
			#endif

			#ifdef WFORCE_VAO_COLORBLEED_OFF
				#ifdef SHADER_API_D3D9
					return half4(acc, pixelDepth, 1, 1);
				#else
					return half2(acc, pixelDepth);
				#endif
			#else
				return half4(gi, acc);
			#endif
		}

		half4 mixing(float4 color, half4 giao)
		{	

			#ifdef WFORCE_VAO_COLORBLEED_ON
				half3 gi = giao.rgb;
			#endif

			float ao = giao.a;
			
			// Luma sensitivity
			if (isLumaSensitive != 0) {
				if (hwBlendingEnabled == 0) {
					float aoIn = ao;

					#ifdef WFORCE_VAO_COLORBLEED_ON
						float3 giIn = gi;
						applyLuma(color, aoIn, ao, giIn, gi);
					#else
						applyLuma(color, aoIn, ao);
					#endif
				}
			}

			#ifdef WFORCE_VAO_COLORBLEED_ON
			
				if (hwBlendingEnabled == 0) {
					gi = processGi(color, gi, ao);
				}

				if (outputAOOnly != 0) {
					return half4(gi, 1.0f);
				}

				color.rgb *= gi;
			#else

				if (outputAOOnly != 0) {
					color = half4(1.0f, 1.0f, 1.0f, 1.0f);
				}

				color.rgb *= ao+colorTint.rgb*(1.0f - ao);
			#endif

			if (useLogEmissiveBuffer != 0) {			
				// Encode for logarithmic emission buffer in LDR
				color.rgb = exp2(-color.rgb);
			}

			return color;
		}
		
		half4 mixingNoBlur(v2fDouble i)
		{	
			float4 mainColor = float4(1.0f, 1.0f, 1.0f, 1.0f);
			
			if (hwBlendingEnabled == 0 && useLogEmissiveBuffer == 0) {
				mainColor = tex2Dlod(_MainTex, float4(i.uv[0], 0.0f, 0.0f));
			}

			if (useLogEmissiveBuffer != 0) {			
				mainColor = tex2Dlod(emissionTexture, float4(i.uv[0], 0, 0)).rgba;
				mainColor.rgb = -log2(mainColor.rgb);
			}

			#ifdef WFORCE_VAO_COLORBLEED_ON
				return mixing(mainColor, tex2Dlod(textureAO, float4(i.uv[1], 0.0f, 0.0f)));
			#else
				return mixing(mainColor, half4(1.0f, 1.0f, 1.0f, tex2Dlod(textureAO, float4(i.uv[1], 0.0f, 0.0f)).r));
			#endif
		}

		half4 uniformBlur(v2fDouble input, const int increment)
		{

			#ifdef WFORCE_VAO_COLORBLEED_ON
				float4 acc = float4(0.0f, 0.0f, 0.0f, 0.0f);

				float weightGi = 4.0f;

				if (useFastBlur == 0) {
					weightGi = 9.0f;
				}

				float3 centerGi = tex2Dlod(textureAO, float4(input.uv[1], 0.0f, 0.0f)).rgb;
				float centerGiLuma = dot(float3(0.3f, 0.3f, 0.3f), centerGi);
			#else
				float acc = 0.0f;
			#endif

			float weight = 4.0f;

			if (useFastBlur == 0) {
				weight = 9.0f;
			}

			float farPlane = getFarPlane(UseCameraFarPlane);
			
			#ifdef WFORCE_VAO_COLORBLEED_OFF
				float centerDepth = -tex2Dlod(textureAO, float4(input.uv[1], 0, 0)).g * farPlane;
			#else
				float centerDepth = fetchDepth(input.uv[1], farPlane);
			#endif

			[unroll(3)]
			for (int i = -1; i <= 1; i+=increment) {
				[unroll(3)]
				for (int j = -1; j <= 1; j+=increment) {

					float2 offset = input.uv[1] + float2(float(i), float(j)) * texelSize;
					
					#ifdef WFORCE_VAO_COLORBLEED_ON

						float tapDepth = fetchDepth(offset, farPlane);

						half4 tapGiAO = tex2Dlod(textureAO, float4(offset, 0, 0));
						float tapLuma = dot(float3(0.3f, 0.3f, 0.3f), tapGiAO.rgb);
                        
						if (abs(tapDepth - centerDepth) > blurDepthThreshold) {
							weight -= 1.0f;
						} else {
							acc.a += tapGiAO.a;
						}

						if (giBlur != 3 && abs(centerGiLuma - tapLuma) > 0.2f) {
							weightGi -= 1.0f;
						} else {
							acc.rgb += tapGiAO.rgb;
						}

					#else
						
						float2 tap = tex2Dlod(textureAO, float4(offset, 0, 0));
						float tapDepth = -tap.g * farPlane;
					
						if (abs(tapDepth - centerDepth) > blurDepthThreshold) {
							weight -= 1.0f;
							continue;
						}

						acc += tap.r;

					#endif

				}
			}

			float result = 1.0f;

			half4 mainColor = half4(1.0f, 1.0f, 1.0f, 1.0f);
			if (hwBlendingEnabled == 0 && useLogEmissiveBuffer == 0) {
				mainColor = tex2Dlod(_MainTex, float4(input.uv[0], 0.0f, 0.0f));
			}
			
			if (useLogEmissiveBuffer != 0) {			
				mainColor = tex2Dlod(emissionTexture, float4(input.uv[0], 0, 0)).rgba;
				mainColor.rgb = -log2(mainColor.rgb);
			}

			#ifdef WFORCE_VAO_COLORBLEED_ON
				if (weight > 0.0f) result = acc.a / weight;
				float3 resultGi = float3(1.0f, 1.0f, 1.0f);
				if (weightGi > 0.0f) resultGi = acc.rgb / weightGi;
				return mixing(mainColor, half4(resultGi, result));
			#else
				if (weight > 0.0f) result = acc / weight;
				return mixing(mainColor, half4(1.0f, 1.0f, 1.0f, result));
			#endif
		}

		half4 enhancedBlur(v2fDouble input, int passIndex, const int blurSize)
		{
			int idx = 0;
			float2 offset;
			float weight = gaussWeight;
			float farPlane = getFarPlane(UseCameraFarPlane);

			#ifdef WFORCE_VAO_COLORBLEED_OFF
				float acc = 0.0f;
				float centerDepth01 = tex2Dlod(textureAO, float4(input.uv[1], 0, 0)).g;
				float centerDepth = -centerDepth01 * farPlane;
			#else
				float4 acc = float4(0.0f, 0.0f, 0.0f, 0.0f);
				float centerDepth = fetchDepth(input.uv[1], farPlane);
				float weightGi = enhancedBlurSize * 2.0f + 1.0f;
				float centerGiLuma = dot(float3(0.3f, 0.3f, 0.3f), tex2Dlod(textureAO, float4(input.uv[1], 0, 0)).rgb);
			#endif

			[unroll(17)]
			for (int i = -blurSize; i <= blurSize; ++i) {
				
				if (passIndex == 1) 
					offset = input.uv[1] + float2(float(i) * texelSize.x, 0.0f);
				else 
					offset = input.uv[1] + float2(0.0f, float(i) * texelSize.y);
			
				#ifdef WFORCE_VAO_COLORBLEED_OFF
					float2 tapSample = tex2Dlod(textureAO, float4(offset, 0, 0));
					float tapDepth = -tapSample.g * farPlane;
					float tapAO = tapSample.r;
				#else
					float4 tapSample = tex2Dlod(textureAO, float4(offset, 0, 0));
					float tapDepth = fetchDepth(offset, farPlane);
					float tapAO = tapSample.a;
				#endif

                if (abs(tapDepth - centerDepth) < blurDepthThreshold) {
					acc += tapAO * gauss[idx].x;
				} else {
					weight -= gauss[idx].x;
				}

				#ifdef WFORCE_VAO_COLORBLEED_ON
					float tapLuma = dot(float3(0.3f, 0.3f, 0.3f), tapSample.rgb);

					if (giBlur != 3 && abs(tapLuma - centerGiLuma) > 0.2f) {
						weightGi -= 1.0f;
					} else {
						acc.rgb += tapSample.rgb;
					}
				#endif

				idx++;
			}

			float result = 1.0f;

			#ifdef WFORCE_VAO_COLORBLEED_OFF
				if (weight > 0.0f) result = acc / weight;
			#else
				float3 resultGi = float3(1.0f, 1.0f, 1.0f);
				if (weightGi > 0.0f) resultGi = acc.rgb / weightGi;
				if (weight > 0.0f) result = acc.a / weight;
			#endif
			
			if (passIndex == 2) {

				float4 mainColor = float4(1.0f, 1.0f, 1.0f, 1.0f);
			
				if (hwBlendingEnabled == 0 && useLogEmissiveBuffer == 0) {
					mainColor = tex2Dlod(_MainTex, float4(input.uv[0], 0.0f, 0.0f));
				}

				if (useLogEmissiveBuffer != 0) {			
					mainColor = tex2Dlod(emissionTexture, float4(input.uv[0], 0, 0)).rgba;
					mainColor.rgb = -log2(mainColor.rgb);
				}

				#ifdef WFORCE_VAO_COLORBLEED_OFF
					return mixing(mainColor, half4(1.0f, 1.0f, 1.0f, result));
				#else
					return mixing(mainColor, half4(resultGi, result));
				#endif
			} else {
				#ifdef WFORCE_VAO_COLORBLEED_OFF
					return half4(result, centerDepth01, 0.0f, 0.0f);
				#else
					return half4(resultGi, result);
				#endif
			}
			
		}

		half4 selectEnhancedBlur(v2fDouble input, int passIndex)
		{
			if (enhancedBlurSize == 1)
				return enhancedBlur(input, passIndex, 1);

			if (enhancedBlurSize == 2)
				return enhancedBlur(input, passIndex, 2);

			if (enhancedBlurSize == 3)
				return enhancedBlur(input, passIndex, 3);

			if (enhancedBlurSize == 4)
				return enhancedBlur(input, passIndex, 4);

			if (enhancedBlurSize == 5)
				return enhancedBlur(input, passIndex, 5);

			if (enhancedBlurSize == 6)
				return enhancedBlur(input, passIndex, 6);

			if (enhancedBlurSize == 7)
				return enhancedBlur(input, passIndex, 7);

			return enhancedBlur(input, passIndex, 8);
		}

		half4 blendLogGbuffer3(v2fDouble i) 
		{
			float3 mainColor = tex2Dlod(emissionTexture, float4(i.uv[0], 0, 0)).rgba;
			mainColor = -log2(mainColor);
			mainColor *= tex2Dlod(occlusionTexture, float4(i.uv[0], 0.0f, 0.0f)).r;
			return half4(exp2(-mainColor.rgb), 1.0f);
		}

		AoOutput blendBeforeReflections(v2fDouble i) 
		{
			AoOutput output;

			output.albedoAO = half4(0.0f, 0.0f, 0.0f, tex2Dlod(occlusionTexture, float4(i.uv[0], 0.0f, 0.0f)).r);
			output.emissionLighting = tex2Dlod(occlusionTexture, float4(i.uv[0], 0.0f, 0.0f)).rrrr;
			
			return output;
		}

		AoOutput blendBeforeReflectionsLog(v2fDouble i) 
		{
			AoOutput output;

			float occlusion = tex2Dlod(occlusionTexture, float4(i.uv[0], 0.0f, 0.0f)).r;

			output.albedoAO = half4(0.0f, 0.0f, 0.0f, occlusion);
			
			float3 mainColor = tex2Dlod(emissionTexture, float4(i.uv[0], 0, 0)).rgba;
			mainColor = -log2(mainColor);
			mainColor *= occlusion;
			output.emissionLighting = half4(exp2(-mainColor.rgb), 1.0f);
			
			return output;
		}

		half4 blendAfterLightingLog(v2fDouble i) 
		{
			float occlusion = tex2Dlod(occlusionTexture, float4(i.uv[0], 0.0f, 0.0f)).r;
			float3 mainColor = tex2Dlod(emissionTexture, float4(i.uv[0], 0, 0)).rgba;

			mainColor = -log2(mainColor);
			mainColor *= occlusion;

			return half4(exp2(-mainColor.rgb), 1.0f);
		}

	ENDCG
	SubShader
	{
		Cull Off ZWrite Off ZTest Always
		
		// 0 - Culling prepass
		Pass{CGPROGRAM
			#pragma vertex vertShed #pragma fragment frag
			WFORCE_VAO_MAIN_PASS_RETURN_TYPE frag(v2fShed i) : SV_Target { return vao(i, 1, 0, 0, eightSamples, 8); }
			ENDCG}

		// 1 - Main pass
		Pass{CGPROGRAM
			#pragma vertex vertShed #pragma fragment frag
			WFORCE_VAO_MAIN_PASS_RETURN_TYPE frag(v2fShed i) : SV_Target { return vao(i, 0, cullingPrepassMode, adaptiveMode, samples, sampleCount); }
			ENDCG}
			
		// 2 - StandardBlurUniform
		Pass{CGPROGRAM
			#pragma vertex vertDouble #pragma fragment frag
			half4 frag(v2fDouble input) : SV_Target { return uniformBlur(input, 1); }
			ENDCG}

		// 3 - StandardBlurUniformMultiplyBlend
		Pass{Blend DstColor Zero // Multiplicative
			CGPROGRAM
			#pragma vertex vertDouble #pragma fragment frag
			half4 frag(v2fDouble input) : SV_Target { return uniformBlur(input, 1); }
			ENDCG}

		// 4 - StandardBlurUniformFast
		Pass{CGPROGRAM
			#pragma vertex vertDouble #pragma fragment frag
			half4 frag(v2fDouble input) : SV_Target { return uniformBlur(input, 2); }
			ENDCG}

		// 5 - StandardBlurUniformFastMultiplyBlend
		Pass{Blend DstColor Zero // Multiplicative
			CGPROGRAM
			#pragma vertex vertDouble #pragma fragment frag
			half4 frag(v2fDouble input) : SV_Target { return uniformBlur(input, 2); }
			ENDCG}

		// 6 - EnhancedBlurFirstPass
		Pass{CGPROGRAM
			#pragma vertex vertDouble #pragma fragment frag
			half4 frag(v2fDouble i) : SV_Target { return selectEnhancedBlur(i, 1); }
			ENDCG}

		// 7 - EnhancedBlurSecondPass
		Pass{CGPROGRAM
			#pragma vertex vertDouble #pragma fragment frag
			half4 frag(v2fDouble i) : SV_Target { return selectEnhancedBlur(i, 2); }
			ENDCG}

		// 8 - EnhancedBlurSecondPassMultiplyBlend
		Pass{Blend DstColor Zero // Multiplicative
			CGPROGRAM
			#pragma vertex vertDouble #pragma fragment frag
			half4 frag(v2fDouble i) : SV_Target { return selectEnhancedBlur(i, 2); }
			ENDCG}

		// 9 - Mixing
		Pass{CGPROGRAM
			#pragma vertex vertDouble #pragma fragment frag
			half4 frag(v2fDouble i) : SV_Target { return mixingNoBlur(i); }
			ENDCG}

		// 10 - MixingMultiplyBlend
		Pass{Blend DstColor Zero // Multiplicative
			CGPROGRAM
			#pragma vertex vertDouble #pragma fragment frag
			half4 frag(v2fDouble i) : SV_Target { return mixingNoBlur(i); }
			ENDCG}

		// 11 - BlendBeforeReflections
		Pass{Blend 0 Zero One, Zero SrcAlpha // Multiply destination alpha by source alpha
			Blend 1 DstColor Zero, Zero One // Multiplicative
			CGPROGRAM
			#pragma vertex vertDoubleTexCopy #pragma fragment frag
			AoOutput frag(v2fDouble i) { return blendBeforeReflections(i); }
			ENDCG}

		// 12 - BlendBeforeReflectionsLog
		Pass{Blend 0 Zero One, Zero SrcAlpha // Multiply destination alpha by source alpha
			Blend 1 One Zero // Overwrite
			CGPROGRAM
			#pragma vertex vertDoubleTexCopy #pragma fragment frag
			AoOutput frag(v2fDouble i) { return blendBeforeReflectionsLog(i); }
			ENDCG}

		// 13 - DownscaleDepthNormalsPass
		Pass{CGPROGRAM
			#pragma vertex vertDouble #pragma fragment frag
			half4 frag(v2fDouble i) : SV_Target { return downscaleDepthNormals(i); }
			ENDCG}
			
		// 14 - Copy
		Pass{CGPROGRAM
			#pragma vertex vertDouble #pragma fragment frag
			half4 frag(v2fDouble i) : SV_Target { return tex2Dlod(_MainTex, float4(i.uv[1],0,0)); }
			ENDCG}

		// 15 - BlendAfterLightingLog
		Pass{CGPROGRAM
			#pragma vertex vertDoubleTexCopy #pragma fragment frag
			half4 frag(v2fDouble i) : SV_Target { return blendAfterLightingLog(i); }
			ENDCG}
	}
}
