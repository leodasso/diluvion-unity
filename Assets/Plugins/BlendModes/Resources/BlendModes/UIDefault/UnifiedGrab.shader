// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "BlendModes/UIDefault/UnifiedGrab"
{
	Properties
	{
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_Color ("Tint", Color) = (1,1,1,1)
		_ColorMask ("Color Mask", Float) = 15

		// Default UI stencil properties: Unity UI mask won't work, but needed to prevent warnings.
		_StencilComp ("Stencil Comparison", Float) = 8
		_Stencil ("Stencil ID", Float) = 0
		_StencilOp ("Stencil Operation", Float) = 0
		_StencilWriteMask ("Stencil Write Mask", Float) = 255
		_StencilReadMask ("Stencil Read Mask", Float) = 255

	    [HideInInspector] _StencilRef ("Stencil Ref", Float) = 8
		[HideInInspector] _BlendStencilComp ("Blend Stencil Comp", Float) = 0
		[HideInInspector] _NormalStencilComp ("Normal Stencil Comp", Float) = 1
		[HideInInspector] _MaskStencilComp ("Mask Stencil Comp", Float) = 1
	}

	CGINCLUDE

	#include "UnityCG.cginc"			

	sampler2D _MainTex;
	sampler2D _BmSharedGT;
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
			
	ENDCG

	SubShader
	{
		Tags
		{ 
			"Queue" = "Transparent" 
			"IgnoreProjector" = "True" 
			"RenderType" = "Transparent"
			"PreviewType" = "Plane"			
		}

		Cull Off
		Lighting Off
		ZWrite Off
		ZTest [unity_GUIZTestMode]
		Fog { Mode Off }
		Blend SrcAlpha OneMinusSrcAlpha
		ColorMask [_ColorMask]

		GrabPass { "_BmSharedGT" }

		Pass
		{
			Name "BlendEffect"

			Stencil
            {
                Ref [_StencilRef]
                Comp [_BlendStencilComp]
            }

			CGPROGRAM
			
			#include "../BlendModes.cginc"

			#pragma target 3.0			
			#pragma multi_compile BmDarken BmMultiply BmColorBurn BmLinearBurn BmDarkerColor BmLighten BmScreen BmColorDodge BmLinearDodge BmLighterColor BmOverlay BmSoftLight BmHardLight BmVividLight BmLinearLight BmPinLight BmHardMix BmDifference BmExclusion BmSubtract BmDivide BmHue BmSaturation BmColor BmLuminosity
			#pragma vertex ComputeVertex
			#pragma fragment ComputeFragment

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
				
				fixed4 grabColor = tex2D(_BmSharedGT, grabTexCoord); 
				
				#include "../BlendOps.cginc"

				return blendResult;
			}
			
			ENDCG
		}

		Pass 
		{  
			Name "NormalBlend"

			Stencil
            {
                Ref [_StencilRef]
                Comp [_NormalStencilComp]
            }

			CGPROGRAM
			
			#pragma vertex ComputeVertex
			#pragma fragment ComputeFragment
			
			fixed4 ComputeFragment(VertexOutput vertexOutput) : SV_Target
			{
				fixed4 texColor = tex2D(_MainTex, vertexOutput.TexCoord) * vertexOutput.Color;
				clip(texColor.a - .01);

				return texColor;
			}
			
			ENDCG
		}

		Pass 
		{  
			Name "AutoMask"
			Colormask 0 
			ZWrite Off

			Stencil
            {
                Ref [_StencilRef]
                Comp [_MaskStencilComp]
				Pass Replace
            }

			CGPROGRAM
			
			#pragma vertex ComputeVertex
			#pragma fragment ComputeFragment
			
			fixed4 ComputeFragment(VertexOutput vertexOutput) : SV_Target
			{
				fixed4 texColor = tex2D(_MainTex, vertexOutput.TexCoord) * vertexOutput.Color;
				clip(texColor.a - .01);

				return texColor;
			}
			
			ENDCG
		}
	}

	Fallback "UI/Default"
	CustomEditor "BmMaterialEditor"
}
