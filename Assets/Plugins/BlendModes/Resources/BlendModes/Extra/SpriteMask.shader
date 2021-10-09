// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'


// Sprite mask for selective blending.

Shader "BlendModes/Extra/SpriteMask" 
{
	Properties 
	{
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_Color ("Tint", Color) = (1,1,1,1)
		[MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
	}

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
		Colormask 0 
		ZWrite Off
		Fog { Mode Off }
		Blend SrcAlpha OneMinusSrcAlpha

		Stencil
		{
			Ref 8
			Comp Always
			Pass Replace
		}

		Pass 
		{  
			CGPROGRAM
			
			#include "UnityCG.cginc"
	
			#pragma vertex ComputeVertex
			#pragma fragment ComputeFragment
			
			sampler2D _MainTex;
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

			fixed4 ComputeFragment(VertexOutput vertexOutput) : SV_Target
			{
				fixed4 texColor = tex2D(_MainTex, vertexOutput.TexCoord) * vertexOutput.Color;
				clip(texColor.a - .01);

				return texColor;
			}
			
			ENDCG
		}
	}
}
