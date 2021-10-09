// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Crystal Glass/Refraction/Skybox" {
	Properties {
		_CubeTex ("Environment Map", Cube) = "white" {}
	}
	CGINCLUDE
		#include "UnityCG.cginc"
		uniform samplerCUBE _CubeTex;
		struct v2f
		{
			float4 pos : SV_POSITION;
			float3 uvw : TEXCOORD0;
		};
		v2f vert (appdata_base v)
		{
			v2f o;
			o.pos = UnityObjectToClipPos(v.vertex);
            o.uvw = v.vertex.xyz;
			return o;
       	}
       	float4 frag (v2f i) : SV_TARGET
		{
			return float4(texCUBE(_CubeTex, i.uvw).rgb, 1);
		}
	ENDCG
	SubShader {
		Tags { "Queue" = "Background" "RenderType" = "Background" "PreviewType" = "Skybox" }
		Pass {
			ZWrite Off
			Cull Front
		
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			ENDCG
		}
	}
	FallBack Off
}