// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Crystal Glass/Refraction/Mesh Alpha" {
	Properties {
	}
	CGINCLUDE
		#include "UnityCG.cginc"
		struct v2f
		{
			float4 pos : SV_POSITION;
		};
		v2f vert (appdata_base v)
		{
			v2f o;
			o.pos = UnityObjectToClipPos(v.vertex);
			return o;
       	}
       	float4 frag (v2f i) : SV_TARGET
		{
			return float4(0, 0, 0, 0);
		}
	ENDCG
	SubShader {
		Tags { "RenderType" = "Opaque" "Queue" = "Geometry+1" }
		Pass {
			Cull Off
			ColorMask A
			
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			ENDCG
		}
	}
	FallBack Off
}