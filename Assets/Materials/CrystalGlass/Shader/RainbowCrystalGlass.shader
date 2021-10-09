// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Crystal Glass/Rainbow" {
	Properties {
		_EnvTex ("Environment Texture", CUBE) = "black" {}
		_RainbowTex ("Rainbow Texture", 2D) = "black" {}
		_RainbowInten ("Rainbow Intensity", Range(0, 1)) = 0.5
	}
	CGINCLUDE
		#include "UnityCG.cginc"
		
		uniform samplerCUBE _EnvTex;
		uniform sampler2D _RainbowTex;
		uniform float _RainbowInten;

		struct v2f
		{
			float4 pos : POSITION;
			float3 view : TEXCOORD0;  // world space view
			float3 norm : TEXCOORD1;  // world space normal
		};
		v2f vert (appdata_base v)
		{
			v2f o;
			o.pos = UnityObjectToClipPos(v.vertex);
			o.norm = mul((float3x3)unity_ObjectToWorld, v.normal);
			o.view = WorldSpaceViewDir(v.vertex);
			return o;
       	}
       	float4 frag (v2f i) : COLOR
		{
			float3 N = normalize(i.norm);
			float3 V = normalize(i.view);

			float vdn = dot(V, N);
			float3 rainbow = tex2D(_RainbowTex, float2(vdn, 0)).rgb;
			
			float3 r = reflect(-V, N);
			float3 refl = texCUBE(_EnvTex, r).rgb;
			float3 c = lerp(refl, rainbow, _RainbowInten * vdn);
			return float4(c, 1 - vdn);
		}
	ENDCG
	SubShader {
		Tags { "RenderType" = "Transparent" "Queue" = "Transparent" }
		Pass {
			Blend SrcAlpha OneMinusSrcAlpha
			
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest
			ENDCG
		}
	}
	FallBack "Diffuse"
}