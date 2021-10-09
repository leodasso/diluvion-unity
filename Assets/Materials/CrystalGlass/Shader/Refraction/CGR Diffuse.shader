// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Crystal Glass/Refraction/Diffuse" {
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
			float3 lit : TEXCOORD1;
			float3 nor : TEXCOORD2;
			LIGHTING_COORDS(3, 4)
		};
		v2f vert (appdata_tan v)
		{
			TANGENT_SPACE_ROTATION;
			
			v2f o;
			o.pos = UnityObjectToClipPos(v.vertex);
			o.tex = TRANSFORM_TEX(v.texcoord, _MainTex);
			o.nor = mul(rotation, SCALED_NORMAL);
			o.lit = mul(rotation, ObjSpaceLightDir(v.vertex));
			TRANSFER_VERTEX_TO_FRAGMENT(o);
			return o;
       	}
       	float4 frag (v2f i) : SV_TARGET
		{
			float3 N = normalize(i.nor);
			float3 L = normalize(i.lit);
			float3 c = tex2D(_MainTex, i.tex).rgb;
			c *= (dot(N, L) * LIGHT_ATTENUATION(i));
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