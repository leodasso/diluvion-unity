// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'


// Blend shader for UI objects with Unity UI masks support.

Shader "BlendModes/Extra/MaskableUI" 
{
	Properties
	{
		[PerRendererData] 
		_MainTex ("Sprite Texture", 2D) = "white" {}
		_Color ("Tint", Color) = (1,1,1,1)
		
		[HideInInspector] _StencilComp ("Stencil Comparison", Float) = 8
		[HideInInspector] _Stencil ("Stencil ID", Float) = 0
		[HideInInspector] _StencilOp ("Stencil Operation", Float) = 0
		[HideInInspector] _StencilWriteMask ("Stencil Write Mask", Float) = 255
		[HideInInspector] _StencilReadMask ("Stencil Read Mask", Float) = 255
		[HideInInspector] _ColorMask ("Color Mask", Float) = 15
	}

	SubShader
	{
		Tags
		{ 
			"Queue" = "Transparent" 
			"IgnoreProjector" = "True" 
			"RenderType" = "Transparent" 
			"PreviewType" = "Plane"		
		}
		
		Stencil
		{
			Ref [_Stencil]
			Comp [_StencilComp]
			Pass [_StencilOp] 
			ReadMask [_StencilReadMask]
			WriteMask [_StencilWriteMask]
		}

		Cull Off
		Lighting Off
		ZWrite Off
		ZTest [unity_GUIZTestMode]
		Fog { Mode Off }
		Blend SrcAlpha OneMinusSrcAlpha
		ColorMask [_ColorMask]

		GrabPass { }

		Pass
		{
			CGPROGRAM
			
			#include "UnityCG.cginc"
			#include "../BlendModes.cginc"

			#pragma target 3.0			
			#pragma multi_compile BmDarken BmMultiply BmColorBurn BmLinearBurn BmDarkerColor BmLighten BmScreen BmColorDodge BmLinearDodge BmLighterColor BmOverlay BmSoftLight BmHardLight BmVividLight BmLinearLight BmPinLight BmHardMix BmDifference BmExclusion BmSubtract BmDivide BmHue BmSaturation BmColor BmLuminosity
			#pragma vertex ComputeVertex
			#pragma fragment ComputeFragment
			
			sampler2D _MainTex;
			sampler2D _GrabTexture;
			fixed4 _Color;
			
			struct VertexInput
			{
				float4 Vertex : POSITION;
				fixed4 Color : COLOR;
				float2 TexCoord : TEXCOORD0;
			};

			struct VertexOutput
			{
				float4 Vertex : SV_POSITION;
				fixed4 Color : COLOR;
				float2 TexCoord : TEXCOORD0;
				float4 ScreenPos : TEXCOORD1;
			};
			
			VertexOutput ComputeVertex (VertexInput vertexInput)
			{
				VertexOutput vertexOutput;
				
				vertexOutput.Vertex = UnityObjectToClipPos(vertexInput.Vertex);
				vertexOutput.ScreenPos = vertexOutput.Vertex;	
				vertexOutput.TexCoord = vertexInput.TexCoord;
				#ifdef UNITY_HALF_TEXEL_OFFSET
				vertexOutput.Vertex.xy += (_ScreenParams.zw - 1.0) * float2(-1.0, 1.0);
				#endif
				vertexOutput.Color = vertexInput.Color * _Color;
							
				return vertexOutput;
			}

			fixed4 ComputeFragment (VertexOutput vertexOutput) : SV_Target
			{
				fixed4 texColor = tex2D(_MainTex, vertexOutput.TexCoord) * vertexOutput.Color;
				clip(texColor.a - .01);
				
				float2 grabTexCoord = vertexOutput.ScreenPos.xy / vertexOutput.ScreenPos.w; 
				grabTexCoord.x = (grabTexCoord.x + 1.0) * .5;
				grabTexCoord.y = (grabTexCoord.y + 1.0) * .5; 
				#if UNITY_UV_STARTS_AT_TOP
				grabTexCoord.y = 1.0 - grabTexCoord.y;
				#endif
				
				fixed4 grabColor = tex2D(_GrabTexture, grabTexCoord); 
				
				#include "../BlendOps.cginc"

				return blendResult;
			}
			
			ENDCG
		}
	}

	Fallback "UI/Default"
	CustomEditor "BmMaterialEditor"
}
