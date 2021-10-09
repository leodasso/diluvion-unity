// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'


// Combines glass distortion and blend mode effects.
// Based on the MeshDefault Grab shader.

Shader "BlendModes/Extra/GlassDistortion" 
{
	Properties 
	{
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_Color ("Tint Color", Color) = (1,1,1,1)
		_BumpMap ("Bump Map", 2D) = "bump" {}
		_Distortion ("Distortion", range (0,128)) = 10
	}
	
	Category 
	{
		Tags 
		{ 
			"Queue" = "Transparent" 
			"RenderType" = "Opaque" 
		}

		SubShader 
		{		
			ZWrite Off
			Blend SrcAlpha OneMinusSrcAlpha 
		
			GrabPass 
			{		
				Name "BASE"					
				Tags { "LightMode" = "Always" }
			}
		
			Pass 
			{  
				Name "BASE"
				Tags { "LightMode" = "Always" }

				CGPROGRAM
			
				#include "UnityCG.cginc"
				#include "../BlendModes.cginc"

				#pragma target 3.0			
				#pragma multi_compile BmDarken BmMultiply BmColorBurn BmLinearBurn BmDarkerColor BmLighten BmScreen BmColorDodge BmLinearDodge BmLighterColor BmOverlay BmSoftLight BmHardLight BmVividLight BmLinearLight BmPinLight BmHardMix BmDifference BmExclusion BmSubtract BmDivide BmHue BmSaturation BmColor BmLuminosity
				#pragma vertex ComputeVertex
				#pragma fragment ComputeFragment
				#pragma fragmentoption ARB_precision_hint_fastest
			
				sampler2D _MainTex;
				float4 _MainTex_ST;
				fixed4 _Color;
				sampler2D _BumpMap;
				float4 _BumpMap_ST;
				float _Distortion;
				sampler2D _GrabTexture;
				float4 _GrabTexture_TexelSize;

				struct VertexInput 
				{
					float4 Vertex : POSITION;
					float2 TexCoord: TEXCOORD0;
				};

				struct VertexOutput 
				{
					float4 Vertex : POSITION;
					float4 GrabUV : TEXCOORD0;
					float2 BumpUV : TEXCOORD1;
					float2 MainUV : TEXCOORD2;
				};

				VertexOutput ComputeVertex(VertexInput vertexInput)
				{
					VertexOutput vertexOutput;

					vertexOutput.Vertex = UnityObjectToClipPos(vertexInput.Vertex);
					#if UNITY_UV_STARTS_AT_TOP
					float scale = -1.0;
					#else
					float scale = 1.0;
					#endif
					vertexOutput.GrabUV.xy = (float2(vertexOutput.Vertex.x, vertexOutput.Vertex.y * scale) + vertexOutput.Vertex.w) * 0.5;
					vertexOutput.GrabUV.zw = vertexOutput.Vertex.zw;
					vertexOutput.BumpUV = TRANSFORM_TEX( vertexInput.TexCoord, _BumpMap );
					vertexOutput.MainUV = TRANSFORM_TEX( vertexInput.TexCoord, _MainTex );

					return vertexOutput;
				}
			
				fixed4 ComputeFragment(VertexOutput vertexOutput) : SV_Target
				{
					half2 bump = UnpackNormal(tex2D(_BumpMap, vertexOutput.BumpUV)).rg; 
					float2 offset = bump * _Distortion * _GrabTexture_TexelSize.xy;
					vertexOutput.GrabUV.xy = offset * vertexOutput.GrabUV.z + vertexOutput.GrabUV.xy;
	
					fixed4 grabColor = tex2Dproj(_GrabTexture, UNITY_PROJ_COORD(vertexOutput.GrabUV));
					fixed4 texColor = tex2D(_MainTex, vertexOutput.MainUV) * _Color;
				
					#include "../BlendOps.cginc"

					return blendResult;
				}
			
				ENDCG
			}
		}

		// Fallback for older hardware.
		SubShader 
		{
			Blend DstColor Zero
			Pass 
			{
				Name "BASE"
				SetTexture [_MainTex] {	combine texture }
			}
		}
	}
	
	FallBack "Diffuse"
	CustomEditor "BmMaterialEditor"
}
