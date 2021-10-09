// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// This shader doesn't actually has anything to do with blend modes,
// but could be useful when you want to tint overlappring sprites in specific color.
Shader "BlendModes/Extra/SpriteOverlapColor"
{
	Properties
	{
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_Color ("Tint", Color) = (1,1,1,1)
        _OverlapColor ("Overlap Tint", Color) = (.5,.5,.5,.5)
		[MaterialToggle] PixelSnap ("Pixel snap", Float) = 0

	    [HideInInspector] _StencilRef ("Stencil Ref", Float) = 8
		[HideInInspector] _BlendStencilComp ("Blend Stencil Comp", Float) = 3 // Nothing But Mask
		[HideInInspector] _NormalStencilComp ("Normal Stencil Comp", Float) = 6 // Normal Mask Behaviour
		[HideInInspector] _MaskStencilComp ("Mask Stencil Comp", Float) = 0 // Auto Mask Enabled
	}

	CGINCLUDE

	#include "UnityCG.cginc"			

	sampler2D _MainTex;
	fixed4 _Color;
    fixed4 _OverlapColor;
			
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
	};
			
	VertexOutput ComputeVertex (VertexInput vertexInput)
	{
		VertexOutput vertexOutput;
				
		vertexOutput.Vertex = UnityObjectToClipPos(vertexInput.Vertex);
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
		
		Pass
		{
			Name "BlendEffect"

			Stencil
            {
                Ref [_StencilRef]
                Comp [_BlendStencilComp]
            }

			CGPROGRAM
			
			#pragma target 3.0			
			#pragma multi_compile DUMMY PIXELSNAP_ON
			#pragma vertex ComputeVertex
			#pragma fragment ComputeFragment
			
			fixed4 ComputeFragment (VertexOutput vertexOutput) : SV_Target
			{
				fixed4 texColor = tex2D(_MainTex, vertexOutput.TexCoord) * vertexOutput.Color;
				//texColor.rgb *= texColor.a;
				
                texColor *= _OverlapColor;

				return texColor;
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
}
