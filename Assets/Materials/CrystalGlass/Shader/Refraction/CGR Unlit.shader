// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Crystal Glass/Refraction/Unlit" {
	Properties {
		_MainTex ("Main Tex", 2D) = "white" {}
	}
	CGINCLUDE
		#include "UnityCG.cginc"
		#include "Lighting.cginc"
		#include "AutoLight.cginc"
		uniform sampler2D _MainTex;
		uniform float4 _MainTex_ST;
		struct v2f
		{
			float4 pos : SV_POSITION;
			float2 tex : TEXCOORD0;
			LIGHTING_COORDS(1, 2)
		};
		v2f vert (appdata_tan v)
		{
			v2f o;
			o.pos = UnityObjectToClipPos(v.vertex);
			o.tex = TRANSFORM_TEX(v.texcoord, _MainTex);
			TRANSFER_VERTEX_TO_FRAGMENT(o);
			return o;
       	}
       	float4 frag (v2f i) : SV_TARGET
		{
			float3 c = tex2D(_MainTex, i.tex).rgb;
			c *= LIGHT_ATTENUATION(i);
			return float4(c, 1);
		}
	ENDCG
	SubShader {
		Tags { "RenderType" = "Opaque" "IgnoreProjector" = "True" "LightMode" = "ForwardBase" }
		Pass {
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fwdbase
			ENDCG
		}
	}
	FallBack Off
}