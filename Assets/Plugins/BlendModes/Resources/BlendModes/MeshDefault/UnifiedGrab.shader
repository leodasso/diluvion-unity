// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "BlendModes/MeshDefault/UnifiedGrab" 
{
	Properties 
	{
		_Color ("Tint Color", Color) = (1,1,1,1)
		_MainTex ("Base (RGB)", 2D) = "white" {}

	    [HideInInspector] _StencilRef ("Stencil Ref", Float) = 8
		[HideInInspector] _BlendStencilComp ("Blend Stencil Comp", Float) = 0
		[HideInInspector] _NormalStencilComp ("Normal Stencil Comp", Float) = 1
		[HideInInspector] _MaskStencilComp ("Mask Stencil Comp", Float) = 1
	}
	
	CGINCLUDE

	#include "UnityCG.cginc"			

	fixed4 _Color;
	sampler2D _MainTex;
	float4 _MainTex_ST;
	sampler2D _BmMeshSharedGT;

	struct VertexInput 
	{
		float4 Vertex : POSITION;
		float2 TexCoord : TEXCOORD0;
	};

	struct VertexOutput 
	{
		float4 Vertex : SV_POSITION;
		float2 TexCoord : TEXCOORD0;
		float4 ScreenPos : TEXCOORD1;
	};
			
	VertexOutput ComputeVertex(VertexInput vertexInput)
	{
		VertexOutput vertexOutput;
				
		vertexOutput.Vertex = UnityObjectToClipPos(vertexInput.Vertex);
		vertexOutput.ScreenPos = vertexOutput.Vertex;
		vertexOutput.TexCoord = TRANSFORM_TEX(vertexInput.TexCoord, _MainTex);
				
		return vertexOutput;
	}
			
	ENDCG

	SubShader 
	{
		Tags 
		{ 
			"Queue" = "Transparent" 
			"RenderType" = "Transparent" 
		}
		
		ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha 
		
		GrabPass { "_BmMeshSharedGT" }
		
		Pass 
		{  
			Name "BlendEffect"

			CGPROGRAM
			
			#include "../BlendModes.cginc"

			#pragma target 3.0			
			#pragma multi_compile BmDarken BmMultiply BmColorBurn BmLinearBurn BmDarkerColor BmLighten BmScreen BmColorDodge BmLinearDodge BmLighterColor BmOverlay BmSoftLight BmHardLight BmVividLight BmLinearLight BmPinLight BmHardMix BmDifference BmExclusion BmSubtract BmDivide BmHue BmSaturation BmColor BmLuminosity
			#pragma vertex ComputeVertex
			#pragma fragment ComputeFragment
			
			fixed4 ComputeFragment(VertexOutput vertexOutput) : SV_Target
			{
				fixed4 texColor = tex2D(_MainTex, vertexOutput.TexCoord) * _Color;
				
				float2 grabTexCoord = vertexOutput.ScreenPos.xy / vertexOutput.ScreenPos.w; 
				grabTexCoord.x = (grabTexCoord.x + 1.0) * .5;
				grabTexCoord.y = (grabTexCoord.y + 1.0) * .5; 
				#if UNITY_UV_STARTS_AT_TOP
				grabTexCoord.y = 1.0 - grabTexCoord.y;
				#endif
				
				fixed4 grabColor = tex2D(_BmMeshSharedGT, grabTexCoord); 
				
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
				return tex2D(_MainTex, vertexOutput.TexCoord) * _Color;
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
	
	FallBack "Diffuse"
	CustomEditor "BmMaterialEditor"
}
