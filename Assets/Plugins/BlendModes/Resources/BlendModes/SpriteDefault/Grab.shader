// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "BlendModes/SpriteDefault/Grab"
{
	Properties
	{
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_Color ("Tint", Color) = (1,1,1,1)
		[MaterialToggle] PixelSnap ("Pixel snap", Float) = 0

	    [HideInInspector] _StencilRef ("Stencil Ref", Float) = 8
		[HideInInspector] _BlendStencilComp ("Blend Stencil Comp", Float) = 0
		[HideInInspector] _NormalStencilComp ("Normal Stencil Comp", Float) = 1
		[HideInInspector] _MaskStencilComp ("Mask Stencil Comp", Float) = 1
	}

	CGINCLUDE

	#include "UnityCG.cginc"			

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
		vertexOutput.Color = vertexInput.Color * _Color;
		#ifdef PIXELSNAP_ON
		vertexOutput.Vertex = UnityPixelSnap(vertexOutput.Vertex);
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
			"CanUseSpriteAtlas" = "True"
		}

		Cull Off
		Lighting Off
		ZWrite Off
		Fog { Mode Off }
		Blend SrcAlpha OneMinusSrcAlpha
		
		GrabPass { }
		
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
			#pragma multi_compile DUMMY PIXELSNAP_ON
			#pragma vertex ComputeVertex
			#pragma fragment ComputeFragment
			
			fixed4 ComputeFragment (VertexOutput vertexOutput) : SV_Target
			{
				fixed4 texColor = tex2D(_MainTex, vertexOutput.TexCoord) * vertexOutput.Color;
				//texColor.rgb *= texColor.a;
				
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
			#pragma multi_compile DUMMY PIXELSNAP_ON
			
			fixed4 ComputeFragment(VertexOutput vertexOutput) : SV_Target
			{
				fixed4 texColor = tex2D(_MainTex, vertexOutput.TexCoord) * vertexOutput.Color;
				//texColor.rgb *= color.a;

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
			#pragma multi_compile DUMMY PIXELSNAP_ON
			
			fixed4 ComputeFragment(VertexOutput vertexOutput) : SV_Target
			{
				fixed4 texColor = tex2D(_MainTex, vertexOutput.TexCoord) * vertexOutput.Color;
				//texColor.rgb *= color.a;
				clip(texColor.a - .01);

				return texColor;
			}
			
			ENDCG
		}
	}
	
	Fallback "Sprites/Default"
	CustomEditor "BmMaterialEditor"
}
