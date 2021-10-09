// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'


// Mesh mask for selective blending.

Shader "BlendModes/Extra/MeshMask" 
{
	Properties 
	{
		_Color ("Tint Color", Color) = (1,1,1,1)
		_MainTex ("Base (RGB)", 2D) = "white" {}
	}

	SubShader 
	{
		Tags 
		{ 
			"Queue" = "Transparent" 
			"RenderType" = "Transparent" 
		}
		
		Colormask 0 
		ZWrite Off
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
			
			fixed4 _Color;
			sampler2D _MainTex;
			float4 _MainTex_ST;

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
			
			VertexOutput ComputeVertex(VertexInput vertexInput)
			{
				VertexOutput vertexOutput;
				
				vertexOutput.Vertex = UnityObjectToClipPos(vertexInput.Vertex);
				vertexOutput.TexCoord = TRANSFORM_TEX(vertexInput.TexCoord, _MainTex);
				
				return vertexOutput;
			}

			fixed4 ComputeFragment(VertexOutput vertexOutput) : SV_Target
			{
				fixed4 texColor = tex2D(_MainTex, vertexOutput.TexCoord) * _Color;
				clip(texColor.a - .01);

				return texColor;
			}
			
			ENDCG
		}
	}
}
