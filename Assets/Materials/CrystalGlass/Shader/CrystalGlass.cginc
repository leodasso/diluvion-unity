// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

#ifndef CRYSTAL_GLASS_INCLUDED
#define CRYSTAL_GLASS_INCLUDED

uniform samplerCUBE _EnvTex;
uniform float _IOR;
uniform float _IOROffset;
uniform float _FresnelPower;
uniform float _FresnelAlpha;
#ifdef CRYSTAL_GLASS_BUMP
	uniform sampler2D _NormalTex;
	uniform float4 _NormalTex_ST;
#endif			
			
struct v2f
{
	float4 pos : POSITION;
	float3 view : TEXCOORD0;  // world space view
	float3 norm : TEXCOORD1;  // world space normal
#ifdef CRYSTAL_GLASS_BUMP
	float2 tex : TEXCOORD4;
	float3 tan : TEXCOORD5;   // world space tangent
	float3 bin : TEXCOORD6;   // world space binormal
#endif
};
v2f vert (appdata_tan v)
{
	v2f o;
	o.pos = UnityObjectToClipPos(v.vertex);
	o.norm = mul((float3x3)unity_ObjectToWorld, v.normal);
	o.view = WorldSpaceViewDir(v.vertex);
#ifdef CRYSTAL_GLASS_BUMP
	o.tex = TRANSFORM_TEX(v.texcoord, _NormalTex);
	TANGENT_SPACE_ROTATION;
	o.tan = mul((float3x3)unity_ObjectToWorld, v.tangent.xyz);
	o.bin = mul((float3x3)unity_ObjectToWorld, binormal);
#endif
	return o;
}
float4 frag (v2f i) : COLOR
{
	float3 N = normalize(i.norm);
#ifdef CRYSTAL_GLASS_BUMP
	float3 bump = tex2D(_NormalTex, i.tex).rgb;
	bump = normalize(bump * 2.0 - 1.0);
	float3 T = normalize(i.tan);
	float3 B = normalize(i.bin);
	N = normalize(N + T * bump.x - B * bump.y);
#endif
	float3 V = normalize(i.view);
	float3 R = reflect(-V, N);
	float3 refl = texCUBE(_EnvTex, R).rgb;

	float3 tr = refract(V, N, _IOR + _IOROffset);
	float3 tg = refract(V, N, _IOR);
	float3 tb = refract(V, N, _IOR - _IOROffset);

	float3 refr;
	refr.r = texCUBE(_EnvTex, tr).r;
	refr.g = texCUBE(_EnvTex, tg).g;
	refr.b = texCUBE(_EnvTex, tb).b;

	float fresnel = saturate(-dot(N, -V));
	fresnel = pow(fresnel, _FresnelPower);

	float3 c = refl * fresnel + refr * (1 - fresnel);
	return float4(c, 1 - fresnel * _FresnelAlpha);
}

#endif