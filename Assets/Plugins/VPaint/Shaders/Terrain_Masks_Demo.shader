Shader "VPaint/Demo/Terrain_Masks"
{
	Properties 
	{
_MacroNormal("_MacroNormal", 2D) = "bump" {}
_Masks("_Masks", 2D) = "white" {}
_FresnelPower("_FresnelPower", Float) = 1
_Color01("_Color01", Color) = (1,1,1,1)
_Diffuse01("_Diffuse01", 2D) = "black" {}
_Diffuse01_Amount("_Diffuse01_Amount", Float) = 0
_Diffuse01_UVTiling("_Diffuse01_UVTiling", Float) = 1
_Color02("_Color02", Color) = (0.2745098,0.2345261,0.1474817,1)
_Diffuse02("_Diffuse02", 2D) = "black" {}
_Diffuse02_Amount("_Diffuse02_Amount", Float) = 0
_Diffuse02_UVTiling("_Diffuse02_UVTiling", Float) = 0
_Color03("_Color03", Color) = (0.3607843,0.3607843,0.3607843,1)
_Diffuse03("_Diffuse03", 2D) = "black" {}
_Diffuse03_Amount("_Diffuse03_Amount", Float) = 0
_Diffuse03_UVTiling("_Diffuse03_UVTiling", Float) = 0
_Color04("_Color04", Color) = (0.6268657,0.5683894,0.4397416,1)
_Diffuse04("_Diffuse04", 2D) = "black" {}
_Diffuse04_Amount("_Diffuse04_Amount", Float) = 0
_Diffuse04_UVTiling("_Diffuse04_UVTiling", Float) = 0

	}
	
	SubShader 
	{
		Tags
		{
"Queue"="Geometry"
"IgnoreProjector"="False"
"RenderType"="Opaque"

		}

		
Cull Back
ZWrite On
ZTest LEqual
ColorMask RGBA
Fog{
}


		CGPROGRAM
#pragma surface surf BlinnPhongEditor  vertex:vert
#pragma target 3.0


sampler2D _MacroNormal;
sampler2D _Masks;
float _FresnelPower;
float4 _Color01;
sampler2D _Diffuse01;
float _Diffuse01_Amount;
float _Diffuse01_UVTiling;
float4 _Color02;
sampler2D _Diffuse02;
float _Diffuse02_Amount;
float _Diffuse02_UVTiling;
float4 _Color03;
sampler2D _Diffuse03;
float _Diffuse03_Amount;
float _Diffuse03_UVTiling;
float4 _Color04;
sampler2D _Diffuse04;
float _Diffuse04_Amount;
float _Diffuse04_UVTiling;

			struct EditorSurfaceOutput {
				half3 Albedo;
				half3 Normal;
				half3 Emission;
				half3 Gloss;
				half Specular;
				half Alpha;
				half4 Custom;
			};
			
			inline half4 LightingBlinnPhongEditor_PrePass (EditorSurfaceOutput s, half4 light)
			{
half3 spec = light.a * s.Gloss;
half4 c;
c.rgb = (s.Albedo * light.rgb + light.rgb * spec);
c.a = s.Alpha;
return c;

			}

			inline half4 LightingBlinnPhongEditor (EditorSurfaceOutput s, half3 lightDir, half3 viewDir, half atten)
			{
				half3 h = normalize (lightDir + viewDir);
				
				half diff = max (0, dot ( lightDir, s.Normal ));
				
				float nh = max (0, dot (s.Normal, h));
				float spec = pow (nh, s.Specular*128.0);
				
				half4 res;
				res.rgb = _LightColor0.rgb * diff;
				res.w = spec * Luminance (_LightColor0.rgb);
				res *= atten * 2.0;

				return LightingBlinnPhongEditor_PrePass( s, res );
			}
			
			struct Input {
				float2 uv_Diffuse01;
float2 uv_Diffuse02;
float2 uv_Masks;
float2 uv_Diffuse03;
float2 uv_Diffuse04;
float4 color : COLOR;
float3 viewDir;
float2 uv_MacroNormal;

			};

			void vert (inout appdata_full v, out Input o) {
float4 VertexOutputMaster0_0_NoInput = float4(0,0,0,0);
float4 VertexOutputMaster0_1_NoInput = float4(0,0,0,0);
float4 VertexOutputMaster0_2_NoInput = float4(0,0,0,0);
float4 VertexOutputMaster0_3_NoInput = float4(0,0,0,0);


			}
			

			void surf (Input IN, inout EditorSurfaceOutput o) {
				o.Normal = float3(0.0,0.0,1.0);
				o.Alpha = 1.0;
				o.Albedo = 0.0;
				o.Emission = 0.0;
				o.Gloss = 0.0;
				o.Specular = 0.0;
				o.Custom = 0.0;
				
float4 Multiply3=(IN.uv_Diffuse01.xyxy) * _Diffuse01_UVTiling.xxxx;
float4 Tex2D1=tex2D(_Diffuse01,Multiply3.xy);
float4 Lerp6=lerp(_Color01,Tex2D1,_Diffuse01_Amount.xxxx);
float4 Multiply4=(IN.uv_Diffuse02.xyxy) * _Diffuse02_UVTiling.xxxx;
float4 Tex2D2=tex2D(_Diffuse02,Multiply4.xy);
float4 Lerp7=lerp(_Color02,Tex2D2,_Diffuse02_Amount.xxxx);
float4 Tex2D0=tex2D(_Masks,(IN.uv_Masks.xyxy).xy);
float4 Splat4=Tex2D0.x;
float4 Lerp3=lerp(Lerp6,Lerp7,Splat4);
float4 Multiply5=(IN.uv_Diffuse03.xyxy) * _Diffuse03_UVTiling.xxxx;
float4 Tex2D3=tex2D(_Diffuse03,Multiply5.xy);
float4 Lerp8=lerp(_Color03,Tex2D3,_Diffuse03_Amount.xxxx);
float4 Splat5=Tex2D0.y;
float4 Lerp4=lerp(Lerp3,Lerp8,Splat5);
float4 Multiply6=(IN.uv_Diffuse04.xyxy) * _Diffuse04_UVTiling.xxxx;
float4 Tex2D4=tex2D(_Diffuse04,Multiply6.xy);
float4 Lerp9=lerp(_Color04,Tex2D4,_Diffuse04_Amount.xxxx);
float4 Splat6=Tex2D0.z;
float4 Lerp5=lerp(Lerp4,Lerp9,Splat6);
float4 Splat7=Tex2D0.w;
float4 Multiply1=Lerp5 * Splat7;
float4 Multiply0=IN.color * float4( 1.2,1.2,1.2,1.2 );
float4 Multiply2=Multiply1 * Multiply0;
float4 Tex2DNormal0=float4(UnpackNormal( tex2D(_MacroNormal,(IN.uv_MacroNormal.xyxy).xy)).xyz, 1.0 );
float4 Fresnel0=(1.0 - dot( normalize( float4( IN.viewDir.x, IN.viewDir.y,IN.viewDir.z,1.0 ).xyz), normalize( Tex2DNormal0.xyz ) )).xxxx;
float4 Pow0=pow(Fresnel0,_FresnelPower.xxxx);
float4 Clamp0=clamp(Pow0,float4( 0,0,0,0 ),float4( 0.5,0.5,0.5,0.5 ));
float4 Multiply7=Multiply2 * Clamp0;
float4 Add0=Multiply2 + Multiply7;
float4 Master0_2_NoInput = float4(0,0,0,0);
float4 Master0_3_NoInput = float4(0,0,0,0);
float4 Master0_4_NoInput = float4(0,0,0,0);
float4 Master0_5_NoInput = float4(1,1,1,1);
float4 Master0_7_NoInput = float4(0,0,0,0);
float4 Master0_6_NoInput = float4(1,1,1,1);

o.Albedo = Add0;
o.Normal = normalize(Tex2DNormal0);

			}
		ENDCG
	}
	Fallback "Diffuse"
}