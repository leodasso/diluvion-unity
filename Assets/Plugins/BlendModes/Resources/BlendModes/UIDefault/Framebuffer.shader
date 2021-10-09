// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "BlendModes/UIDefault/Framebuffer"
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
	fixed4 _Color;
			
	struct VertexInput
	{
		float4 Vertex : POSITION;
		float2 TexCoord : TEXCOORD0;
	};

	struct VertexOutput
	{
		float4 Vertex : SV_POSITION;
		float2 TexCoord : TEXCOORD0;
	};
			
	VertexOutput ComputeVertex (VertexInput vertexInput)
	{
		VertexOutput vertexOutput;
				
		vertexOutput.Vertex = UnityObjectToClipPos(vertexInput.Vertex);
		vertexOutput.TexCoord = vertexInput.TexCoord;
		#ifdef UNITY_HALF_TEXEL_OFFSET
		vertexOutput.Vertex.xy += (_ScreenParams.zw - 1.0) * float2(-1.0, 1.0);
		#endif
							
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

			#pragma only_renderers framebufferfetch
			#pragma target 3.0			
			#pragma multi_compile BmDarken BmMultiply BmColorBurn BmLinearBurn BmDarkerColor BmLighten BmScreen BmColorDodge BmLinearDodge BmLighterColor BmOverlay BmSoftLight BmHardLight BmVividLight BmLinearLight BmPinLight BmHardMix BmDifference BmExclusion BmSubtract BmDivide BmHue BmSaturation BmColor BmLuminosity
			#pragma vertex ComputeVertex
			#pragma fragment ComputeFragment

			void ComputeFragment (VertexOutput vertexOutput, inout fixed4 bufferColor : SV_Target)
			{
			    fixed4 texColor = tex2D(_MainTex, vertexOutput.TexCoord) * _Color;
			    clip(texColor.a - .01);
				fixed4 grabColor = bufferColor;
				
				#include "../BlendOps.cginc"

				bufferColor = blendResult;
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
				fixed4 texColor = tex2D(_MainTex, vertexOutput.TexCoord) * _Color;
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
				fixed4 texColor = tex2D(_MainTex, vertexOutput.TexCoord) * _Color;
				clip(texColor.a - .01);

				return texColor;
			}
			
			ENDCG
		}
	}

	Fallback "UI/Default"
	CustomEditor "BmMaterialEditor"
}
